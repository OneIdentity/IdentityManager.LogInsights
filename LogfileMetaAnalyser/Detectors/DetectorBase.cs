using System;

using LogfileMetaAnalyser.Datastore;

namespace LogfileMetaAnalyser.Detectors
{
    public enum TextReadMode
    {
        SingleMessage,  //a message can consists of several file rows, but only with one introducing header (e.g. timestamp)
        GroupMessage, //a group message is a group of singlemessages
        StreamGroupMessage //same as GroupMessage, but all messages are regarded within a specific stream (e.g. all messages of log source "ProjectorEngine")
    }


    public abstract class DetectorBase
    {
        public virtual DataStore _datastore { get; set; }

        public TextReadMode textReadMode = TextReadMode.SingleMessage;
        private TextMessageGroup msgGroup = new TextMessageGroup();
        private TextMessageGroupStream msgGroupStream = new TextMessageGroupStream();

        protected Datastore.DetectorStatistic detectorStats = new DetectorStatistic();

        protected bool isFinalizing = false;

        protected Helpers.NLog logger;

        public virtual DataStore datastore
        {
            set { _datastore = value; }
        }

        public virtual string identifier
        {
            get { return "#DetectorBase"; }
        }

        public virtual string caption
        {
            get { return ""; }
        }

        public virtual string category
        {
            get { return "<no category>"; }
        }

        protected bool _isEnabled;
        public virtual bool isEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        public virtual ILogDetector[] parentDetectors
        {
            set {}
        }

        public virtual string[] requiredParentDetectors
        {
            get
            {
                return new string[] { };
            }
        }



        public DetectorBase(TextReadMode textReadMode = TextReadMode.SingleMessage)
        {
            this.textReadMode = textReadMode;

            logger = new Helpers.NLog($"Detector.{identifier.Replace("#", "")}");
            logger.Info($"Starting detector with ReadMode = {textReadMode}");
        }

        protected TextMessage[] ProcessMessageBase(ref TextMessage msg)
        {
            //return output is a list of all collected text messages that were not yet processed (returned)
            if (isFinalizing && msg != null)
                return null;

            switch (textReadMode)
            {
                case TextReadMode.SingleMessage:
                    //just return the unchanged msg object as ref message; no return output as every msg was processed, no single messages needs to be buffered
                    return null; 


                case TextReadMode.GroupMessage:
                    if (msg == null) //special case, return all unclosed group messages 
                    {
                        TextMessage tm = msgGroup.GetGroupMessage();
                        if (tm == null)
                            return null;

                        return new TextMessage[] { tm };
                    }

                    //put the current msg into the group 
                    msgGroup.AppendMessage(msg);

                    if (msgGroup.IsGroupClosed())
                    {
                        //group is closed, transfer the group msg into msg object and put the current income into a new group msg
                        TextMessage msgOri = msg.Clone();
                        msg = msgGroup.GetGroupMessage();
                        
                        //start a new group
                        msgGroup = new TextMessageGroup(msgOri);
                    }
                    else //group is not yet closed, null the msg object
                        msg = null;

                    return null;


                case TextReadMode.StreamGroupMessage:
                    if (msg == null) //special case, return all unclosed group messages                    
                        return msgGroupStream.GetAllGroupMessages() ;
                    
                    msgGroupStream.AppendMessage(msg);

                    if (msgGroupStream.IsGroupClosed())
                    {
                        //group is closed, transfer the group msg into msg object and put the current income into a new group msg
                        TextMessage msgOri = msg.Clone();
                        msg = msgGroupStream.GetGroupMessage();

                        msgGroupStream.Clear(); //clear the last stream
                        msgGroupStream.AppendMessage(msgOri);
                    }
                    else //group is not yet closed, null the msg object
                        msg = null;

                    return null;
            }

            return null;
        }
        

        public override string ToString()
        {
            return string.Format("{0}; textReadMode={1}; required detector(s): {2}", identifier, textReadMode, requiredParentDetectors.Length == 0 ? "<none>" : string.Join("; ", requiredParentDetectors));
        }

    }
}
