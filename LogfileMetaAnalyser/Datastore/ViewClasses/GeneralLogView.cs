using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

using LogfileMetaAnalyser.Helpers;
using LogfileMetaAnalyser.Controls;

namespace LogfileMetaAnalyser.Datastore
{
    public class GeneralLogView : DatastoreBaseView, IDatastoreView
    {
        public GeneralLogView() { }

        public override string BaseKey => $"{base.BaseKey}/GeneralLogView";

        public int GetElementCount(string key)
        {
            var dsref = datastore.generalLogData;

            if (key == $"{BaseKey}/ErrorsAndWarnings")
                return dsref.messageErrors.Count + dsref.messageWarnings.Count;

            if (key == $"{BaseKey}/ErrorsAndWarnings/Errors")
                return dsref.messageErrors.Count;

            if (key == $"{BaseKey}/ErrorsAndWarnings/Warnings")
                return dsref.messageWarnings.Count;

            if (key == $"{BaseKey}/timegaps")
                return dsref.timegaps.Count;

            return 0;
        }

        public void ExportView(string key)
        {
            if (!key.StartsWith(BaseKey))
                return;

            var dsref = datastore.generalLogData;


            if (key == BaseKey)  //information
            {
                MultiListViewUC uc = new MultiListViewUC();
                ContextLinesUC contextLinesUc = new ContextLinesUC(logfileFilterExporter);

                uc.SetupLayout(2);


                uc[0].SetupCaption("General information about this analysis - attention: all information are captured after a read optimization (not really every single line is fully parsed!)");
                uc[0].SetupHeaders(new string[] { "Attribute", "Value" });

                uc[0].AddItemRow("gt1", new string[] { "overall log start (earliest event)", dsref.logDataOverallTimeRange_Start.ToString("G") });
                uc[0].AddItemRow("gt2", new string[] { "overall log end (latest event)", dsref.logDataOverallTimeRange_Finish.ToString("G") });
                uc[0].AddItemRow("gt3", new string[] { "overall log time span ", (dsref.logDataOverallTimeRange_Finish - dsref.logDataOverallTimeRange_Start).ToHumanString() });
                uc[0].AddItemRow("gl4", new string[] { "most detailed log level", dsref.mostDetailedLogLevel.ToString() }, "", GetBgColor(dsref.mostDetailedLogLevel.IsGreater(Loglevels.Info)));

                foreach (var kp in dsref.numberOfEntriesPerLoglevel.OrderBy(t => (int)t.Value))
                    uc[0].AddItemRow("gnl" + kp.Key.ToString(), new string[]
                    {
                        $"number of log entries for log level '{kp.Key.ToString()}'",
                        kp.Value.ToString("n0")
                    });

                foreach (var kp in dsref.numberOflogSources.OrderBy(t => t.Key))
                    uc[0].AddItemRow("gns" + kp.Key.ToString(), new string[]
                    {
                        $"number of entries for log message source '{kp.Key.ToString()}'",
                        kp.Value.ToString("n0")
                    });


                uc[1].SetupCaption("Analyzed files");
                uc[1].SetupHeaders(new string[] { "File name", "Type", "Log level", "Start", "End", "Duration", "File size", "Bytes read (opt.)", "Cnt lines read (opt.)", "Cnt messages read (opt.)", "avg chars per line", "avg chars per block message", "avg lines per block message" });

                foreach (var kp in dsref.logfileInformation)
                    uc[1].AddItemRow(kp.Key, new string[] {
                        kp.Value.filename,
                        kp.Value.logfileType.ToString(),
                        kp.Value.mostDetailedLogLevel.ToString(),
                        kp.Value.logfileTimerange_Start.ToString("G"),
                        kp.Value.logfileTimerange_Finish.ToString("G"),
                        (kp.Value.logfileTimerange_Finish-kp.Value.logfileTimerange_Start).ToHumanString(),
                        FileHelper.ToHumanBytes(kp.Value.filesize),
                        FileHelper.ToHumanBytes(kp.Value.charsRead),
                        kp.Value.cntLines.ToString(),
                        kp.Value.cntBlockMsgs.ToString(),
                        kp.Value.avgCharsPerLine.ToString(1),
                        kp.Value.avgCharsPerBlockmsg.ToString(1),
                        kp.Value.avgLinesPerBlockmsg.ToString(1)
                    });


                uc[1].ItemClicked += new ListViewItemSelectionChangedEventHandler((object o, ListViewItemSelectionChangedEventArgs args) =>
                {
                    string k = args.Item.Name;
                    var firstmsg = dsref.logfileInformation[k].firstMessage;

                    if (firstmsg != null)
                        contextLinesUc.SetData(firstmsg);
                });

                uc.Resume();
                upperPanelControl.Add(uc);
                lowerPanelControl.Add(contextLinesUc);
            } //gen. information


            if (key.StartsWith(BaseKey + "/ErrorsAndWarnings"))
            {
                ListViewUC uc = new ListViewUC();
                ContextLinesUC contextLinesUC = new ContextLinesUC(logfileFilterExporter);
                List<DatastoreBaseDataPoint> data = new List<DatastoreBaseDataPoint>();
                string caption = "";

                if (key == BaseKey + "/ErrorsAndWarnings")
                {
                    caption = "Errors and Warnings";
                    data = dsref.messageErrors.Union(dsref.messageWarnings).ToList();
                }
                else if (key.StartsWith(BaseKey + "/ErrorsAndWarnings/Errors"))
                {
                    caption = "Error and Ciritcal messages";
                    data = dsref.messageErrors;
                }
                else if (key.StartsWith(BaseKey + "/ErrorsAndWarnings/Warnings"))
                {
                    caption = "Warning messages";
                    data = dsref.messageWarnings;
                }

                if (!key.Contains("/distinct"))
                {
                    uc.SetupCaption($"{caption} ({data.Count})");
                    uc.SetupHeaders(new string[] { "Type", "Timestamp", "Message" });

                    foreach (var item in data)
                        uc.AddItemRow(item.uuid,
                                      new string[] {
                                          item.metaData,
                                          item.dtTimestamp.ToString("G"),
                                          item.message.messageText
                                      });
                }
                else
                {
                    var distGrps = data.GroupBy(m => m.message.payloadmessageDevalued);

                    uc.SetupCaption($"{caption} ({distGrps.Count()})");
                    uc.SetupHeaders(new string[] { "Type", "Timestamp min", "Timestamp max", "Message count", "Distinct message" });

                    foreach (var grp in distGrps)
                        uc.AddItemRow(grp.Min(k => k.uuid),
                                        new string[] {
                                            grp.First().metaData,
                                            grp.Min(t => t.dtTimestamp).ToString("G"),
                                            grp.Max(t => t.dtTimestamp).ToString("G"),
                                            grp.Count().ToString(),
                                            grp.Key
                                        });
                }

                if (uc.HasData())
                {
                    uc.ItemClicked += new ListViewItemSelectionChangedEventHandler((object o, ListViewItemSelectionChangedEventArgs args) =>
                    {
                        string k = args.Item.Name;

                        var msg = dsref.messageErrors.Union(dsref.messageWarnings).FirstOrDefault(t => t.uuid == k);
                        if (msg != null)
                            contextLinesUC.SetData(msg.message);
                    }
                    );

                    uc.Resume();
                    upperPanelControl.Add(uc);
                    lowerPanelControl.Add(contextLinesUC);
                }
            }  // if (key.StartsWith("2.geninfo/ErrorsAndWarnings"))


            if (key == BaseKey + "/timegaps")
            {
                ListViewUC uc = new ListViewUC();
                ContextLinesUC contextLinesUC = new ContextLinesUC(logfileFilterExporter);

                uc.SetupCaption("encountered time gaps and jumps");
                uc.SetupHeaders(new string[] { "File name", "Time gmp start", "Time gap end", "Time gap duration", "Position in log file" });

                foreach (var gap in dsref.timegaps.OrderBy(t => t.durationSec))
                    uc.AddItemRow(gap.uuid, new string[] {
                        gap.logfileNameStart,
                        gap.dtTimestampStart.ToString("G"),
                        gap.dtTimestampEnd.ToString("G"),
                        gap.DurationString(true),
                        gap.logfilePositionStart.ToString()
                    });

                if (uc.HasData())
                {
                    uc.ItemClicked += new ListViewItemSelectionChangedEventHandler((object o, ListViewItemSelectionChangedEventArgs args) =>
                    {
                        string k = args.Item.Name;

                        var msg = dsref.timegaps.FirstOrDefault(t => t.uuid == k);
                        if (msg != null)
                            contextLinesUC.SetData(msg.message);
                    }
                    );

                    uc.Resume();
                    upperPanelControl.Add(uc);
                    lowerPanelControl.Add(contextLinesUC);
                }
            }

        }

    }
}
