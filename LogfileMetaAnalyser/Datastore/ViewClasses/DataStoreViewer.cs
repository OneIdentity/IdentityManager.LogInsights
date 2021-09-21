using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using LogfileMetaAnalyser.Helpers;
using System.Text.Json;


namespace LogfileMetaAnalyser.Datastore
{
    public class DataStoreViewer
    {      
        private DatastoreStructure datastore;
        private Exporter logfileFilterExporter;

        private ResultUcCache ucCache = new ResultUcCache();
        private Form ownerForm;
        private Control.ControlCollection upperPanelControl;
        private Control.ControlCollection lowerPanelControl;

        private Dictionary<string, IDatastoreView> responsibleViewerClass = new Dictionary<string, IDatastoreView>();


        public DataStoreViewer(DatastoreStructure datastore, Exporter logfileFilterExporter, Form ownerForm, Control.ControlCollection upperPanelControl, Control.ControlCollection lowerPanelControl)
        {
            this.datastore = datastore;
            this.logfileFilterExporter = logfileFilterExporter;
            this.ownerForm = ownerForm;
            this.upperPanelControl = upperPanelControl;
            this.lowerPanelControl = lowerPanelControl;

            datastore.storeInvalidation += new EventHandler((object s, EventArgs args) => 
            {
                ucCache.ClearCache();
            });
        }


        public string ExportAsJson()
        {
            if (datastore == null || !datastore.HasData())
                return ("Datastore is empty because no analysis was yet performed!");

            try
            {                                 
                return JsonSerializer.Serialize(datastore, new JsonSerializerOptions()
                {
                    
                    PropertyNameCaseInsensitive = true,
                    IncludeFields = true, //include public fields even without an explicit getter/setter
                    WriteIndented = true, //write pretty formatted text
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
            }
            catch (Exception E)
            {
                return $"Export not possible: {E.Message}";
            }
        }

        public bool ExportAsNavigationTreeView(ref TreeView tw)
        {
            try
            {
                tw.BeginUpdate();
                tw.Nodes.Clear();
                responsibleViewerClass.Clear();

                //top node
                IDatastoreView viewer = new TopNodeViewer() { upperPanelControl = upperPanelControl, lowerPanelControl = lowerPanelControl, datastore = datastore, logfileFilterExporter = this.logfileFilterExporter };
                string key = viewer.BaseKey;
                TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, "Analyze report", "report", System.Drawing.Color.Transparent);
                responsibleViewerClass.Add(key, viewer);


                //1.) timetrace
                ExportAsNavigationTreeView_Timetrace(ref tw);
                
                //2.) General log information
                ExportAsNavigationTreeView_GenLogInfo(ref tw);

                //3.) general sql information
                ExportAsNavigationTreeView_SqlInfo(ref tw);

                //4.) Jobservice activity 
                ExportAsNavigationTreeView_JobServiceActivity(ref tw);

                //5.) projection activity
                ExportAsNavigationTreeView_ProjectionActivity(ref tw);

                //99.) Statistics
                ExportAsNavigationTreeView_Statistics(ref tw);
            }
            finally
            {
                if (tw.Nodes.Count > 0)
                    tw.Nodes[0].Expand();
                tw.EndUpdate();
            }

            return true;
        }


        private void ExportAsNavigationTreeView_Timetrace(ref TreeView tw)
        {
            IDatastoreView viewer = new TimetraceReferenceView() { upperPanelControl = upperPanelControl, lowerPanelControl = lowerPanelControl, datastore = datastore, logfileFilterExporter = this.logfileFilterExporter };

            string key = viewer.BaseKey;
            int cnt = viewer.GetElementCount(key);
            TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"Graphical time line ({cnt})", "timetrace", Constants.treenodeBackColorNormal);
            responsibleViewerClass.Add(key, viewer);

            key = $"{viewer.BaseKey}/Full";
            cnt = viewer.GetElementCount(key);
            if (cnt > 300)
            {                
                TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"show all  ({cnt})", "timetrace", Constants.treenodeBackColorNormal);
                responsibleViewerClass.Add(key, viewer);
            }
        }

        private void ExportAsNavigationTreeView_GenLogInfo(ref TreeView tw)
        {
            IDatastoreView viewer = new GeneralLogView() { upperPanelControl = upperPanelControl, lowerPanelControl = lowerPanelControl, datastore = datastore, logfileFilterExporter = this.logfileFilterExporter };
            string key = viewer.BaseKey;
            var dsref = datastore.generalLogData;


            //Branch: General information
            TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, "General information", "information", GetBgColor(dsref.mostDetailedLogLevel.IsGreater(LogLevel.Info)));
            responsibleViewerClass.Add(key, viewer);


            //Branch: ErrorsAndWarnings
            if (dsref.messageErrors.Any() || dsref.messageWarnings.Any())
            {
                key = $"{viewer.BaseKey}/ErrorsAndWarnings";
                TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"message of type error and warning ({viewer.GetElementCount(key)})", "warning", Constants.treenodeBackColorSuspicious);
                responsibleViewerClass.Add(key, viewer);
            }

            if (dsref.messageErrors.Any())
            {
                key = $"{viewer.BaseKey}/ErrorsAndWarnings/Errors";
                TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"message of type error ({viewer.GetElementCount(key)})", "error", Constants.treenodeBackColorSuspicious);
                responsibleViewerClass.Add(key, viewer);

                key = $"{viewer.BaseKey}/ErrorsAndWarnings/Errors/distinct";
                TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"message of type error distinct", "error", Constants.treenodeBackColorSuspicious);
                responsibleViewerClass.Add(key, viewer);
            }

            if (dsref.messageWarnings.Any())
            {
                key = $"{viewer.BaseKey}/ErrorsAndWarnings/Warnings";
                TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"message of type warning ({viewer.GetElementCount(key)})", "warning", Constants.treenodeBackColorSuspicious);
                responsibleViewerClass.Add(key, viewer);

                key = $"{viewer.BaseKey}/ErrorsAndWarnings/Warnings/distinct";
                TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, "message of type warning distinct", "warning", Constants.treenodeBackColorSuspicious);
                responsibleViewerClass.Add(key, viewer);
            }

            //Branch: timestamp
            if (dsref.timegaps.Any())
            {
                key = $"{viewer.BaseKey}/timegaps";
                TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"timestamp gaps ({viewer.GetElementCount(key)})", "warning", Constants.treenodeBackColorSuspicious);
                responsibleViewerClass.Add(key, viewer);
            }
        }

        private void ExportAsNavigationTreeView_SqlInfo(ref TreeView tw)
        {
            IDatastoreView viewer = new SqlInformationView() { upperPanelControl = upperPanelControl, lowerPanelControl = lowerPanelControl, datastore = datastore, logfileFilterExporter = this.logfileFilterExporter };
            string key = viewer.BaseKey;
            var dsref = datastore.generalSqlInformation;

            TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, "General SQL session information", "information", Constants.treenodeBackColorNormal);
            responsibleViewerClass.Add(key, viewer);

            if (dsref.sqlSessions.Any())
            {
                key = $"{viewer.BaseKey}/sessions";
                TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"General SQL session information ({viewer.GetElementCount(key)})", "information", Constants.treenodeBackColorNormal);
                responsibleViewerClass.Add(key, viewer);

                foreach (var session in dsref.sqlSessions.OrderBy(t => t.loggerSourceId))
                {
                    key = $"{viewer.BaseKey}/sessions/{session.uuid}";
                    TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"Sql session {session.ToString()}", (session.isSuspicious ? "warning" : "information"), GetBgColor(!session.isSuspicious));
                    responsibleViewerClass.Add(key, viewer);
                }
            }
        }

        private void ExportAsNavigationTreeView_JobServiceActivity(ref TreeView tw)
        {
            IDatastoreView viewer = new JobServiceActivitiesView() { upperPanelControl = upperPanelControl, lowerPanelControl = lowerPanelControl, datastore = datastore, logfileFilterExporter = this.logfileFilterExporter };
            string key = viewer.BaseKey;
            var dsref = datastore.jobserviceActivities;
            int cnt;
            int cntJsJobs = viewer.GetElementCount(key); 

            TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"Jobservice activities ({cntJsJobs})", "activity", Constants.treenodeBackColorNormal);
            responsibleViewerClass.Add(key, viewer);


            key = $"{viewer.BaseKey}/byCompTask";
            cnt = viewer.GetElementCount(key);
            TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"by component and task ({cnt})", "component", Constants.treenodeBackColorNormal);
            responsibleViewerClass.Add(key, viewer);

            foreach (var task in dsref.distinctTaskfull.OrderBy(t => t))
            {
                key = $"{viewer.BaseKey}/byCompTask/{task.Replace("/", "")}";
                TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"{task} ({viewer.GetElementCount(key)})", "component", GetBgColor(!dsref.jobserviceJobs.Any(t => t.taskfull == task && !t.isSmoothExecuted)));
                responsibleViewerClass.Add(key, viewer);
            }


            key = $"{viewer.BaseKey}/byQueue";
            TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"by queue name ({viewer.GetElementCount(key)})", "component", Constants.treenodeBackColorNormal);
            responsibleViewerClass.Add(key, viewer);

            foreach (var queuename in dsref.distinctQueuename.OrderBy(t => t))
            {
                string qn = (string.IsNullOrEmpty(queuename) ? "<empty>" : queuename);
                key = $"{viewer.BaseKey}/byQueue/{qn.Replace("/", "")}";
                TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"{qn} ({viewer.GetElementCount(key)})", "component", GetBgColor(!dsref.jobserviceJobs.Any(t => t.queuename == queuename && !t.isSmoothExecuted)));
                responsibleViewerClass.Add(key, viewer);
            }


            if (cnt > 0)
            {
                key = $"{viewer.BaseKey}/byCompTaskPerf";
                TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"run time performance ({cnt})", "component", Constants.treenodeBackColorNormal);
                responsibleViewerClass.Add(key, viewer);
            }


            key = $"{viewer.BaseKey}/withRetries";
            cnt = viewer.GetElementCount(key);
            if (cnt > 0)
            {                
                TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"jobs with retries or errors/warnings ({cnt})", "warning", Constants.treenodeBackColorSuspicious);
                responsibleViewerClass.Add(key, viewer);
            }

            
            key = $"{viewer.BaseKey}/withMissingStartFinish";
            cnt = viewer.GetElementCount(key);
            if (cnt > 0)
            {                
                TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"jobs with missing start or finish message ({cnt})", "warning", Constants.treenodeBackColorSuspicious);
                responsibleViewerClass.Add(key, viewer);
            }
        }

        private void ExportAsNavigationTreeView_ProjectionActivity(ref TreeView tw)
        {
            IDatastoreView viewer = new ProjectionActivitiesView() { upperPanelControl = upperPanelControl, lowerPanelControl = lowerPanelControl, datastore = datastore, logfileFilterExporter = this.logfileFilterExporter };
            string key = viewer.BaseKey;
            var dsref = datastore.projectionActivity;

            TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"Projection activities ({viewer.GetElementCount(key)})", "sync", Constants.treenodeBackColorNormal);
            responsibleViewerClass.Add(key, viewer);


            //by projection type (adHoc, ImportSync, Export Sync, ...)
            key = $"{viewer.BaseKey}/byPType";
            TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"by projection activity type ({viewer.GetElementCount(key)})", "sync", Constants.treenodeBackColorNormal);
            responsibleViewerClass.Add(key, viewer);

            foreach (var ptype in dsref.projections.Select(t => t.projectionType).Distinct().OrderBy(t => t))
            {
                key = $"{viewer.BaseKey}/byPType/{ptype.ToString()}";
                TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"{ptype.ToString()} ({viewer.GetElementCount(key)})", "sync", Constants.treenodeBackColorNormal);
                responsibleViewerClass.Add(key, viewer);

                foreach (var proj in dsref.projections.Where(t => t.projectionType == ptype).OrderBy(t => t.dtTimestampStart))
                {
                    key = $"{viewer.BaseKey}/byPType/{ptype.ToString()}/#{proj.uuid}";
                    TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, proj.GetLabel(), "job", Constants.treenodeBackColorNormal);
                    responsibleViewerClass.Add(key, viewer);
                }
            }


            //by target system type (Active Directory, SAP, CSV, Native Database, ...)
            key = $"{viewer.BaseKey}/byTsType";
            TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"by projection targetsystem type ({viewer.GetElementCount(key)})", "sync", Constants.treenodeBackColorNormal);
            responsibleViewerClass.Add(key, viewer);


            foreach (var tstype in dsref.projections.Select(t => t.conn_TargetSystem).Distinct().OrderBy(t => t))
            {
                key = $"{viewer.BaseKey}/byTsType/{tstype.Replace("/","")}";
                TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, $"{tstype} ({viewer.GetElementCount(key)})", "sync", Constants.treenodeBackColorNormal);
                responsibleViewerClass.Add(key, viewer);

                foreach (var proj in dsref.projections.Where(t => t.conn_TargetSystem == tstype).OrderBy(t => t.dtTimestampStart))
                {
                    key = $"{viewer.BaseKey}/byTsType/{tstype.Replace("/", "")}/#{proj.uuid}";
                    TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, proj.GetLabel(), "job", Constants.treenodeBackColorNormal);
                    responsibleViewerClass.Add(key, viewer);
                }
            }
        }

        private void ExportAsNavigationTreeView_Statistics(ref TreeView tw)
        {
            IDatastoreView viewer = new StatisticsStoreView() { upperPanelControl = upperPanelControl, lowerPanelControl = lowerPanelControl, datastore = datastore, logfileFilterExporter = this.logfileFilterExporter };
            string key = viewer.BaseKey;
            TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, "Analyze statistics", "stats", Constants.treenodeBackColorNormal);
            responsibleViewerClass.Add(key, viewer);

            key = $"{viewer.BaseKey}/1";
            TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, "Parsing statistics", "stats", Constants.treenodeBackColorNormal);
            responsibleViewerClass.Add(key, viewer);

            key = $"{viewer.BaseKey}/2";
            TreeNodeCollectionHelper.CreateNode(tw.Nodes, key, "Detectors statistics", "stats", Constants.treenodeBackColorNormal);
            responsibleViewerClass.Add(key, viewer);
        }

        public async Task ExportAsViewContent(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            await Task.Run(() =>
            {

                Helpers.GuiHelper.SetGuiSave(ownerForm, () =>
                {

                    try
                    {
                        ownerForm.Cursor = Cursors.WaitCursor;

                        upperPanelControl.Clear();
                        lowerPanelControl.Clear();

                        if (ucCache.HasData(key))
                            ucCache.GetFromCache(key, upperPanelControl, lowerPanelControl);
                        else
                        {

                 
                            if (ownerForm.IsDisposed || ownerForm.Disposing)
                                return;

                            if (responsibleViewerClass.ContainsKey(key))
                                responsibleViewerClass[key].ExportView(key);
                            else
                            {
                                var viewer = responsibleViewerClass.Where(v => key.StartsWith(v.Value.BaseKey));

                                if (viewer.Any())
                                    viewer.First().Value.ExportView(key);
                                else
                                {
                                    //Fallback :(                                    
                                    foreach (var viewercls in responsibleViewerClass.Values.Distinct())
                                    {
                                        viewercls.ExportView(key);

                                        if (upperPanelControl.Count > 0)
                                            break;
                                    }

                                    if (upperPanelControl.Count > 0)
                                        MessageBox.Show($"no viewer class found to handle key '{key}'");
                                }
                            }

                            ucCache.AddToCache(key, upperPanelControl, lowerPanelControl);
                      
                            
                        }
               
                    }
                    finally
                    {
                        Helpers.GuiHelper.SetGuiSave(ownerForm, () =>
                        {
                            ownerForm.Cursor = Cursors.Default;
                        });
                    }
                });
            }).ConfigureAwait(false);
        }

        private System.Drawing.Color GetBgColor(bool conditionForGood)
        {
            return conditionForGood ? Constants.treenodeBackColorNormal : Constants.treenodeBackColorSuspicious;
        }

    }
}
