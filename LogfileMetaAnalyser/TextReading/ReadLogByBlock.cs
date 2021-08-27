using System;
using System.Collections.Generic; 
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser
{
    public class ReadLogByBlock : IDisposable
    {
        private static int maxLinesPerMessage = 1024*64; //when a text file without timestamp markers is parsed single messaes cannot be recognized, so we need an upper limit to break a message

        private string filename;
        private StreamReader readerstream;
        private long currentmessageNumber = 0;
        private long currentLineNumber = 0;
        private bool disposed = false;
        private long filesize = 0;
        private long filesizeDone = 0;
        private long filesizeDoneLast = 0;
        private float progressPercentLast = 0;

        private TextMessage bufferReadAhead = null;

        private AdaptiveStatisticalStopKeyAnalyser adaptiveStatisticalStopKeyAnalyser;

        private int contextMessageNumber;
        private Queue<TextMessage> contextQueuePast = new Queue<TextMessage>();

        private Queue<TextMessage> contextQueueFuture = new Queue<TextMessage>();
        
        public event EventHandler<ReadLogByBlockEventArgs> OnProgressChanged;

        public Encoding CurrentEncoding
        {
            get { return readerstream != null ? readerstream.CurrentEncoding : Encoding.Default; }
        }

        public bool HasMoreMessages
        {
            get
            {
                if (disposed)
                    return false;

                if (contextQueueFuture.Any())
                    return true;

                if (currentmessageNumber == 0 && contextQueueFuture.Count == 0) //start phase, no line was ever loaded
                    return true;

                if (contextMessageNumber > 0)  //the contextQueueFuture is only valid to be checked, if context messages are to be collected 
                    return contextQueueFuture.Any();

                if (readerstream != null)  //finally, check the stream itself
                    return (!readerstream.EndOfStream);

                return (false);
            }
        }


        public ReadLogByBlock(string filename, string messageInsignificantStopTermRegexString, int contextMessageNumber = 24, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException("Filename must not be empty!");

            if (!System.IO.File.Exists(filename))
                throw new ArgumentNullException($"Filname {filename} does not exist or cannot be found!");

            this.filename = filename; 

            var filestream = File.OpenRead(filename);

            if (!filestream.CanRead)
            {
                filestream.Dispose();
                throw new FileLoadException($"Cannot read file: {filename}");
            }

            readerstream = new StreamReader(filestream, encoding ?? Encoding.UTF8, true, 2 * 1024 * 1024);
            
            this.contextMessageNumber = contextMessageNumber;
            this.filesize = (new FileInfo(filename)).Length;

            CheckLogfileSupportsTimestamps();

            adaptiveStatisticalStopKeyAnalyser = new AdaptiveStatisticalStopKeyAnalyser(messageInsignificantStopTermRegexString);
        }

        private void CheckLogfileSupportsTimestamps()
        {
            string s = string.Empty;
            int lines = 0;
            bool ok = false;

            //pre read 128 lines
            while (!readerstream.EndOfStream && lines < 128 && !ok)
            {
                s = readerstream.ReadLine();
                ok = Constants.regexTimeStampAtLinestart.IsMatch(s);
            }

            //logfiles has no timesstamps in the first 100 lines :(
            if (!ok)
            {
                Dispose();
                throw new Exception($"logfile {filename} does not provide ISO timestamp information on line start. Searched the first {lines} lines");
            }

            //reset stream
            readerstream.BaseStream.Seek(0, SeekOrigin.Begin);
            readerstream.DiscardBufferedData();
        }

        private async Task ProcessReadAhead()
        {
            bool reading = true;
            TextMessage currentElem = null;

            while (reading)
            {
                GlobalStopWatch.StartWatch("GetMessageFromDiskAsync()");
                currentElem = await GetMessageFromDiskAsync().ConfigureAwait(false);
                GlobalStopWatch.StopWatch("GetMessageFromDiskAsync()");

                reading = (currentElem != null);

                if (reading)
                    contextQueueFuture.Enqueue(currentElem);

                if (contextQueueFuture.Count > contextMessageNumber)
                    break;
            }
        }

        private void ProcessReadBehind(TextMessage currentElem)
        {
            if (contextMessageNumber <= 0)
                return;

            //move current elem into past queue
            contextQueuePast.Enqueue(currentElem);

            //clean past queue
            while (contextQueuePast.Count > contextMessageNumber)
                contextQueuePast.Dequeue();
        }

        public async Task<TextMessage> GetNextMessageAsync()
        {
            TextMessage dequeuedMsg = null;
                                    
            FireProgressEvent();

            while (HasMoreMessages) 
            {
                TextMessage currentPayloadMsg = null;
                 
                //fill context queue
                await ProcessReadAhead();

                //move from future queue into current element; if future queue drained out, we are done at all
                if (contextQueueFuture.Count > 0)
                {
                    dequeuedMsg = contextQueueFuture.Dequeue();
                    filesizeDone += dequeuedMsg.messageText.Length;

                    
                    if (dequeuedMsg.finalizeState != FinalizeStates.NotFinalized)
                    {
                        if (contextMessageNumber > 0)
                        {
                            currentPayloadMsg = dequeuedMsg.Clone();
                            currentPayloadMsg.contextMsgBefore = contextQueuePast.ToArray();
                            //currentPayloadMsg.contextMsgAfter = contextQueueFuture.ToArray();
                            currentPayloadMsg.contextMsgAfter = contextQueueFuture.Select(m => m.GetShrunkMessage()).ToArray(); //mem opt #1
                        }
                        else
                            currentPayloadMsg = dequeuedMsg;
                    }
                           
                    //move current elem into past queue                    
                    ProcessReadBehind(dequeuedMsg.GetShrunkMessage());  //mem opt.#2

                    if (dequeuedMsg.textLocator.messageNumber % 3000 == 0)
                        FireProgressEvent();

                    //only return finalized messages, all non finalized messages are to ignore (ref AdaptiveStatisticalStopKeyAnalyser)
                    if (dequeuedMsg.finalizeState != FinalizeStates.NotFinalized)
                        return currentPayloadMsg;
                }  
            }

            FireProgressEvent(true);
            Dispose();
            return null;
        }
                     

        private async Task<TextMessage> GetMessageFromDiskAsync()
        {
            if (disposed)
                return null;

            string s = string.Empty;
            int linesThisMessage = 0;
            long streamPos;
            bool startNewMsg = false;
            StringBuilder sb = new StringBuilder();

            TextMessage currentMsg = bufferReadAhead;
            streamPos = readerstream.BaseStream.Position;

            while (!readerstream.EndOfStream)
            {
                //read from stream
                GlobalStopWatch.StartWatch("ReadLineAsync()");
                s = await readerstream.ReadLineAsync();
                currentLineNumber++;
                GlobalStopWatch.StopWatch("ReadLineAsync()");


                //start a new message
                if (currentLineNumber == 1)
                {
                    linesThisMessage = 1;
                    currentMsg = new TextMessage(
                                        new TextLocator(filename, streamPos, currentLineNumber, ++currentmessageNumber),
                                        string.Format("{0}{1}", s, Environment.NewLine)
                                   );

                    streamPos = readerstream.BaseStream.Position;
                    continue;
                }

                //is the recently read message the beginning of a NEW message block?
                GlobalStopWatch.StartWatch("GetMessageFromDiskAsync.regexTimeStampAtLinestart.IsMatch()");
                if (Constants.regexTimeStampAtLinestart.IsMatch(s) || linesThisMessage >= maxLinesPerMessage)
                {
                    GlobalStopWatch.StopWatch("GetMessageFromDiskAsync.regexTimeStampAtLinestart.IsMatch()");

                    //save current new message to process the old message
                    bufferReadAhead = new TextMessage(
                                        new TextLocator(filename, streamPos, currentLineNumber, ++currentmessageNumber), 
                                        string.Format("{0}{1}", s, Environment.NewLine)
                                   );

                    //a new block starts, push out the last one if available
                    if (!string.IsNullOrEmpty(currentMsg.messageText))
                    {
                        GlobalStopWatch.StartWatch("GetMessageFromDiskAsync.adaptiveStatisticalStopKeyAnalyser");
                        var stopTermRes = adaptiveStatisticalStopKeyAnalyser.CheckMsg(currentMsg);
                        GlobalStopWatch.StopWatch("GetMessageFromDiskAsync.adaptiveStatisticalStopKeyAnalyser");

                        if (stopTermRes != AdaptiveStatisticalStopKeyAnalyserResult.StopKeyPostive)
                        {
                            currentMsg.FinalizeMessage(GetLogfileType());
                            SetLogfileTypeStat(false, currentMsg.messageLogfileType);
                        }
                        //else currentMsg.finalizeState = FinalizeStates.NotFinalized
                                               
                        //hmm, if this is the first (finalized) message, it might have had no timestamp at all
                        if (currentMsg.finalizeState != FinalizeStates.NotFinalized && currentMsg.messageTimestamp.IsNull())
                            startNewMsg = true;
                        else
                            return currentMsg;
                    }
                    else
                        startNewMsg = true;

                    if (startNewMsg)
                    {
                        //start a new message
                        linesThisMessage = 1;
                        currentMsg = bufferReadAhead;
                        startNewMsg = false;
                    }
                }
                else
                {
                    GlobalStopWatch.StopWatch("GetMessageFromDiskAsync.regexTimeStampAtLinestart.IsMatch()");

                    //just add the current line to the current stored message                   
                    currentMsg.AppendMessageLine(s);
                    linesThisMessage++; 
                }

                streamPos = readerstream.BaseStream.Position;
            }

            bufferReadAhead = null;

            //return last msg
            if (currentMsg != null)
            {

                var stopTermRes = adaptiveStatisticalStopKeyAnalyser.CheckMsg(currentMsg);

                if (stopTermRes != AdaptiveStatisticalStopKeyAnalyserResult.StopKeyPostive)
                {
                    currentMsg.FinalizeMessage(GetLogfileType());
                    SetLogfileTypeStat(false, currentMsg.messageLogfileType);
                }
                //else currentMsg.finalizeState = FinalizeStates.NotFinalized

                return currentMsg;
            }

            return null;
        }

        private LogfileType _expectedLogfileType = LogfileType.Undef;
        private Dictionary<LogfileType, int> _expectedLogfileTypeData = new Dictionary<LogfileType, int>();
        private void SetLogfileTypeStat(bool clear = false, LogfileType input = LogfileType.Undef)
        {
            if (clear)
            {
                _expectedLogfileType = LogfileType.Undef;
                _expectedLogfileTypeData.Clear();
            }
            else
            {
                if (_expectedLogfileType != LogfileType.Undef) //already found
                    return;

                //collect data
                _expectedLogfileTypeData.AddOrIncrease(input);

                //analyse result in case we've collected 10 data points
                if (_expectedLogfileTypeData.Values.Sum() >= 10)
                    _expectedLogfileType = _expectedLogfileTypeData.OrderByDescending(t => t.Value).First().Key;
            }
        }

        private LogfileType GetLogfileType()
        {
            return _expectedLogfileType;
        }


        public void Dispose()
        {
            if (!disposed)
            {
                readerstream.Close();
                readerstream.Dispose();

                contextQueuePast = null;
                contextQueueFuture = null;

                SetLogfileTypeStat(true);

                disposed = true;
            }
        }

        public void FireProgressEvent(bool force=false)
        {
            GlobalStopWatch.StartWatch("ReadLogByBlock.FireProgressEvent()");
            try
            {
                if (OnProgressChanged == null)
                    return;

                int delta = Convert.ToInt32(filesizeDone - filesizeDoneLast);
                float currentPercentDone = 100f * filesizeDone / filesize;

                //this GUI methdod is really expensive, do not call it too often! fire the even only in case 1MB of logfile was processed or 1.5% in progress was made
                if (!force && (delta <= 1048576) || (currentPercentDone - progressPercentLast) < 1.5f)
                    return;

                filesizeDoneLast = filesizeDone;
                progressPercentLast = currentPercentDone;

                OnProgressChanged(this, new ReadLogByBlockEventArgs()
                {
                    progressPercent = currentPercentDone,
                    sizeProcessedKb = System.Convert.ToInt32(filesizeDone / 1024f),
                    sizeProcessedDeltaKb = System.Convert.ToInt32(delta / 1024f)
                });
            }
            catch { }
            finally
            {
                GlobalStopWatch.StopWatch("ReadLogByBlock.FireProgressEvent()");
            }
        }
    }

    public class ReadLogByBlockEventArgs : EventArgs
    {
        public float progressPercent;
        public long sizeProcessedKb;
        public int sizeProcessedDeltaKb;
    }
}
