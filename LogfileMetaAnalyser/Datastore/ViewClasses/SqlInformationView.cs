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
    internal class SqlInformationView : DatastoreBaseView
    {
        public SqlInformationView(TreeView navigationTree,
            Control.ControlCollection upperPanelControl,
            Control.ControlCollection lowerPanelControl,
            DataStore datastore,
            Exporter logfileFilterExporter) :
            base(navigationTree, upperPanelControl, lowerPanelControl, datastore, logfileFilterExporter)
        {
        }

        public override int SortOrder => 300;

        public override IEnumerable<string> Build()
        {
            string key = BaseKey;
            var dsref = Datastore.GetOrAdd<SqlInformation>();
            var result = new List<string>();

            TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, "General SQL session information", "information", Constants.treenodeBackColorNormal);
            result.Add(key);

            if (dsref.SqlSessions.Count > 0)
            {
                key = $"{BaseKey}/sessions";
                TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, $"General SQL session information ({GetElementCount(key)})", "information", Constants.treenodeBackColorNormal);
                result.Add(key);

                foreach (var session in dsref.SqlSessions.OrderBy(t => t.loggerSourceId))
                {
                    key = $"{BaseKey}/sessions/{session.uuid}";
                    TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, $"Sql session {session.ToString()}", (session.isSuspicious ? "warning" : "information"), GetBgColor(!session.isSuspicious));
                    result.Add(key);
                }
            }

            return result;
        }

        public override string BaseKey => $"{base.BaseKey}/genSqlInfo";

        public override int GetElementCount(string key)
        {
            if (key == $"{BaseKey}/sessions")
                return Datastore.GetOrAdd<SqlInformation>().numberOfSqlSessions;

            return 0;
        }

        public override void ExportView(string key)
        {
            if (!key.StartsWith(BaseKey))
                return;

            MultiListViewUC uc = new MultiListViewUC();
            ContextLinesUC contextLinesUc = new ContextLinesUC(LogfileFilterExporter);
            var dsref = Datastore.GetOrAdd<SqlInformation>();

            if (key == BaseKey)
            {
                uc.SetupLayout(1);
                uc[0].SetupCaption("general sql session information");
                uc[0].SetupHeaders(new string[] { "Attribute", "Value" });

                uc[0].AddItemRow("number", new string[] { "number of sql session found", dsref.numberOfSqlSessions.ToString() });
                uc[0].AddItemRow("isSuspicious", new string[] { "is suspicious", dsref.isSuspicious ? "yes; please check some of the sql session marked" : "seems to be all fine" });
                uc[0].AddItemRow("threshold_suspicious_duration_SqlCommand_msec", new string[] { "defined config value: threshold_suspicious_duration_SqlCommand_msec", dsref.ThresholdSuspiciousDurationSqlCommandMsec.ToString() });
                uc[0].AddItemRow("threshold_suspicious_duration_SqlTransaction_min", new string[] { "defined config value: threshold_suspicious_duration_SqlTransaction_sec", dsref.ThresholdSuspiciousDurationSqlTransactionSec.ToString() });

                contextLinesUc = null;
            }

            if (key == BaseKey + "/sessions")
            {
                uc.SetupLayout(1);

                uc[0].SetupCaption("general sql session information - session list");
                uc[0].SetupHeaders(new string[] { "Session id", "Is suspicious", "Session start", "Session ends", "Session duration", "Transaction count", "Top duration transaction", "Long running statement count", "Top duration statement" });

                foreach (var s in dsref.SqlSessions)
                    uc[0].AddItemRow(s.uuid, new string[] {
                        s.loggerSourceId,
                        s.isSuspicious ? "yes, pls check" : "all fine",
                        s.dtTimestampStart.ToString("G"),
                        s.dtTimestampEnd.ToString("G"),
                        (s.dtTimestampEnd - s.dtTimestampStart).ToHumanString(),
                        s.transactions.Count.ToString(),
                        s.transactions.Count == 0 ? "-" : s.transactions.OrderByDescending(t => t.durationMin).First().GetLabel(),
                        s.longRunningStatements.Count.ToString(),
                        s.longRunningStatements.Count == 0 ? "-" : s.longRunningStatements.OrderByDescending(t => t.durationMsec).First().GetLabel()
                    });

                uc[0].ItemClicked += new ListViewItemSelectionChangedEventHandler((object o, ListViewItemSelectionChangedEventArgs args) =>
                {
                    try
                    {
                        string uuid = args.Item.Name;
                        var tra = dsref.SqlSessions.FirstOrDefault(t => t.uuid == uuid);

                        if (tra != null && tra.message != null)
                            contextLinesUc.SetData(tra.message);
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Instance.HandleException(e);
                    }
                });
            }

            if (key.StartsWith(BaseKey + "/sessions/"))
            {
                string uuid = key.Substring((BaseKey + "/sessions/").Length);
                var sess = dsref.SqlSessions.FirstOrDefault(t => t.uuid == uuid);
                if (sess != null)
                {
                    uc.SetupLayout(3);

                    //titles and headers
                    uc[0].SetupCaption("sql session information for id " + sess.loggerSourceId);
                    uc[0].SetupHeaders(new string[] { "Attribute", "Value" });

                    uc[1].SetupCaption("long running statements");
                    uc[1].SetupHeaders(new string[] { "Timestamp", "Locator", "Duration [ms]", "Logger id", "Command", "Involved tables", "Statement text" });

                    uc[2].SetupCaption("transacount list");
                    uc[2].SetupHeaders(new string[] { "Start", "End", "Duration", "Start locator", "End locator", "State", "Logger id" });

                    //fill lists with data
                    uc[0].AddItemRow("firstOccurrence", new string[] { "first occurrence", sess.dtTimestampStart.ToString("G") });
                    uc[0].AddItemRow("lastOccurrence", new string[] { "last occurrence", sess.dtTimestampEnd.ToString("G") });
                    uc[0].AddItemRow("isSuspicious", new string[] { "is suspicious", sess.isSuspicious.ToString() }, "", GetBgColor(!sess.isSuspicious));
                    uc[0].AddItemRow("logfilename", new string[] { "logfile name", sess.logfileName });

                    foreach (var longrunner in sess.longRunningStatements)
                        uc[1].AddItemRow(sess.uuid, new string[]
                        {
                            longrunner.dtTimestampStart.ToString("G"),
                            longrunner.GetEventLocator(),
                            longrunner.durationMsec.ToString(),
                            longrunner.loggerSourceId,
                            longrunner.sqlCmdType.ToString(),
                            string.Join(", ", longrunner.assignedTablenames),
                            longrunner.statementText
                        });

                    foreach (var trans in sess.transactions)
                        uc[2].AddItemRow(trans.uuid, new string[]
                        {
                            trans.dtTimestampStart.ToString("G"),
                            trans.dtTimestampEnd.ToString("G"),
                            trans.DurationString(true),
                            trans.GetEventLocatorStart(),
                            trans.GetEventLocatorEnd(),
                            trans.isSuccessFullTransaction ? "COMMIT" : "ROLLBACK/TERMINATION",
                            trans.loggerSourceId
                        }, "", GetBgColor(trans.isSuccessFullTransaction));

                    //click events
                    //show the session start lines
                    uc[0].ItemClicked += new ListViewItemSelectionChangedEventHandler((object o, ListViewItemSelectionChangedEventArgs args) =>
                    {
                        try
                        {
                            //string k = args.Item.Name;
                            var sqlsess = dsref.SqlSessions.FirstOrDefault(t => t.uuid == uuid);

                            var msg = sqlsess.message;
                            if (msg != null)
                                contextLinesUc.SetData(msg);
                        }
                        catch (Exception e)
                        {
                            ExceptionHandler.Instance.HandleException(e);
                        }
                    });

                    //show the long running statement
                    uc[1].ItemClicked += new ListViewItemSelectionChangedEventHandler((object o, ListViewItemSelectionChangedEventArgs args) =>
                    {
                        try
                        {
                            string k = args.Item.Name;
                            var statement = dsref.SqlSessions.FirstOrDefault(t => t.uuid == uuid).longRunningStatements.FirstOrDefault(st => st.uuid == k);

                            if (statement != null && statement.message != null)
                                contextLinesUc.SetData(statement.message);
                        }
                        catch (Exception e)
                        {
                            ExceptionHandler.Instance.HandleException(e);
                        }
                    });

                    //show the transaction
                    uc[2].ItemClicked += new ListViewItemSelectionChangedEventHandler((object o, ListViewItemSelectionChangedEventArgs args) =>
                    {
                        try
                        {
                            string k = args.Item.Name;
                            var tra = dsref.SqlSessions.FirstOrDefault(t => t.uuid == uuid).transactions.FirstOrDefault(st => st.uuid == k);

                            if (tra != null && tra.message != null)
                                contextLinesUc.SetData(tra.message, tra.messageEnd);
                        }
                        catch (Exception e)
                        {
                            ExceptionHandler.Instance.HandleException(e);
                        }
                    });
                }
            }

            if (uc != null && uc.HasData())
            {
                uc.Resume();
                UpperPanelControl.Add(uc);
            }

            if (contextLinesUc != null)
                LowerPanelControl.Add(contextLinesUc);
        }
    }
}
