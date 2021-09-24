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
    internal class ProjectionActivitiesView : DatastoreBaseView
    {
        public ProjectionActivitiesView(TreeView navigationTree, 
            Control.ControlCollection upperPanelControl,
            Control.ControlCollection lowerPanelControl, 
            DataStore datastore, 
            Exporter logfileFilterExporter) :
            base(navigationTree, upperPanelControl, lowerPanelControl, datastore, logfileFilterExporter)
        {

        }

        public override int SortOrder => 500;

        public override IEnumerable<string> Build()
        {
            var result = new List<string>();
            string key = BaseKey;
            var dsref = Datastore.GetOrAdd<ProjectionActivity>();

            TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, $"Projection activities ({GetElementCount(key)})", "sync", Constants.treenodeBackColorNormal);
            result.Add(key);


            //by projection type (adHoc, ImportSync, Export Sync, ...)
            key = $"{BaseKey}/byPType";
            TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, $"by projection activity type ({GetElementCount(key)})", "sync", Constants.treenodeBackColorNormal);
            result.Add(key);

            foreach (var ptype in dsref.Projections.Select(t => t.projectionType).Distinct().OrderBy(t => t))
            {
                key = $"{BaseKey}/byPType/{ptype.ToString()}";
                TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, $"{ptype.ToString()} ({GetElementCount(key)})", "sync", Constants.treenodeBackColorNormal);
                result.Add(key);

                foreach (var proj in dsref.Projections.Where(t => t.projectionType == ptype).OrderBy(t => t.dtTimestampStart))
                {
                    key = $"{BaseKey}/byPType/{ptype.ToString()}/#{proj.uuid}";
                    TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, proj.GetLabel(), "job", Constants.treenodeBackColorNormal);
                    result.Add(key);
                }
            }


            //by target system type (Active Directory, SAP, CSV, Native Database, ...)
            key = $"{BaseKey}/byTsType";
            TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, $"by projection targetsystem type ({GetElementCount(key)})", "sync", Constants.treenodeBackColorNormal);
            result.Add(key);


            foreach (var tstype in dsref.Projections.Select(t => t.conn_TargetSystem).Distinct().OrderBy(t => t))
            {
                key = $"{BaseKey}/byTsType/{tstype.Replace("/","")}";
                TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, $"{tstype} ({GetElementCount(key)})", "sync", Constants.treenodeBackColorNormal);
                result.Add(key);

                foreach (var proj in dsref.Projections.Where(t => t.conn_TargetSystem == tstype).OrderBy(t => t.dtTimestampStart))
                {
                    key = $"{BaseKey}/byTsType/{tstype.Replace("/", "")}/#{proj.uuid}";
                    TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, proj.GetLabel(), "job", Constants.treenodeBackColorNormal);
                    result.Add(key);
                }
            }

            return result;
        }

        public override string BaseKey => $"{base.BaseKey}/ProjectionAct";

        public override int GetElementCount(string key)
        {
            var dsref = Datastore.GetOrAdd<ProjectionActivity>();

            if (key == BaseKey || key == $"{BaseKey}/byPType" || key == $"{BaseKey}/byTsType") 
                return dsref.Projections.Count;


            if (key.StartsWith(BaseKey + "/byPType/"))
            {
                var ptype = key.Substring((BaseKey + "/byPType/").Length);

                int i = ptype.IndexOf("/");
                if (i >= 0)
                    ptype = ptype.Substring(0, i);

                return dsref.Projections.Count(t => t.projectionType.ToString() == ptype);
            }

            if (key.StartsWith(BaseKey + "/byTsType/"))            
            {
                var tsType = key.Substring((BaseKey + "/byTsType/").Length);

                int i = tsType.IndexOf("/");
                if (i >= 0)
                    tsType = tsType.Substring(0, i);

                return dsref.Projections.Count(t => t.conn_TargetSystem.Replace("/", "") == tsType);
            }


            return 0;
        }

        public override void ExportView(string key)
        {
            if (!key.StartsWith(BaseKey))
                return;


            string uuid = "*";   //select a specific projection
            string PType = "*";  //filter a projection type (adHoc, ImportSync, Export Sync, ...)
            string TsType = "*"; //filter a projection target system type

            if (key.StartsWith(BaseKey + "/byPType/"))
                PType = key.Substring((BaseKey + "/byPType/").Length);

            int i = PType.IndexOf("/");
            if (i >= 0)
                PType = PType.Substring(0, i);

            if (key.StartsWith(BaseKey + "/byTsType/"))
                TsType = key.Substring((BaseKey + "/byTsType/").Length);

            i = TsType.IndexOf("/");
            if (i >= 0)
                TsType = TsType.Substring(0, i);

            if (key.Contains("#"))
                uuid = key.Substring(key.IndexOf("#") + 1);

            if (PType == "" && uuid != "")
                PType = "*";

            if (TsType == "" && uuid != "")
                TsType = "*";

            ExportProjectionInformation(uuid, PType, TsType);
        }

        private void ExportProjectionInformation(string uuidFilter, string PTypeFilter, string TsTypeFilter)
        {
            MultiListViewUC uc = new MultiListViewUC();

            uc.Suspend();

            ContextLinesUC contextLinesUc = new ContextLinesUC(LogfileFilterExporter);
            var dsref = Datastore.GetOrAdd<ProjectionActivity>();

            var projListScope = dsref.Projections.Where(p => (p.uuid == uuidFilter || uuidFilter == "*") &&
                                                             (p.projectionType.ToString() == PTypeFilter ||
                                                              PTypeFilter == "*") &&
                                                             (p.conn_TargetSystem.Replace("/", "") ==
                                                                 TsTypeFilter || TsTypeFilter == "*")
            );

#warning RFE#1.c: todo Projection.Maintenance
            //#6: Maintenance

            var projectionActivity = Datastore.GetOrAdd<ProjectionActivity>();

            //layout definition
            //=============================
            //the journal is very vague whether it was recorded or not and even when, e.g. failures are not always included ;D
            int numOfSubControls = FillListVcTypes.count;

            bool isToShowJournalSetup = projectionActivity.NumberOfJournalSetupTotal > 0;
            //numOfSubControls -= isToShowJournalSetup ? 0 : 1;

            bool isToShowJournalObjests = projectionActivity.NumberOfJournalObjectTotal > 0;
            //numOfSubControls -= isToShowJournalObjests ? 0 : 1;

            bool isToShowJournalMessages = projectionActivity.NumberOfJournalMessagesTotal > 0;
            //numOfSubControls -= isToShowJournalMessages ? 0 : 1;

            bool isToShowJournalFailures = projectionActivity.NumberOfJournalFailuresTotal > 0;
            //numOfSubControls -= isToShowJournalFailures ? 0 : 1;


            uc.SetupLayout(numOfSubControls);

            //Note: this method defines the layout and will call the FillListviewControl method only for the top nodes 1 only
            //each node will call this method for the child nodes (e.g. node 1 will call it for node 1.1 and 1.2)
            //when a node is clicked, it will set a projectionID filter and refresh all child nodes plus:
            //  all nodes of the top level exect node 1


            //1.) Projection activity general information
            //-------------------------------------------
            uc[FillListVcTypes.p1__PrjActGeneral].SetupCaption("Projection activity general information");
            uc[FillListVcTypes.p1__PrjActGeneral].SetupHeaders(new string[]
            {
                "Start", "End", "Type", "connection target system", "connetion OneIM", "StartUp config", "# steps",
                "AdHoc obj", "# cycles", "Logger id", "Log file"
            });
            uc[FillListVcTypes.p1__PrjActGeneral].ItemClicked += new ListViewItemSelectionChangedEventHandler(
                (object o, ListViewItemSelectionChangedEventArgs args) =>
                {
                    try
                    {
                        string prUuid = args.Item.Name;
                        var proj = dsref.Projections.FirstOrDefault(t => t.uuid == prUuid);
                        if (proj == null)
                            return;

                        RefreshListviewControlRecursive(FillListVcTypes.p1__PrjActGeneral, true, projListScope,
                            prUuid, "", uc, contextLinesUc);


                        if (proj?.message != null)
                            contextLinesUc.SetData(proj.message);
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Instance.HandleException(e);
                    }
                });


            //1.1.) Projection cycles
            //------------------------
            uc.SetupSubLevel(FillListVcTypes.p1_1__PrCycles, 1);
            uc[FillListVcTypes.p1_1__PrCycles].SetupCaption("Projection cycles");
            uc[FillListVcTypes.p1_1__PrCycles].SetupHeaders(new string[] {"Nr.", "Start"});
            uc[FillListVcTypes.p1_1__PrCycles].ItemClicked += new ListViewItemSelectionChangedEventHandler(
                (object o, ListViewItemSelectionChangedEventArgs args) =>
                {
                    try
                    {
                        string[] keydata = args.Item.Name.Split('@'); //{cy.uuid}@{prUuid}@{(++i)}                 

                        var cycle = dsref.Projections.First(t => t.uuid == keydata[1]).projectionCycles
                            .Where(c => c.uuid == keydata[0]);

                        if (cycle == null || cycle.FirstOrDefault() == null)
                            return;

                        if (cycle.FirstOrDefault().message != null)
                            contextLinesUc.SetData(cycle.FirstOrDefault().message);

                        RefreshListviewControlRecursive(FillListVcTypes.p1_1__PrCycles, true, projListScope,
                            keydata[1], "", uc, contextLinesUc);
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Instance.HandleException(e);
                    }
                });



            //1.2.) Projection execution steps
            //---------------------------- ----
            uc.SetupSubLevel(FillListVcTypes.p1_2__PrSteps, 1);
            uc[FillListVcTypes.p1_2__PrSteps].SetupCaption("Projection execution steps");
            uc[FillListVcTypes.p1_2__PrSteps].SetupHeaders(new string[]
            {
                "Step nr.", "Step id", "Start", "Direction", "Use Rev", "Map", "Schema class left",
                "Schema type left", "Schema class right", "Schema type right", "AdHoc object", "# of step details"
            });

            uc[FillListVcTypes.p1_2__PrSteps].ItemClicked += new ListViewItemSelectionChangedEventHandler(
                (object o, ListViewItemSelectionChangedEventArgs args) =>
                {
                    try
                    {
                        string[] keydata = args.Item.Name.Split('@'); //step@projection

                        var projStep = dsref.Projections.FirstOrDefault(t => t.uuid == keydata[1]).projectionSteps
                            .Where(t => t.uuid == keydata[0]).FirstOrDefault();
                        if (projStep == null)
                            return;

                        RefreshListviewControlRecursive(FillListVcTypes.p1_2__PrSteps, true, projListScope,
                            keydata[1], keydata[0], uc, contextLinesUc);

                        if (projStep?.message != null)
                            contextLinesUc.SetData(projStep.message);
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Instance.HandleException(e);
                    }
                });


            //1.2.1 Projection execution steps details
            //----------------------------------------
            uc.SetupSubLevel(FillListVcTypes.p1_2_1__PrStepsDetails, 2);
            uc[FillListVcTypes.p1_2_1__PrStepsDetails]
                .SetupCaption("Projection execution steps details - system data object list and object loading");
            uc[FillListVcTypes.p1_2_1__PrStepsDetails].SetupHeaders(new string[]
            {
                "Start", "End", "Duration", "State", "Type", "Load direction", "Schema class", "Query type",
                "Load information"
            });
            uc[FillListVcTypes.p1_2_1__PrStepsDetails].ItemClicked += new ListViewItemSelectionChangedEventHandler(
                (object o, ListViewItemSelectionChangedEventArgs args) =>
                {
                    try
                    {
                        string[] keydata = args.Item.Name.Split('@'); //detail@step@projection

                        var projStepDetail = dsref.Projections.First(t => t.uuid == keydata[2])
                            .projectionSteps.First(t => t.uuid == keydata[1])
                            .syncStepDetail
                            .GetSyncStepDetailByUuid(keydata[0]);

                        if (projStepDetail?.message != null)
                            contextLinesUc.SetData(projStepDetail.message, projStepDetail.messageEnd);

                        RefreshListviewControlRecursive(FillListVcTypes.p1_2_1__PrStepsDetails, true, projListScope,
                            keydata[2], "", uc, contextLinesUc);
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Instance.HandleException(e);
                    }
                });


            //2.) system connections and system connectors
            //---------------------------------------------
            uc[FillListVcTypes.p2__SysConns].SetupCaption("Projection system connections and system connectors");
            uc[FillListVcTypes.p2__SysConns].SetupHeaders(new string[]
                {"Projection logger id", "Type", "Logger id", "Start", "End", "Side", "Log file"});
            uc[FillListVcTypes.p2__SysConns].ItemClicked += new ListViewItemSelectionChangedEventHandler(
                (object o, ListViewItemSelectionChangedEventArgs args) =>
                {
                    try
                    {
                        string[] keydata = args.Item.Name.Split('@'); //{prSystemConn.uuid}@{pr.uuid}

                        var systemConn = dsref.Projections.First(t => t.uuid == keydata[1])
                            .systemConnectors.First(t => t.uuid == keydata[0]);

                        if (systemConn == null)
                            return;

                        RefreshListviewControlRecursive(FillListVcTypes.p2__SysConns, true, projListScope,
                            keydata[1], "", uc, contextLinesUc);


                        if (systemConn.message != null)
                            contextLinesUc.SetData(systemConn.message);
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Instance.HandleException(e);
                    }
                });


            //3.) Projection SQL
            //-----------------
            uc[FillListVcTypes.p3__PrSql]
                .SetupCaption(
                    "Projection sql information (attention: relation between a sql session and a projection job is a pure guess only!)");
            uc[FillListVcTypes.p3__PrSql].SetupHeaders(new string[]
            {
                "SQL logger id", "Is suspicious", "Session start", "Session ends", "Session duration",
                "Transaction count", "Top duration transaction", "Long running statement count",
                "Top duration statement"
            });
            uc[FillListVcTypes.p3__PrSql].ItemClicked += new ListViewItemSelectionChangedEventHandler(
                (object o, ListViewItemSelectionChangedEventArgs args) =>
                {
                    try
                    {
                        string[] keydata = args.Item.Name.Split('@'); //{sql.loggerSourceId}@{pr.uuid}

                        var sqlsession = dsref.Projections.First(t => t.uuid == keydata[1])
                            .specificSqlInformation
                            .SqlSessions.FirstOrDefault(t => t.uuid == keydata[0]);

                        if (sqlsession == null)
                            return;

                        RefreshListviewControlRecursive(FillListVcTypes.p3__PrSql, true, projListScope, keydata[1],
                            keydata[0], uc, contextLinesUc);

                        if (sqlsession.message != null)
                            contextLinesUc.SetData(sqlsession.message);
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Instance.HandleException(e);
                    }
                });


            //3.1) Projection SQL:  long running statements
            //-----------------------------------------------
            uc.SetupSubLevel(FillListVcTypes.p3_1__PrSqlLongRunning, 1);
            uc[FillListVcTypes.p3_1__PrSqlLongRunning].SetupCaption("long running statements");
            uc[FillListVcTypes.p3_1__PrSqlLongRunning].SetupHeaders(new string[]
            {
                "Timestamp", "Locator", "Duration [ms]", "Logger id", "Command", "Involved tables", "Statement text"
            });
            uc[FillListVcTypes.p3_1__PrSqlLongRunning].ItemClicked += new ListViewItemSelectionChangedEventHandler(
                (object o, ListViewItemSelectionChangedEventArgs args) =>
                {
                    try
                    {
                        string[]
                            keydata = args.Item.Name.Split('@'); //{longsql.uuid}@{sql.loggerSourceId}@{pr.uuid}

                        var longrunner = dsref.Projections.First(t => t.uuid == keydata[2])
                            .specificSqlInformation
                            .SqlSessions.First(t => t.uuid == keydata[1])?
                            .longRunningStatements?.First(l => l.uuid == keydata[0]);

                        if (longrunner?.message != null)
                            contextLinesUc.SetData(longrunner.message);

                        RefreshListviewControlRecursive(FillListVcTypes.p3_1__PrSqlLongRunning, true, projListScope,
                            keydata[2], "", uc, contextLinesUc);
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Instance.HandleException(e);
                    }
                });


            //3.2) Projection SQL: transactions
            //-----------------------------------
            uc.SetupSubLevel(FillListVcTypes.p3_2__PrSqlTransactions, 1);
            uc[FillListVcTypes.p3_2__PrSqlTransactions].SetupCaption("transacount list");
            uc[FillListVcTypes.p3_2__PrSqlTransactions].SetupHeaders(new string[]
                {"Start", "End", "Duration", "Start locator", "End locator", "State", "Logger id"});
            uc[FillListVcTypes.p3_2__PrSqlTransactions].ItemClicked += new ListViewItemSelectionChangedEventHandler(
                (object o, ListViewItemSelectionChangedEventArgs args) =>
                {
                    try
                    {
                        string[] keydata = args.Item.Name.Split('@'); //{trans.uuid}@{sql.loggerSourceId}@{pr.uuid}

                        var transact = dsref.Projections.First(t => t.uuid == keydata[2])
                            .specificSqlInformation
                            .SqlSessions.First(t => t.uuid == keydata[1])?
                            .transactions?.First(l => l.uuid == keydata[0]);

                        if (transact == null)
                            return;

                        if (transact.message != null)
                            contextLinesUc.SetData(transact.message);

                        RefreshListviewControlRecursive(FillListVcTypes.p3_2__PrSqlTransactions, true,
                            projListScope, keydata[2], "", uc, contextLinesUc);
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Instance.HandleException(e);
                    }
                });


            //4.) dpr sync journal information
            //---------------------------------
            uc[FillListVcTypes.p4__PrJournal].SetupCaption("sync journal information");
            uc[FillListVcTypes.p4__PrJournal].SetupHeaders(new string[]
            {
                "Projection", "Journal Start", "Journal End", "Projection Context", "Projection Start Info",
                "Projection Config", "Variable Set", "State", "Obj count", "Msg count", "Failure count"
            });
            uc[FillListVcTypes.p4__PrJournal].ItemClicked += new ListViewItemSelectionChangedEventHandler(
                (object o, ListViewItemSelectionChangedEventArgs args) =>
                {
                    try
                    {
                        string[] keydata = args.Item.Name.Split('@'); //{journalobj.uuid}@{proj.uuid}

                        var journal = dsref.Projections.First(t => t.uuid == keydata[1])
                            .projectionJournal;

                        if (journal == null)
                            return;

                        if (journal.uuid == keydata[0] && journal.message != null)
                            contextLinesUc.SetData(journal.message);

                        RefreshListviewControlRecursive(FillListVcTypes.p4__PrJournal, true, projListScope,
                            keydata[1], "", uc, contextLinesUc);
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Instance.HandleException(e);
                    }
                });

            //4.1) Dpr Sync Journal Setup
            //------------------------------
            if (isToShowJournalSetup)
            {
                uc.SetupSubLevel(FillListVcTypes.p4_1__PrJournalSetup, 1);
                uc[FillListVcTypes.p4_1__PrJournalSetup].SetupCaption("sync journal setup");
                uc[FillListVcTypes.p4_1__PrJournalSetup].SetupHeaders(new string[]
                    {"OptionContextDisplay", "OptionName", "OptionValue", "OptionContext"});
                //no item click, does not make sense :D
            }


            //4.2 objects
            //------------------------------            
            if (isToShowJournalObjests)
            {
                uc.SetupSubLevel(FillListVcTypes.p4_2__PrJournalObj, 1);
                uc[FillListVcTypes.p4_2__PrJournalObj].SetupCaption(
                    $"sync journal treated objects (total recorded number of messages: {projectionActivity.NumberOfJournalObjectTotal})");
                uc[FillListVcTypes.p4_2__PrJournalObj].SetupHeaders(new string[]
                    {"Object", "Object Id", "Method", "Schema type", "Sequence number", "Is import"});
                uc[FillListVcTypes.p4_2__PrJournalObj].ItemClicked += new ListViewItemSelectionChangedEventHandler(
                    (object o, ListViewItemSelectionChangedEventArgs args) =>
                    {
                        try
                        {
                            string[] keydata = args.Item.Name.Split('@'); //{journalObjects.uuid}@{proj.uuid}

                            var journalObj = dsref.Projections.First(t => t.uuid == keydata[1])
                                .projectionJournal
                                .journalObjects.Where(jo => jo.uuid == keydata[0]).FirstOrDefault();

                            if (journalObj == null)
                                return;

                            if (journalObj.uuid == keydata[0] && journalObj.message != null)
                                contextLinesUc.SetData(journalObj.message);

                            RefreshListviewControlRecursive(FillListVcTypes.p4_2__PrJournalObj, true, projListScope,
                                keydata[1], journalObj.uuid, uc, contextLinesUc);
                        }
                        catch (Exception e)
                        {
                            ExceptionHandler.Instance.HandleException(e);
                        }
                    });

                //4.2.1 props of objects
                //------------------------------            
                uc.SetupSubLevel(FillListVcTypes.p4_2_1__PrJournalObjProp, 2);
                uc[FillListVcTypes.p4_2_1__PrJournalObjProp]
                    .SetupCaption("sync journal treated objects's properties");
                uc[FillListVcTypes.p4_2_1__PrJournalObjProp]
                    .SetupHeaders(new string[] {"Property name", "Old value", "New value"});
                uc[FillListVcTypes.p4_2_1__PrJournalObjProp].ItemClicked +=
                    new ListViewItemSelectionChangedEventHandler(
                        (object o, ListViewItemSelectionChangedEventArgs args) =>
                        {
                            try
                            {
                                string[] keydata = args.Item.Name.Split('@'); //JournaleProp.UUID @ Projection.UUID

                                var journal = dsref.Projections.First(t => t.uuid == keydata[1])
                                    .projectionJournal;

                                if (journal == null)
                                    return;

                                var journalPropAll = journal
                                    .journalObjects.SelectMany(x => x.dprJournalProperties);

                                if (journalPropAll == null)
                                    return;

                                var journalProp = journalPropAll.Where(jp => jp.uuid == keydata[0])
                                    .FirstOrDefault();

                                if (journalProp == null)
                                    return;

                                if (journalProp.uuid == keydata[0] && journalProp.message != null)
                                    contextLinesUc.SetData(journalProp.message);

                                RefreshListviewControlRecursive(FillListVcTypes.p4_2_1__PrJournalObjProp, true,
                                    projListScope, keydata[1], journalProp.uuid, uc, contextLinesUc);
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Instance.HandleException(e);
                            }
                        });

            } //4.2.x JournalObjects and properties if any


            //4.3 Messages
            //------------------------------            
            if (isToShowJournalMessages)
            {
                uc.SetupSubLevel(FillListVcTypes.p4_3__PrJournalMessage, 1);

                uc[FillListVcTypes.p4_3__PrJournalMessage].SetupCaption(
                    $"sync journal messages (total number of recorded messages: {projectionActivity.NumberOfJournalMessagesTotal})");
                uc[FillListVcTypes.p4_3__PrJournalMessage].SetupHeaders(new string[]
                    {"Time", "Context", "Type", "Messagetext", "Source"});
                uc[FillListVcTypes.p4_3__PrJournalMessage].ItemClicked +=
                    new ListViewItemSelectionChangedEventHandler(
                        (object o, ListViewItemSelectionChangedEventArgs args) =>
                        {
                            try
                            {
                                string[]
                                    keydata = args.Item.Name.Split('@'); //JournaleMessage.UUID @ Projection.UUID

                                var journal = dsref.Projections.First(t => t.uuid == keydata[1]).projectionJournal;

                                if (journal == null)
                                    return;

                                var jmesg = journal.journalMessages.Where(m => m.uuid == keydata[0])
                                    .FirstOrDefault();

                                if (jmesg?.message != null)
                                    contextLinesUc.SetData(jmesg.message);

                                RefreshListviewControlRecursive(FillListVcTypes.p4_3__PrJournalMessage, true,
                                    projListScope, keydata[1], "", uc, contextLinesUc);
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Instance.HandleException(e);
                            }
                        });
            } //4.3 journal messages, if any

            //4.4 Failures
            //------------------------------            
            if (isToShowJournalFailures)
            {
                uc.SetupSubLevel(FillListVcTypes.p4_4__PrJournalFailure, 1);
                uc[FillListVcTypes.p4_4__PrJournalFailure].SetupCaption(
                    $"sync journal failure messages (total number of recorded messages: {projectionActivity.NumberOfJournalFailuresTotal})");
                uc[FillListVcTypes.p4_4__PrJournalFailure].SetupHeaders(new string[]
                    {"Time", "Projection step", "Schema type", "Object", "Reason", "Object state"});
                uc[FillListVcTypes.p4_4__PrJournalFailure].ItemClicked +=
                    new ListViewItemSelectionChangedEventHandler(
                        (object o, ListViewItemSelectionChangedEventArgs args) =>
                        {
                            try
                            {
                                string[]
                                    keydata = args.Item.Name.Split('@'); //JournaleFailure.UUID @ Projection.UUID

                                var journal = dsref.Projections.First(t => t.uuid == keydata[1]).projectionJournal;

                                if (journal == null)
                                    return;

                                var failure = journal.journalFailures.Where(m => m.uuid == keydata[0])
                                    .FirstOrDefault();

                                if (failure?.message != null)
                                    contextLinesUc.SetData(failure.message);

                                RefreshListviewControlRecursive(FillListVcTypes.p4_4__PrJournalFailure, true,
                                    projListScope, keydata[1], keydata[0], uc, contextLinesUc);
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Instance.HandleException(e);
                            }
                        });


                uc.SetupSubLevel(FillListVcTypes.p4_4_1__PrJournalFailureObjProp, 2);
                uc[FillListVcTypes.p4_4_1__PrJournalFailureObjProp]
                    .SetupCaption($"sync journal failed objects and logged changes");
                uc[FillListVcTypes.p4_4_1__PrJournalFailureObjProp].SetupHeaders(new string[]
                {
                    "Object", "Object Id", "Method", "Schema type", "Sequence number", "Is import", "Property name",
                    "Old value", "New value"
                });
                uc[FillListVcTypes.p4_4_1__PrJournalFailureObjProp].ItemClicked +=
                    new ListViewItemSelectionChangedEventHandler(
                        (object o, ListViewItemSelectionChangedEventArgs args) =>
                        {
                            try
                            {
                                string[]
                                    keydata = args.Item.Name
                                        .Split(
                                            '@'); //DPRJournalProperty.uuid @ DPRJournalObject.uuid @ DPRJournalFailure.uuid @ Projection.UUID


                                var journal = dsref.Projections.First(t => t.uuid == keydata[3]).projectionJournal;

                                if (journal == null)
                                    return;

                                var failure = journal.journalFailures.Where(m => m.uuid == keydata[2])
                                    .FirstOrDefault();

                                if (failure == null)
                                    return;

                                var obj = failure.dprJournalObjects.Where(jo => jo.uuid == keydata[1])
                                    .FirstOrDefault();

                                if (obj == null)
                                    return;

                                var prop = obj.dprJournalProperties.Where(jp => jp.uuid == keydata[0])
                                    .FirstOrDefault();

                                if (prop?.message != null)
                                    contextLinesUc.SetData(prop.message);

                                RefreshListviewControlRecursive(FillListVcTypes.p4_4_1__PrJournalFailureObjProp,
                                    true, projListScope, keydata[1], prop.uuid, uc, contextLinesUc);
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Instance.HandleException(e);
                            }
                        });
            } //failures, if any


            //Data handling, execute the top node only, the rest is done recursivly
            //=============================
            RefreshListviewControlRecursive(FillListVcTypes.p1__PrjActGeneral, false, projListScope, "", "", uc,
                contextLinesUc);
            /*FillListviewControl(FillListVcTypes.p2__SysConns, projListScope, "", "", uc, contextLinesUc);
            FillListviewControl(FillListVcTypes.p3__PrSql, projListScope, "", "", uc, contextLinesUc);
            FillListviewControl(FillListVcTypes.p4__PrJournal, projListScope, "", "", uc, contextLinesUc);*/

            //resulting
            //=============================
            if (uc.HasData())
            {
                UpperPanelControl.Add(uc);
                LowerPanelControl.Add(contextLinesUc);
            }

            uc.Resume();
        }

        private void RefreshListviewControlRecursive(byte startNode, bool updateChildreenOnly, IEnumerable<Projection> scope, string prUuid, string secondId, MultiListViewUC uc, ContextLinesUC contextLinesUc)
        {
            try
            {
                uc.Suspend();

                //draw the current node, but only if not just clicked an item on it
                if (!updateChildreenOnly)
                    FillListviewControl(startNode, scope, prUuid, secondId, uc, contextLinesUc);

                //refresh the childdreen dending on the startnode
                switch (startNode)
                {
                    case FillListVcTypes.p1__PrjActGeneral:
                        FillListviewControl(FillListVcTypes.p1_1__PrCycles, scope, prUuid, secondId, uc,
                            contextLinesUc);
                        FillListviewControl(FillListVcTypes.p1_2__PrSteps, scope, prUuid, secondId, uc, contextLinesUc);

                        FillListviewControl(FillListVcTypes.p2__SysConns, scope, prUuid, secondId, uc, contextLinesUc);
                        FillListviewControl(FillListVcTypes.p3__PrSql, scope, prUuid, secondId, uc, contextLinesUc);
                        FillListviewControl(FillListVcTypes.p4__PrJournal, scope, prUuid, secondId, uc, contextLinesUc);

                        break;

                    case FillListVcTypes.p1_1__PrCycles:
                        break;

                    case FillListVcTypes.p1_2__PrSteps:
                        FillListviewControl(FillListVcTypes.p1_2_1__PrStepsDetails, scope, prUuid, secondId, uc,
                            contextLinesUc);
                        break;

                    case FillListVcTypes.p1_2_1__PrStepsDetails:
                        break;


                    case FillListVcTypes.p2__SysConns:
                        FillListviewControl(FillListVcTypes.p3__PrSql, scope, prUuid, secondId, uc, contextLinesUc);
                        FillListviewControl(FillListVcTypes.p4__PrJournal, scope, prUuid, secondId, uc, contextLinesUc);
                        break;


                    case FillListVcTypes.p3__PrSql:
                        FillListviewControl(FillListVcTypes.p3_1__PrSqlLongRunning, scope, prUuid, secondId, uc,
                            contextLinesUc);
                        FillListviewControl(FillListVcTypes.p3_2__PrSqlTransactions, scope, prUuid, secondId, uc,
                            contextLinesUc);

                        FillListviewControl(FillListVcTypes.p2__SysConns, scope, prUuid, secondId, uc, contextLinesUc);
                        FillListviewControl(FillListVcTypes.p4__PrJournal, scope, prUuid, secondId, uc, contextLinesUc);
                        break;

                    case FillListVcTypes.p3_1__PrSqlLongRunning:
                        break;

                    case FillListVcTypes.p3_2__PrSqlTransactions:
                        break;


                    case FillListVcTypes.p4__PrJournal:
                        FillListviewControl(FillListVcTypes.p4_1__PrJournalSetup, scope, prUuid, secondId, uc,
                            contextLinesUc);
                        FillListviewControl(FillListVcTypes.p4_2__PrJournalObj, scope, prUuid, secondId, uc,
                            contextLinesUc);
                        FillListviewControl(FillListVcTypes.p4_3__PrJournalMessage, scope, prUuid, secondId, uc,
                            contextLinesUc);
                        FillListviewControl(FillListVcTypes.p4_4__PrJournalFailure, scope, prUuid, secondId, uc,
                            contextLinesUc);

                        FillListviewControl(FillListVcTypes.p2__SysConns, scope, prUuid, secondId, uc, contextLinesUc);
                        FillListviewControl(FillListVcTypes.p3__PrSql, scope, prUuid, secondId, uc, contextLinesUc);
                        break;

                    case FillListVcTypes.p4_1__PrJournalSetup:
                        break;

                    case FillListVcTypes.p4_2__PrJournalObj:
                        FillListviewControl(FillListVcTypes.p4_2_1__PrJournalObjProp, scope, prUuid, secondId, uc,
                            contextLinesUc);
                        break;

                    case FillListVcTypes.p4_2_1__PrJournalObjProp:
                        break;

                    case FillListVcTypes.p4_3__PrJournalMessage:
                        break;

                    case FillListVcTypes.p4_4__PrJournalFailure:
                        FillListviewControl(FillListVcTypes.p4_4_1__PrJournalFailureObjProp, scope, prUuid, secondId,
                            uc, contextLinesUc);
                        break;

                    case FillListVcTypes.p4_4_1__PrJournalFailureObjProp:
                        break;

                }
            }
            finally
            {
                uc.Resume();
            }
        }

        private void FillListviewControl(byte ucNumber, IEnumerable<Projection> scope, string prUuid, string secondId, MultiListViewUC uc, ContextLinesUC contextLinesUc)
        {
            if (ucNumber >= uc.Count())
                return; //the requested sub control was not defined in method ExportProjectionInformation but another sub control requested now an update, so we can skip this

            if (scope.HasNoData())
                return;

            try
            {
                uc.Suspend();

                uc[ucNumber].Suspend();

                uc[ucNumber].Clear();

                /*
                if (string.IsNullOrEmpty(prUuid) && (ucNumber != 0) && (ucNumber != 4) && (ucNumber != 5))
                {
                    prUuid = scope.FirstOrDefault()?.uuid;
                    if (prUuid == null)
                        return;
                }*/

                switch (ucNumber)
                {
                    case FillListVcTypes.p1__PrjActGeneral:  // uc[uc_1prBaseList].SetupHeaders(new string[] {"Start", "End", "Type", "connection target system", "connetion OneIM", "StartUp config", "# steps", "AdHoc obj", "# cycles", "Logger id", "Log file" });
                        foreach (var pr in scope.OrderBy(t => t.dtTimestampStart))
                            uc[ucNumber].AddItemRow(pr.uuid, new string[]
                            {
                                pr.dtTimestampStart.ToString("G"), pr.dtTimestampEnd.ToString("G"),
                                pr.projectionType.ToString(),
                                pr.conn_TargetSystem, pr.conn_IdentityManager,
                                pr.syncStartUpConfig,
                                pr.projectionSteps.Count.ToString(),
                                pr.adHocObject.DefaultIfEmpty("-"),
                                pr.projectionCycles.Count.ToString(),
                                pr.loggerSourceId,
                                pr.logfileName
                            });

                        return;

                    case FillListVcTypes.p1_1__PrCycles: //Projection cycles: uc[uc_2prCyclList].SetupHeaders(new string[] {"Nr." , "Start", "End" });                   
                        foreach (var pr in scope.Where(p => p.uuid == prUuid))
                        {
                            int i = 0;
                            foreach (var cy in pr.projectionCycles)
                                uc[ucNumber].AddItemRow($"{cy.uuid}@{prUuid}@{(++i)}", new string[] {
                                    i.ToString(),
                                    cy.dtTimestamp.ToString("G")
                                });
                        }

                        return;

                    case FillListVcTypes.p1_2__PrSteps: //Projection execution steps: Headers(new string[] { "Step nr.", "Step id", "start", "Direction", "Use Rev", "Map", "Schema class left", "Schema type left", "Schema class right", "Schema type right", "AdHoc object", "# of step details" });
                        foreach (var pr in scope.Where(p => p.uuid == prUuid))
                            foreach (var step in pr.projectionSteps)
                                uc[ucNumber].AddItemRow($"{step.uuid}@{prUuid}", new string[]
                                {
                                    step.stepNr.ToString(), step.stepId,
                                    step.dtTimestampStart.ToString("G"), step.direction,
                                    step.useRevision,
                                    step.map,
                                    step.leftSchemaClassName, step.leftSchemaTypeGuess,
                                    step.rightSchemaClassName, step.rightSchemaTypeGuess,
                                    step.adHocObject,
                                    step.syncStepDetail.Count().ToString()
                                });

                       return;

                    case FillListVcTypes.p1_2_1__PrStepsDetails: //Projection execution steps Detail: Headers(new string[] { "Start", "End", "dura", "State", "Type", "Load direction", "Schema class", "Query type", "Load information" });                    
                        foreach (var pr in scope.Where(p => p.uuid == prUuid))
                        {
                            if (secondId == "")  //step id
                                secondId = pr.projectionSteps.FirstOrDefault()?.uuid ?? ""; // .syncStepDetail.loadingObjectList_left.uuid ?? "-";

                            var lst = pr.projectionSteps?.Where(s => s.uuid == secondId)?.Select(s => s.syncStepDetail).EmptyIfNull();

                            foreach (var detail in lst) 
                                foreach (var det in detail.GetSyncStepDetails())
                                    uc[ucNumber].AddItemRow($"{det.uuid}@{secondId}@{prUuid}", new string[]
                                        {
                                            det.dtTimestampStart.ToString("G"),
                                            det.dtTimestampEnd.ToString("G"),
                                            det.DurationString(true),
                                            det.isSuccessful ? "success": "failure",
                                            det.systemConnType.ToString(),
                                            det.connectionSide.ToString(),  
                                            det.schemaClassName,
                                            det.queryType.ToString(),
                                            det.GetAdditionalInformation()
                                        }, 
                                        "", 
                                        GetBgColor(!det.IsSuspicious)
                                    );
                        }

                          return;

                    case FillListVcTypes.p2__SysConns: //Projection system connections and system connectors - Headers(new string[] { "Projection logger id", "Type", "Logger id", "Start", "End", "Side", "Log file" });
                        foreach (var pr in scope.Where(p => prUuid == "" || p.uuid == prUuid))
                            foreach (var prSystemConn in pr.systemConnectors.OrderBy(c => c.belongsToProjectorId + c.dtTimestampEnd.Ticks.ToString()))
                                uc[ucNumber].AddItemRow($"{prSystemConn.uuid}@{pr.uuid}", new string[]
                                {
                                    pr.loggerSourceId,
                                    prSystemConn.systemConnType.ToString(),
                                    prSystemConn.loggerSourceId,
                                    prSystemConn.dtTimestampStart.ToString("G"), prSystemConn.dtTimestampEnd.ToString("G"),
                                    prSystemConn.belongsToSide.ToString(),
                                    prSystemConn.logfileName
                                });

                         return;

                    case FillListVcTypes.p3__PrSql: //Projection sql information - Headers(new string[] { "session id", "is suspicious", "session start", "session ends", "session duration", "transaction count", "top duration transaction", "long running statement count", "top duration statement" });
                        foreach (var pr in scope.Where(p => prUuid == "" || p.uuid == prUuid))
                            foreach (var sql in pr.specificSqlInformation.SqlSessions)
                                uc[ucNumber].AddItemRow($"{sql.uuid}@{pr.uuid}", new string[]
                                {
                                    sql.loggerSourceId,
                                    sql.isSuspicious ? "yes, pls check" : "all fine",
                                    sql.dtTimestampStart.ToString("G"), sql.dtTimestampEnd.ToString("G"),
                                    (sql.dtTimestampEnd - sql.dtTimestampStart).ToHumanString(),
                                    sql.transactions.Count.ToString(),
                                    sql.transactions.Count == 0 ? "-" : sql.transactions.OrderByDescending(t => t.durationMin).First().GetLabel(),
                                    sql.longRunningStatements.Count.ToString(),
                                    sql.longRunningStatements.Count == 0 ? "-" : sql.longRunningStatements.OrderByDescending(t => t.durationMsec).First().GetLabel()
                                }, 
                                "",
                                sql.isSuspicious ? Constants.treenodeBackColorSuspicious : Constants.treenodeBackColorNormal
                                );


                        return;

                    case FillListVcTypes.p3_1__PrSqlLongRunning: //"long running statements" - Headers(new string[] { "timestamp", "locator", "duration [s]", "logger id", "command", "involved tables", "statement text" });
                        foreach (var pr in scope.Where(p => p.uuid == prUuid))
                        {
                            if (secondId == "")  //sql session id
                                secondId = pr.specificSqlInformation.SqlSessions.FirstOrDefault()?.uuid ?? "-";

                            var lst = pr.specificSqlInformation.SqlSessions.Where(s => s.uuid == secondId).FirstOrDefault()?.longRunningStatements;
                            foreach (var longsql in lst.EmptyIfNull())
                                uc[ucNumber].AddItemRow($"{longsql.uuid}@{secondId}@{pr.uuid}", new string[]
                                {
                                    longsql.dtTimestampStart.ToString("G"),
                                    longsql.GetEventLocator(),
                                    longsql.durationMsec.ToString("n0"),
                                    longsql.loggerSourceId,
                                    longsql.sqlCmdType.ToString(),
                                    string.Join("; ",longsql.assignedTablenames),
                                    longsql.statementText
                                });
                        }

                        return;

                    case FillListVcTypes.p3_2__PrSqlTransactions: //"transacount list" - Headers(new string[] { "start", "end", "duration", "start locator", "end locator", "state", "logger id" });
                        foreach (var pr in scope.Where(p => p.uuid == prUuid))
                        {
                            if (secondId == "")  //sql session id
                                secondId = pr.specificSqlInformation.SqlSessions.FirstOrDefault()?.uuid ?? "-";

                            var lst = pr.specificSqlInformation.SqlSessions.Where(s => s.uuid == secondId).FirstOrDefault()?.transactions;
                            foreach (var trans in lst.EmptyIfNull())
                                uc[ucNumber].AddItemRow($"{trans.uuid}@{secondId}@{pr.uuid}", new string[]
                                {
                                    trans.dtTimestampStart.ToString("G"), trans.dtTimestampEnd.ToString("G"),
                                    trans.DurationString(true),
                                    trans.GetEventLocatorStart(), trans.GetEventLocatorEnd(),
                                    trans.isSuccessFullTransaction ? "COMMIT" : "ROLLBACK/TERMINATION",
                                    trans.loggerSourceId
                                });
                        }

                        return;

                    case FillListVcTypes.p4__PrJournal: //DPRJournal
                        foreach (var pr in scope.Where(p => (prUuid == "" || (prUuid != "" && p.uuid == prUuid))))
                        {
                            DprJournal dprj = pr.projectionJournal;
                            if (dprj == null)
                                continue;

                            uc[ucNumber].AddItemRow($"{dprj.uuid}@{pr.uuid}", new string[]
                                    {
                                        pr.GetLabel(), //.ToString(),
                                        dprj.dtTimestampStart.ToString("G"),
                                        dprj.dtTimestampEnd.ToString("G"),
                                        dprj.Get("ProjectionContext"),
                                        dprj.Get("ProjectionStartInfoDisplay"),
                                        dprj.Get("ProjectionConfigDisplay"),
                                        dprj.Get("SystemVariableSetDisplay"),
                                        dprj.Get("ProjectionState"),
                                        dprj.journalObjects.Count.ToString(),
                                        dprj.journalMessages.Count.ToString(),
                                        dprj.journalFailures.Count.ToString()
                                    });
                        }
                                               
                        return;

                    case FillListVcTypes.p4_1__PrJournalSetup:
                        foreach (var pr in scope.Where(p =>  p.uuid == prUuid))
                        {
                            DprJournal dprj = pr.projectionJournal;
                            if (dprj == null)
                                continue;

                            string[] cols = { "OptionContextDisplay", "OptionName", "OptionValue", "OptionContext" };

                            foreach (var row in dprj.GetSetupElements(false, cols)
                                                .Union(dprj.GetSetupElements(true, cols)))
                                uc[ucNumber].AddItemRow(row.Key, row.Value);
                        }
                        return;

                    case FillListVcTypes.p4_2__PrJournalObj:
                        foreach (var pr in scope.Where(p => p.uuid == prUuid))
                        {
                            DprJournal dprj = pr.projectionJournal;
                            if (dprj == null)
                                return;
                            
                            string[] cols = { "ObjectDisplay", "ObjectIdentifier", "Method", "SchemaTypeName", "SequenceNumber", "IsImport" };

                            string elemID;
                            foreach (var row in dprj.GetJournalObjects(cols))
                            {
                                elemID = $"{row.Key}@{pr.uuid}";
                                uc[ucNumber].AddItemRow(elemID, row.Value);
                            }
                        }
                        return;

                    case FillListVcTypes.p4_2_1__PrJournalObjProp:
                        foreach (var pr in scope.Where(p => p.uuid == prUuid))
                        {
                            DprJournal dprj = pr.projectionJournal;
                            if (dprj == null)
                                return;

                            string elemID;
                            foreach (var row in dprj.GetJournalObjectsProperties(secondId))
                            {
                                elemID = $"{row.Key}@{pr.uuid}";  //JournaleProp.UUID @ Projection.UUID
                                uc[ucNumber].AddItemRow(elemID, row.Value);
                            }
                        }
                        return; 

                    case FillListVcTypes.p4_3__PrJournalMessage:
                        foreach (var pr in scope.Where(p => p.uuid == prUuid))
                        {
                            DprJournal dprj = pr.projectionJournal;
                            if (dprj == null)
                                return;

                            string elemID;
                            Color col;
                            foreach (var row in dprj.GetJournalMessages())
                            {
                                elemID = $"{row.Key}@{pr.uuid}";  //JournalMsg.UUID @ Projection.UUID
                                col = row.Value[2].StartsWith("E") ? Constants.treenodeBackColorSuspicious : Constants.treenodeBackColorNormal;
                                uc[ucNumber].AddItemRow(elemID, row.Value, "", col);
                            }
                        }
                        return;

                    case FillListVcTypes.p4_4__PrJournalFailure:
                        foreach (var pr in scope.Where(p => p.uuid == prUuid))
                        {
                            DprJournal dprj = pr.projectionJournal;
                            if (dprj == null)
                                return;

                            string elemID;
                            foreach (var row in dprj.GetJournalFailures(new string[] { "ProjectionStepDisplay", "SchemaTypeName", "ObjectDisplay", "Reason", "ObjectState" }))
                            {
                                elemID = $"{row.Key}@{pr.uuid}";  //JournalFailure.UUID @ Projection.UUID
                                uc[ucNumber].AddItemRow(elemID, row.Value, "", Constants.treenodeBackColorSuspicious);
                            }
                        }
                        return;

                    case FillListVcTypes.p4_4_1__PrJournalFailureObjProp:
                        foreach (var pr in scope.Where(p => p.uuid == prUuid))
                        {
                            DprJournal dprj = pr.projectionJournal;
                            if (dprj == null)
                                return;

                            string elemID;
                            foreach (var row in dprj.GetJournalFailedObjAndProps(secondId))
                            {
                                //row.Key is already: $"{prop.Key}@{failedObj.uuid}";
                                elemID = $"{row.Key}@{secondId}@{pr.uuid}";  //DPRJournalProperty.uuid @ DPRJournalObject.uuid @ DPRJournalFailure.uuid @ Projection.UUID
                                uc[ucNumber].AddItemRow(elemID, row.Value);
                            }
                        }
                        return;


                }
            }
            finally
            {                
                uc[ucNumber].Resume();

                uc.Resume();
            }
        }

        private static class FillListVcTypes
        {
            public const byte count = 15;

            public const byte p1__PrjActGeneral = 0;
            public const byte p1_1__PrCycles = 1;
            public const byte p1_2__PrSteps = 2;
            public const byte p1_2_1__PrStepsDetails = 3;

            public const byte p2__SysConns = 4;

            public const byte p3__PrSql = 5;
            public const byte p3_1__PrSqlLongRunning = 6;
            public const byte p3_2__PrSqlTransactions = 7;

            public const byte p4__PrJournal = 8;
            public const byte p4_1__PrJournalSetup = 9;
            public const byte p4_2__PrJournalObj = 10;
            public const byte p4_2_1__PrJournalObjProp = 11;
            public const byte p4_3__PrJournalMessage = 12;
            public const byte p4_4__PrJournalFailure = 13;
            public const byte p4_4_1__PrJournalFailureObjProp = 14;

        }
    }
}
