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
    internal class JobServiceActivitiesView : DatastoreBaseView
    {
        public JobServiceActivitiesView(TreeView navigationTree, 
            Control.ControlCollection upperPanelControl,
            Control.ControlCollection lowerPanelControl, 
            DataStore datastore, 
            Exporter logfileFilterExporter) :
            base(navigationTree, upperPanelControl, lowerPanelControl, datastore, logfileFilterExporter)
        {
        }


        public override int SortOrder => 400;

        public override IEnumerable<string> Build()
        {
            var result = new List<string>();
            string key = BaseKey;
            var dsref = Datastore.GetOrAdd<JobServiceActivity>();
            int cnt;
            int cntJsJobs = GetElementCount(key);
            
            TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, $"Jobservice activities ({cntJsJobs})", "activity", Constants.treenodeBackColorNormal);
            result.Add(key);


            key = $"{BaseKey}/byCompTask";
            cnt = GetElementCount(key);
            TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, $"by component and task ({cnt})", "component", Constants.treenodeBackColorNormal);
            result.Add(key);

            foreach (var task in dsref.DistinctTaskFull.OrderBy(t => t))
            {
                key = $"{BaseKey}/byCompTask/{task.Replace("/", "")}";
                TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, $"{task} ({GetElementCount(key)})", "component", GetBgColor(!dsref.JobServiceJobs.Any(t => t.taskfull == task && !t.isSmoothExecuted)));
                result.Add(key);
            }


            key = $"{BaseKey}/byQueue";
            TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, $"by queue name ({GetElementCount(key)})", "component", Constants.treenodeBackColorNormal);
            result.Add(key);

            foreach (var queuename in dsref.DistinctQueueName.OrderBy(t => t))
            {
                string qn = (string.IsNullOrEmpty(queuename) ? "<empty>" : queuename);
                key = $"{BaseKey}/byQueue/{qn.Replace("/", "")}";
                TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, $"{qn} ({GetElementCount(key)})", "component", GetBgColor(!dsref.JobServiceJobs.Any(t => t.queuename == queuename && !t.isSmoothExecuted)));
                result.Add(key);
            }


            if (cnt > 0)
            {
                key = $"{BaseKey}/byCompTaskPerf";
                TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, $"run time performance ({cnt})", "component", Constants.treenodeBackColorNormal);
                result.Add(key);
            }


            key = $"{BaseKey}/withRetries";
            cnt = GetElementCount(key);
            if (cnt > 0)
            {                
                TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, $"jobs with retries or errors/warnings ({cnt})", "warning", Constants.treenodeBackColorSuspicious);
                result.Add(key);
            }

            
            key = $"{BaseKey}/withMissingStartFinish";
            cnt = GetElementCount(key);
            if (cnt > 0)
            {                
                TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, $"jobs with missing start or finish message ({cnt})", "warning", Constants.treenodeBackColorSuspicious);
                result.Add(key);
            }

            return result;
        }

        public override string BaseKey => $"{base.BaseKey}/JobserviceAct";

        public override int GetElementCount(string key)
        {
            var dsref = Datastore.GetOrAdd<JobServiceActivity>();

            if (key == BaseKey)
                return dsref.JobServiceJobs.Count;


            if (key == $"{BaseKey}/byCompTask")
                return dsref.DistinctTaskFull.Count;


            if (key == $"{BaseKey}/byQueue")
                return dsref.DistinctQueueName.Count;


            if (key.StartsWith($"{BaseKey}/byCompTask/"))
            {
                var task = key.Substring((BaseKey + "/byCompTask/").Length);
                return dsref.JobServiceJobs.Count(t => t.taskfull.Replace("/", "") == task);
            }

            if (key.StartsWith($"{BaseKey}/byQueue/"))
            {
                var queuename = key.Substring((BaseKey + "/byQueue/").Length);

                if (queuename == "<empty>")
                    queuename = "";

                return dsref.JobServiceJobs.Count(t => t.queuename.Replace("/", "") == queuename);
            }

            if (key == $"{BaseKey}/withRetries")
                return dsref.JobServiceJobs.Count(t => !t.isSmoothExecuted);

            if (key == $"{BaseKey}/withMissingStartFinish")
                return dsref.JobServiceJobs.Count(j => j.hasMissingStartOrFinishMessages);

            return 0;
        }

        public override void ExportView(string key)
        {
            if (!key.StartsWith(BaseKey))
                return;

            MultiListViewUC uc;
            ContextLinesUC contextLinesUc;
            Tuple<MultiListViewUC, ContextLinesUC> guiContent;


            if (key == $"{BaseKey}/byCompTaskPerf")
                guiContent = ListJsJobsPerformance(key);
            else
                guiContent = ListJsJobsbyTable(key);


            uc = guiContent.Item1;
            contextLinesUc = guiContent.Item2;

            uc.Resume();

            if (uc.HasData())
            {
                UpperPanelControl.Add(uc);
                LowerPanelControl.Add(contextLinesUc);
            }
        }

        private Tuple<MultiListViewUC, ContextLinesUC> ListJsJobsbyTable(string key)
        {
            MultiListViewUC uc = new MultiListViewUC();
            ContextLinesUC contextLinesUc = new ContextLinesUC(LogfileFilterExporter);
            var dsref = Datastore.GetOrAdd<JobServiceActivity>();


            string caption = "Job service jobs";

            string tasknamefull = "";
            bool filterByTask = false;
            string queuename = "*";
            bool filterByQueuename = false;
            bool filterByErrorJobs = false;
            bool filterByMissingStartFinishJobs = false;


            if (key.StartsWith(BaseKey + "/byCompTask/"))
            {
                filterByTask = true;
                tasknamefull = key.Substring((BaseKey + "/byCompTask/").Length);
                caption = $"Job service jobs of task '{tasknamefull}'";
            }


            if (key.StartsWith(BaseKey + "/byQueue/"))
            {
                filterByQueuename = true;
                queuename = key.Substring((BaseKey + "/byQueue/").Length);
                if (queuename == "<empty>")
                    queuename = "";
                caption = $"Job service jobs of queue name '{queuename}'";
            }


            if (key == BaseKey + "/withRetries")
            {
                filterByErrorJobs = true;
                caption = $"{GetElementCount(key)} Job Service jobs with unsuccessfull or suspicious state";
            }

            if (key == BaseKey + "/withMissingStartFinish")
            {
                filterByMissingStartFinishJobs = true;
                caption = $"{GetElementCount(key)} Job Service jobs with missing start or finish message";
            }



            uc.SetupLayout(1);
            uc[0].SetupCaption(caption);
            uc[0].SetupHeaders(new string[] { "Start time", "Finish time", "Duration", "Exec Attemp", "State", "Queue name", "Component", "Task", "Params", "Job id", "Result text" });

            int attemptNr;
            foreach (var jsAct in dsref.JobServiceJobs.Where(j => (key == BaseKey ||
                                                                        filterByErrorJobs && !j.isSmoothExecuted ||
                                                                        filterByMissingStartFinishJobs && j.hasMissingStartOrFinishMessages ||
                                                                        filterByTask && j.taskfull.Replace("/", "") == tasknamefull ||
                                                                        filterByQueuename && j.queuename.Replace("/", "") == queuename
                                                                    ) &&
                                                                    j.jobserviceJobattempts.Count > 0)
                                                        .OrderBy(j => j.jobserviceJobattempts[0].dtTimestampStart)
                    )
            {
                attemptNr = 0;
                foreach (var jsAttemp in jsAct.jobserviceJobattempts.OrderBy(j => j.dtTimestampStart))
                {
                    attemptNr++;
                    uc[0].AddItemRow(jsAttemp.uuid, new string[]
                        {
                            jsAttemp.dtTimestampStart.IsNull() ? "n/a" : jsAttemp.dtTimestampStart.ToString("G"),
                            jsAttemp.dtTimestampEnd.IsNull() ? "n/a" : jsAttemp.dtTimestampEnd.ToString("G"),
                            jsAttemp.DurationString(),
                            attemptNr.ToString(),
                            jsAttemp.SuccessInformation,
                            jsAct.queuename,
                            jsAct.componentname,
                            jsAct.taskname,
                            jsAct.parameters,
                            jsAct.uidJob,
                            jsAttemp.resultmessagetext ?? ""
                        });
                }
            }

            if (dsref.JobServiceJobs.Count > 0)
                uc[0].ItemClicked += new ListViewItemSelectionChangedEventHandler((object o, ListViewItemSelectionChangedEventArgs args) =>
                {
                    try
                    {
                        string uuidAttemp = args.Item.Name;
                        var jsAttempSelect = dsref.JobServiceJobs.SelectMany(t => t.jobserviceJobattempts).Where(a => a.uuid == uuidAttemp).FirstOrDefault();

                        if (jsAttempSelect == null)
                            return;

                        if (jsAttempSelect.message != null && jsAttempSelect.endmessage == null)
                            contextLinesUc.SetData(jsAttempSelect.message);

                        else if (jsAttempSelect.message == null && jsAttempSelect.endmessage != null)
                            contextLinesUc.SetData(jsAttempSelect.endmessage);

                        else if (jsAttempSelect.message != null && jsAttempSelect.endmessage != null)
                            contextLinesUc.SetData(jsAttempSelect.message, jsAttempSelect.endmessage);
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Instance.HandleException(e);
                    }
                });


            return new Tuple<MultiListViewUC, ContextLinesUC>(uc, contextLinesUc);
        }


        private Tuple<MultiListViewUC, ContextLinesUC> ListJsJobsPerformance(string key)
        {
            MultiListViewUC uc = new MultiListViewUC();
            ContextLinesUC contextLinesUc = new ContextLinesUC(LogfileFilterExporter);
            var dsref = Datastore.GetOrAdd<JobServiceActivity>();

            uc.SetupLayout(1);
            uc[0].SetupCaption("Job Service job run time performance");

            uc[0].SetupHeaders(new string[] { "Task", "Queue name", "State", "Count", "Minimum duration [sec]", "Maximum duration [sec]", "Average duration [sec]" });


            foreach (string taskname in dsref.DistinctTaskFull.OrderBy(n => n))
            {
                foreach (string queuename in dsref.JobServiceJobs.Where(j => j.taskfull == taskname).Select(j => j.queuename).Distinct())
                {
                    var jobs_all = dsref.JobServiceJobs.Where(j => j.taskfull == taskname && j.queuename == queuename).SelectMany(j => j.jobserviceJobattempts);
                    var jobs_valid = jobs_all.Where(a => !a.hasMissingStartOrFinishMessage).ToArray();
                    var jobs_invalid = jobs_all.Where(a => a.hasMissingStartOrFinishMessage).ToArray();

                    if (jobs_valid.Length > 0)
                    {
                        uint maxDura = jobs_valid.Max(j => j.durationSec);
                        string uuid_expensiveJob = jobs_valid.First(j => j.durationSec == maxDura).uuid;

                        uc[0].AddItemRow($"{taskname}{queuename}valid#{uuid_expensiveJob}", new string[] {
                            taskname,
                            queuename,
                            "finished jobs",
                            jobs_valid.Length.ToString(),
                            jobs_valid.Min(j => j.durationSec).ToString(),
                            maxDura.ToString(),
                            Math.Round(jobs_valid.Average(j => j.durationSec),1).ToString("F1")
                        });
                    }

                    if (jobs_invalid.Length > 0)
                        uc[0].AddItemRow($"{taskname}{queuename}invalid", new string[] {
                            taskname,
                            queuename,
                            "jobs start/finish missing",
                            jobs_invalid.Length.ToString(),
                            "N/A","N/A","N/A"
                        });
                }
            }

            if (dsref.JobServiceJobs.Count > 0)
                uc[0].ItemClicked += new ListViewItemSelectionChangedEventHandler((object o, ListViewItemSelectionChangedEventArgs args) =>
                {
                    try
                    {
                        int pos = args.Item.Name.LastIndexOf("#");

                        if (pos < 0)
                            return;

                        string uuidAttemp = args.Item.Name.Substring(pos+1);
                        var jsAttempSelect = dsref.JobServiceJobs.SelectMany(t => t.jobserviceJobattempts).Where(a => a.uuid == uuidAttemp).FirstOrDefault();

                        if (jsAttempSelect == null)
                            return;

                        if (jsAttempSelect.message != null && jsAttempSelect.endmessage == null)
                            contextLinesUc.SetData(jsAttempSelect.message);

                        else if (jsAttempSelect.message == null && jsAttempSelect.endmessage != null)
                            contextLinesUc.SetData(jsAttempSelect.endmessage);

                        else if (jsAttempSelect.message != null && jsAttempSelect.endmessage != null)
                            contextLinesUc.SetData(jsAttempSelect.message, jsAttempSelect.endmessage);
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Instance.HandleException(e);
                    }
                });

            return new Tuple<MultiListViewUC, ContextLinesUC>(uc, contextLinesUc);
        }
    }
}
