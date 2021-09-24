using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

using LogfileMetaAnalyser.Helpers;
using LogfileMetaAnalyser.Controls;
using LogfileMetaAnalyser.ExceptionHandling;

namespace LogfileMetaAnalyser.Datastore
{
    public class GeneralLogView : DatastoreBaseView
    {
        public GeneralLogView(TreeView navigationTree,
            Control.ControlCollection upperPanelControl,
            Control.ControlCollection lowerPanelControl,
            DataStore datastore,
            Exporter logfileFilterExporter) :
            base(navigationTree, upperPanelControl, lowerPanelControl, datastore, logfileFilterExporter)
        {
        }

        public override int SortOrder => 200;

        public override IEnumerable<string> Build()
        {
            string key = BaseKey;
            var dsref = Datastore.GetOrAdd<GeneralLogData>();
            var result = new List<string>();

            //Branch: General information
            TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, "General information", "information",
                GetBgColor(dsref.mostDetailedLogLevel.IsGreater(LogLevel.Info)));
            result.Add(key);


            //Branch: ErrorsAndWarnings
            if (dsref.MessageErrors.Count > 0 || dsref.MessageWarnings.Count > 0)
            {
                key = $"{BaseKey}/ErrorsAndWarnings";
                TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key,
                    $"message of type error and warning ({GetElementCount(key)})", "warning",
                    Constants.treenodeBackColorSuspicious);
                result.Add(key);
            }

            if (dsref.MessageErrors.Count > 0)
            {
                key = $"{BaseKey}/ErrorsAndWarnings/Errors";
                TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key,
                    $"message of type error ({GetElementCount(key)})", "error", Constants.treenodeBackColorSuspicious);
                result.Add(key);

                key = $"{BaseKey}/ErrorsAndWarnings/Errors/distinct";
                TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, $"message of type error distinct",
                    "error", Constants.treenodeBackColorSuspicious);
                result.Add(key);
            }

            if (dsref.MessageWarnings.Count > 0)
            {
                key = $"{BaseKey}/ErrorsAndWarnings/Warnings";
                TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key,
                    $"message of type warning ({GetElementCount(key)})", "warning",
                    Constants.treenodeBackColorSuspicious);
                result.Add(key);

                key = $"{BaseKey}/ErrorsAndWarnings/Warnings/distinct";
                TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, "message of type warning distinct",
                    "warning", Constants.treenodeBackColorSuspicious);
                result.Add(key);
            }

            //Branch: timestamp
            if (dsref.TimeGaps.Count > 0)
            {
                key = $"{BaseKey}/timegaps";
                TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key,
                    $"timestamp gaps ({GetElementCount(key)})", "warning", Constants.treenodeBackColorSuspicious);
                result.Add(key);
            }

            return result;
        }

        public override string BaseKey => $"{base.BaseKey}/GeneralLogView";

        public override int GetElementCount(string key)
        {
            var dsref = Datastore.GetOrAdd<GeneralLogData>();

            if (key == $"{BaseKey}/ErrorsAndWarnings")
                return dsref.MessageErrors.Count + dsref.MessageWarnings.Count;

            if (key == $"{BaseKey}/ErrorsAndWarnings/Errors")
                return dsref.MessageErrors.Count;

            if (key == $"{BaseKey}/ErrorsAndWarnings/Warnings")
                return dsref.MessageWarnings.Count;

            if (key == $"{BaseKey}/timegaps")
                return dsref.TimeGaps.Count;

            return 0;
        }

        public override void ExportView(string key)
        {
            if (!key.StartsWith(BaseKey))
                return;

            var dsref = Datastore.GetOrAdd<GeneralLogData>();


            if (key == BaseKey) //information
            {
                MultiListViewUC uc = new MultiListViewUC();
                ContextLinesUC contextLinesUc = new ContextLinesUC(LogfileFilterExporter);

                try
                {
                    uc.Suspend();

                    uc.SetupLayout(2);

                    uc[0].SetupCaption(
                        "General information about this analysis - attention: all information are captured after a read optimization (not really every single line is fully parsed!)");
                    uc[0].SetupHeaders(new string[] {"Attribute", "Value"});

                    uc[0].AddItemRow("gt1",
                        new string[]
                            {"overall log start (earliest event)", dsref.LogDataOverallTimeRangeStart.ToString("G")});
                    uc[0].AddItemRow("gt2",
                        new string[]
                            {"overall log end (latest event)", dsref.LogDataOverallTimeRangeFinish.ToString("G")});
                    uc[0].AddItemRow("gt3",
                        new string[]
                        {
                            "overall log time span ",
                            (dsref.LogDataOverallTimeRangeFinish - dsref.LogDataOverallTimeRangeStart).ToHumanString()
                        });
                    uc[0].AddItemRow("gl4",
                        new string[] {"most detailed log level", dsref.mostDetailedLogLevel.ToString()},
                        "", GetBgColor(dsref.mostDetailedLogLevel.IsGreater(LogLevel.Info)));

                    foreach (var kp in dsref.NumberOfEntriesPerLoglevel.OrderBy(t => (int)t.Value))
                        uc[0].AddItemRow("gnl" + kp.Key.ToString(), new string[]
                        {
                            $"number of log entries for log level '{kp.Key.ToString()}'",
                            kp.Value.ToString("n0")
                        });

                    foreach (var kp in dsref.NumberOflogSources.OrderBy(t => t.Key))
                        uc[0].AddItemRow("gns" + kp.Key.ToString(), new string[]
                        {
                            $"number of entries for log message source '{kp.Key.ToString()}'",
                            kp.Value.ToString("n0")
                        });


                    uc[1].SetupCaption("Analyzed files");
                    uc[1].SetupHeaders(new string[]
                    {
                        "File name", "Log level", "Start", "End", "Duration", "File size", "Bytes read (opt.)",
                        "Cnt lines read (opt.)", "Cnt messages read (opt.)", "avg chars per line",
                        "avg chars per block message", "avg lines per block message"
                    });

                    foreach (var kp in dsref.LogfileInformation)
                        uc[1].AddItemRow(kp.Key, new string[]
                        {
                            kp.Value.filename,
                            kp.Value.mostDetailedLogLevel.ToString(),
                            kp.Value.logfileTimerange_Start.ToString("G"),
                            kp.Value.logfileTimerange_Finish.ToString("G"),
                            (kp.Value.logfileTimerange_Finish - kp.Value.logfileTimerange_Start).ToHumanString(),
                            FileHelper.ToHumanBytes(kp.Value.filesize),
                            FileHelper.ToHumanBytes(kp.Value.charsRead),
                            kp.Value.cntLines.ToString(),
                            kp.Value.cntBlockMsgs.ToString(),
                            kp.Value.avgCharsPerLine.ToString(1),
                            kp.Value.avgCharsPerBlockmsg.ToString(1),
                            kp.Value.avgLinesPerBlockmsg.ToString(1)
                        });


                    uc[1].ItemClicked += new ListViewItemSelectionChangedEventHandler(
                        (object o, ListViewItemSelectionChangedEventArgs args) =>
                        {
                            try
                            {
                                string k = args.Item.Name;
                                var firstmsg = dsref.LogfileInformation[k].firstMessage;

                                if (firstmsg != null)
                                    contextLinesUc.SetData(firstmsg);
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Instance.HandleException(e);
                            }
                        });

                    UpperPanelControl.Add(uc);
                    LowerPanelControl.Add(contextLinesUc);
                }
                finally
                {
                    uc.Resume();
                }
            } //gen. information


            if (key.StartsWith(BaseKey + "/ErrorsAndWarnings"))
            {
                ListViewUC uc = new ListViewUC();
                ContextLinesUC contextLinesUC = new ContextLinesUC(LogfileFilterExporter);
                List<DatastoreBaseDataPoint> data = new List<DatastoreBaseDataPoint>();
                string caption = "";

                try
                {
                    uc.Suspend();


                    if (key == BaseKey + "/ErrorsAndWarnings")
                    {
                        caption = "Errors and Warnings";
                        data = dsref.MessageErrors.Union(dsref.MessageWarnings).ToList();
                    }
                    else if (key.StartsWith(BaseKey + "/ErrorsAndWarnings/Errors"))
                    {
                        caption = "Error and Ciritcal messages";
                        data = dsref.MessageErrors;
                    }
                    else if (key.StartsWith(BaseKey + "/ErrorsAndWarnings/Warnings"))
                    {
                        caption = "Warning messages";
                        data = dsref.MessageWarnings;
                    }

                    if (!key.Contains("/distinct"))
                    {
                        uc.SetupCaption($"{caption} ({data.Count})");
                        uc.SetupHeaders(new string[] {"Type", "Timestamp", "Message"});

                        foreach (var item in data)
                            uc.AddItemRow(item.uuid,
                                new string[]
                                {
                                    item.metaData,
                                    item.dtTimestamp.ToString("G"),
                                    item.message.messageText
                                });
                    }
                    else
                    {
                        var distGrps = data.GroupBy(m => m.message.payloadMessageDevalued).ToArray();

                        uc.SetupCaption($"{caption} ({distGrps.Length})");
                        uc.SetupHeaders(new string[]
                            {"Type", "Timestamp min", "Timestamp max", "Message count", "Distinct message"});

                        foreach (var grp in distGrps)
                            uc.AddItemRow(grp.Min(k => k.uuid),
                                new string[]
                                {
                                    grp.First().metaData,
                                    grp.Min(t => t.dtTimestamp).ToString("G"),
                                    grp.Max(t => t.dtTimestamp).ToString("G"),
                                    grp.Count().ToString(),
                                    grp.Key
                                });
                    }

                    if (uc.HasData())
                    {
                        uc.ItemClicked += new ListViewItemSelectionChangedEventHandler(
                            (object o, ListViewItemSelectionChangedEventArgs args) =>
                            {
                                try
                                {
                                    string k = args.Item.Name;
                                    var msg = dsref.MessageErrors.Union(dsref.MessageWarnings)
                                        .FirstOrDefault(t => t.uuid == k);
                                    if (msg != null)
                                        contextLinesUC.SetData(msg.message);
                                }
                                catch (Exception e)
                                {
                                    ExceptionHandler.Instance.HandleException(e);
                                }
                            }
                        );

                        UpperPanelControl.Add(uc);
                        LowerPanelControl.Add(contextLinesUC);
                    }
                }
                finally
                {
                    uc.Resume();
                }
            } // if (key.StartsWith("2.geninfo/ErrorsAndWarnings"))


            if (key == BaseKey + "/timegaps")
            {
                ListViewUC uc = new ListViewUC();

                try
                {
                    uc.Suspend();

                    ContextLinesUC contextLinesUC = new ContextLinesUC(LogfileFilterExporter);

                    uc.SetupCaption("encountered time gaps and jumps");
                    uc.SetupHeaders(new string[]
                        {"File name", "Time gmp start", "Time gap end", "Time gap duration", "Position in log file"});

                    foreach (var gap in dsref.TimeGaps.OrderBy(t => t.durationSec))
                        uc.AddItemRow(gap.uuid, new string[]
                        {
                            gap.logfileNameStart,
                            gap.dtTimestampStart.ToString("G"),
                            gap.dtTimestampEnd.ToString("G"),
                            gap.DurationString(true),
                            gap.logfilePositionStart.ToString()
                        });

                    if (uc.HasData())
                    {
                        uc.ItemClicked += new ListViewItemSelectionChangedEventHandler(
                            (object o, ListViewItemSelectionChangedEventArgs args) =>
                            {
                                try
                                {
                                    string k = args.Item.Name;

                                    var msg = dsref.TimeGaps.FirstOrDefault(t => t.uuid == k);
                                    if (msg != null)
                                        contextLinesUC.SetData(msg.message);
                                }
                                catch (Exception e)
                                {
                                    ExceptionHandler.Instance.HandleException(e);
                                }
                            }
                        );

                        UpperPanelControl.Add(uc);
                        LowerPanelControl.Add(contextLinesUC);
                    }
                }
                finally
                {
                    uc.Resume();
                }
            }
        }
    }
}