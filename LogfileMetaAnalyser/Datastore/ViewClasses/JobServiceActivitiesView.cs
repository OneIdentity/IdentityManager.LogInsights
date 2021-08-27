using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

using LogfileMetaAnalyser.Helpers;
using LogfileMetaAnalyser.Controls;


namespace LogfileMetaAnalyser.Datastore
{
    class JobServiceActivitiesView : DatastoreBaseView, IDatastoreView
    {
        public JobServiceActivitiesView()
        { }

        public override string BaseKey => $"{base.BaseKey}/JobserviceAct";

        public int GetElementCount(string key)
        {
            var dsref = datastore.jobserviceActivities;

            if (key == BaseKey)
                return dsref.jobserviceJobs.Count;


            if (key == $"{BaseKey}/byCompTask")
                return dsref.distinctTaskfull.Count;


            if (key == $"{BaseKey}/byQueue")
                return dsref.distinctQueuename.Count;


            if (key.StartsWith($"{BaseKey}/byCompTask/"))
            {
                var task = key.Substring((BaseKey + "/byCompTask/").Length);
                return dsref.jobserviceJobs.Count(t => t.taskfull.Replace("/", "") == task);
            }

            if (key.StartsWith($"{BaseKey}/byQueue/"))
            {
                var queuename = key.Substring((BaseKey + "/byQueue/").Length);

                if (queuename == "<empty>")
                    queuename = "";

                return dsref.jobserviceJobs.Count(t => t.queuename.Replace("/", "") == queuename);
            }

            if (key == $"{BaseKey}/withRetries")
                return dsref.jobserviceJobs.Where(t => !t.isSmoothExecuted).Count();

            if (key == $"{BaseKey}/withMissingStartFinish")
                return dsref.jobserviceJobs.Where(j => j.hasMissingStartOrFinishMessages).Count();

            return 0;
        }

        public void ExportView(string key)
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
                upperPanelControl.Add(uc);
                lowerPanelControl.Add(contextLinesUc);
            }
        }

        private Tuple<MultiListViewUC, ContextLinesUC> ListJsJobsbyTable(string key)
        {
            MultiListViewUC uc = new MultiListViewUC();
            ContextLinesUC contextLinesUc = new ContextLinesUC(logfileFilterExporter);
            var dsref = datastore.jobserviceActivities;


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
            foreach (var jsAct in dsref.jobserviceJobs.Where(j => (key == BaseKey ||
                                                                        filterByErrorJobs && !j.isSmoothExecuted ||
                                                                        filterByMissingStartFinishJobs && j.hasMissingStartOrFinishMessages ||
                                                                        filterByTask && j.taskfull.Replace("/", "") == tasknamefull ||
                                                                        filterByQueuename && j.queuename.Replace("/", "") == queuename
                                                                    ) &&
                                                                    j.jobserviceJobattempts.Any())
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

            if (dsref.jobserviceJobs.Count > 0)
                uc[0].ItemClicked += new ListViewItemSelectionChangedEventHandler((object o, ListViewItemSelectionChangedEventArgs args) =>
                {
                    string uuidAttemp = args.Item.Name;
                    var jsAttempSelect = dsref.jobserviceJobs.SelectMany(t => t.jobserviceJobattempts).Where(a => a.uuid == uuidAttemp).FirstOrDefault();

                    if (jsAttempSelect == null)
                        return;

                    if (jsAttempSelect.message != null && jsAttempSelect.endmessage == null)
                        contextLinesUc.SetData(jsAttempSelect.message);

                    else if (jsAttempSelect.message == null && jsAttempSelect.endmessage != null)
                        contextLinesUc.SetData(jsAttempSelect.endmessage);

                    else if (jsAttempSelect.message != null && jsAttempSelect.endmessage != null)
                        contextLinesUc.SetData(jsAttempSelect.message, jsAttempSelect.endmessage);
                });


            return new Tuple<MultiListViewUC, ContextLinesUC>(uc, contextLinesUc);
        }


        private Tuple<MultiListViewUC, ContextLinesUC> ListJsJobsPerformance(string key)
        {
            MultiListViewUC uc = new MultiListViewUC();
            ContextLinesUC contextLinesUc = new ContextLinesUC(logfileFilterExporter);
            var dsref = datastore.jobserviceActivities;

            uc.SetupLayout(1);
            uc[0].SetupCaption("Job Service job run time performance");

            uc[0].SetupHeaders(new string[] { "Task", "Queue name", "State", "Count", "Minimum duration [sec]", "Maximum duration [sec]", "Average duration [sec]" });


            foreach (string taskname in dsref.distinctTaskfull.OrderBy(n => n))
            {
                foreach (string queuename in dsref.jobserviceJobs.Where(j => j.taskfull == taskname).Select(j => j.queuename).Distinct())
                {
                    var jobs_all = dsref.jobserviceJobs.Where(j => j.taskfull == taskname && j.queuename == queuename).SelectMany(j => j.jobserviceJobattempts);
                    var jobs_valid = jobs_all.Where(a => !a.hasMissingStartOrFinishMessage);
                    var jobs_invalid = jobs_all.Where(a => a.hasMissingStartOrFinishMessage);

                    if (jobs_valid.Any())
                    {
                        uint maxDura = jobs_valid.Max(j => j.durationSec);
                        string uuid_expensiveJob = jobs_valid.Where(j => j.durationSec == maxDura).First().uuid;

                        uc[0].AddItemRow($"{taskname}{queuename}valid#{uuid_expensiveJob}", new string[] {
                            taskname,
                            queuename,
                            "finished jobs",
                            jobs_valid.Count().ToString(),
                            jobs_valid.Min(j => j.durationSec).ToString(),
                            maxDura.ToString(),
                            Math.Round(jobs_valid.Average(j => j.durationSec),1).ToString("F1")
                        });
                    }

                    if (jobs_invalid.Any())
                        uc[0].AddItemRow($"{taskname}{queuename}invalid", new string[] {
                            taskname,
                            queuename,
                            "jobs start/finish missing",
                            jobs_invalid.Count().ToString(),
                            "N/A","N/A","N/A"
                        });
                }
            }

            if (dsref.jobserviceJobs.Count > 0)
                uc[0].ItemClicked += new ListViewItemSelectionChangedEventHandler((object o, ListViewItemSelectionChangedEventArgs args) =>
                {
                    int pos = args.Item.Name.LastIndexOf("#");

                    if (pos < 0)
                        return;

                    string uuidAttemp = args.Item.Name.Substring(pos+1);
                    var jsAttempSelect = dsref.jobserviceJobs.SelectMany(t => t.jobserviceJobattempts).Where(a => a.uuid == uuidAttemp).FirstOrDefault();

                    if (jsAttempSelect == null)
                        return;

                    if (jsAttempSelect.message != null && jsAttempSelect.endmessage == null)
                        contextLinesUc.SetData(jsAttempSelect.message);

                    else if (jsAttempSelect.message == null && jsAttempSelect.endmessage != null)
                        contextLinesUc.SetData(jsAttempSelect.endmessage);

                    else if (jsAttempSelect.message != null && jsAttempSelect.endmessage != null)
                        contextLinesUc.SetData(jsAttempSelect.message, jsAttempSelect.endmessage);
                });

            return new Tuple<MultiListViewUC, ContextLinesUC>(uc, contextLinesUc);
        }
    }
}
