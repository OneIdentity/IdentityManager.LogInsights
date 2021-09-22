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
    class TimetraceReferenceView: DatastoreBaseView, IDatastoreView
    {
        private static int showLimitedViewElementCount = 100; //each max cnt for ad hoc jobs; Jobservice jobs; warnings; errors

        public TimetraceReferenceView()
        { }

        public override string BaseKey => $"{base.BaseKey}/Timetrace"; 

        public int GetElementCount(string key)
        {
            bool showFull = (key == $"{BaseKey}/Full");

            int cnt = 0;
            int cnt_real;

            
            //syncs
            foreach (var ps in datastore.projectionActivity.projections.Where(p =>
                                                                                (p.projectionType != ProjectionType.AdHocProvision && p.projectionType != ProjectionType.Unknown)
                                                                                || (p.projectionType == ProjectionType.Unknown)))
                cnt += ps.projectionSteps.Count;


            //ad hoc jobs
            cnt_real = datastore.projectionActivity.projections.Count(p => p.projectionType == ProjectionType.AdHocProvision);
            if (showFull)
                cnt += cnt_real;
            else
                cnt += Math.Min(showLimitedViewElementCount, cnt_real);


            //other (Jobservice) jobs 
            if (showFull)
                cnt += datastore.jobserviceActivities.jobserviceJobs
                            .Where(js => js.jobserviceJobattempts.Count > 0)
                            .SelectMany(ja => ja.jobserviceJobattempts)
                            .Count();
            else
                cnt += Math.Min(showLimitedViewElementCount, datastore.jobserviceActivities.jobserviceJobs.Count(js => js.jobserviceJobattempts.Count > 0));


            //errors
            cnt_real = datastore.generalLogData.messageErrors.Count;
            if (showFull)
                cnt += cnt_real;
            else
                cnt += Math.Min(showLimitedViewElementCount, cnt_real);


            //warnings
            cnt_real = datastore.generalLogData.messageWarnings.Count;
            if (showFull)
                cnt += cnt_real;
            else
                cnt += Math.Min(showLimitedViewElementCount, cnt_real);


            return cnt;
        }

        public void ExportView(string key)
        {
            if (!key.StartsWith(BaseKey) || (datastore.generalLogData.logfileInformation.Count == 0))
                return;

            bool showFull = (key == $"{BaseKey}/Full");

            //display most important time events

            ContextLinesUC contextLinesUc = new ContextLinesUC(logfileFilterExporter);
            TimeTraceUC uc = new TimeTraceUC();
            uc.Dock = DockStyle.Fill;


            //file times
            //============================
            try
            {
                Color c1 = Color.FromArgb(5, 170, 220);
                Color c2 = c1.Darken(64);
                Color c3 = c1.Darken(-64);

                uc.AddBlockTrack("allfiles", "log file coverage", Color.DarkBlue, c1, c2, c1, c3);

                //print for each file a block into time trace
                var fileinfoEvs = datastore.generalLogData.logfileInformation.Select(f => new TimelineTrackEvent(
                                                                                                            f.Value.GetLabel(),
                                                                                                            f.Value.logfileTimerange_Start,
                                                                                                            f.Value.logfileTimerange_Finish)
                                                                                             ).ToList();
                uc.GetTrack("allfiles").AddTrackEvents(fileinfoEvs);
            } catch { }


            //syncs
            //============================
            try
            { 
                ExportProjectionJobs("Sync",  (p => p.projectionType != ProjectionType.AdHocProvision && p.projectionType != ProjectionType.Unknown), uc);
                ExportProjectionJobs("SyncX", (p => p.projectionType == ProjectionType.Unknown), uc);
            }
            catch { }


            //ad hoc jobs
            //============================ 
            try
            {
                Color TCol1 = Color.Orange;
                Color TCol2 = Color.DarkOrange;
                Color ECol1 = Color.Orange;
                Color ECol2 = Color.Wheat;
                var proj = datastore.projectionActivity.projections.Where(p => p.projectionType == ProjectionType.AdHocProvision).ToArray();

                if (proj.Length > 0)
                {
                    uc.AddBlockTrack("AdHoc", "Ad Hoc Projection jobs", Color.DarkBlue, TCol1, TCol2, ECol1, ECol2);

                    var adHocJobs = proj.Select(p => new TimelineTrackEvent(
                                                            p.GetLabel(true),
                                                            DateHelper.IfNull(p.dtTimestampStart, datastore.generalLogData.logDataOverallTimeRange_Start),
                                                            DateHelper.IfNull(p.dtTimestampEnd, datastore.generalLogData.logDataOverallTimeRange_Finish),
                                                            p.message,
                                                            p.ToString())
                                               );

                    if (showFull)
                        uc.GetTrack("AdHoc").AddTrackEvents(adHocJobs.ToList());
                    else
                        uc.GetTrack("AdHoc").AddTrackEvents(adHocJobs.TakeDistributed(showLimitedViewElementCount, (lst) => { Random r = new Random(); return lst.ToDictionary(x => x.eventStart.ToFileTime() + r.Next()); }).ToList());
                }
            }
            catch { }


            //other (Jobservice) jobs
            //============================
            try
            {
                List<TimelineTrackEvent> jsJobs;

                if (!showFull)
                    jsJobs = datastore.jobserviceActivities.jobserviceJobs
                                                       .Where(js => js.jobserviceJobattempts.Count>0)
                                                       .Select(j => new TimelineTrackEvent(
                                                                        j.GetLabel(),
                                                                        DateHelper.IfNull(j.jobserviceJobattempts.OrderBy(x => x.dtTimestampStart).First().dtTimestampStart, datastore.generalLogData.logDataOverallTimeRange_Start),
                                                                        DateHelper.IfNull(j.jobserviceJobattempts.OrderBy(x => x.dtTimestampStart).Last().dtTimestampEnd, datastore.generalLogData.logDataOverallTimeRange_Finish),
                                                                        j.jobserviceJobattempts[0].message,
                                                                        j.ToString()))
                                                       .ToList();
                else
                {
                    jsJobs = new List<TimelineTrackEvent>();

                    foreach (var job in datastore.jobserviceActivities.jobserviceJobs
                                                       .Where(js => js.jobserviceJobattempts.Count > 0))
                        jsJobs.AddRange(job.jobserviceJobattempts.Select(a => new TimelineTrackEvent(
                                                                                job.GetLabel(),
                                                                                DateHelper.IfNull(a.dtTimestampStart, datastore.generalLogData.logDataOverallTimeRange_Start),
                                                                                DateHelper.IfNull(a.dtTimestampEnd, datastore.generalLogData.logDataOverallTimeRange_Finish),
                                                                                a.message,
                                                                                job.ToString()))

                                       );
                }


                Color colJsTrack1 = Color.FromArgb(130, 10, 225);
                Color colJsTrack2 = Color.FromArgb(90, 70, 100);
                Color colJsEvt1 = Color.FromArgb(90, 10, 225);
                Color colJsEvt2 = Color.FromArgb(200, 200, 200);

                uc.AddBlockTrack("jobs", "Other Jobservice Jobs", Color.DarkBlue, colJsTrack1, colJsTrack2, colJsEvt1, colJsEvt2);

                if (showFull)
                    uc.GetTrack("jobs").AddTrackEvents(jsJobs.ToList());
                else
                    uc.GetTrack("jobs").AddTrackEvents(jsJobs.TakeDistributed(showLimitedViewElementCount, (lst) => { Random r = new Random(); return lst.ToDictionary(x => x.eventStart.ToFileTime() + r.Next()); }).ToList());
            }
            catch { }


            //errors
            //============================
            try
            {
                var errorEvents = datastore.generalLogData.messageErrors
                        .Select(t => 
                            new TimelineTrackEvent(
                                    StringHelper.ShortenText(t.message.messageText), 
                                    t.dtTimestamp,
                                    t.message
                                    ));

                Color colErr = Color.FromArgb(255, 20, 20);
                uc.AddPointTrack("errors", "error messages", colErr);

                if (showFull)
                    uc.GetTrack("errors").AddTrackEvents(errorEvents.ToList());
                else
                    uc.GetTrack("errors").AddTrackEvents(errorEvents.TakeDistributed(showLimitedViewElementCount, (lst) => { Random r = new Random(); return lst.ToDictionary(x => x.eventStart.ToFileTime() + r.Next()); }).ToList());
            }
            catch { }


            //warnings
            //============================
            try
            {
                var warningEvents = datastore.generalLogData.messageWarnings
                                        .Select(t =>
                                            new TimelineTrackEvent(
                                                    StringHelper.ShortenText(t.message.messageText),
                                                    t.dtTimestamp,
                                                    t.message));

                Color colWarn = Color.SaddleBrown;// Color.FromArgb(255, 150, 13);
                uc.AddPointTrack("warnings", "error messages", colWarn);

                if (showFull)
                    uc.GetTrack("warnings").AddTrackEvents(warningEvents.ToList());
                else
                    uc.GetTrack("warnings").AddTrackEvents(warningEvents.TakeDistributed(showLimitedViewElementCount, (lst) => { Random r = new Random(); return lst.ToDictionary(x => x.eventStart.ToFileTime() + r.Next()); }).ToList());
                
            }
            catch { }


            //we do not want the TimeTrace UC to popup its own message box, we'd like to use the lower panel context line UC
            uc.showPopupOnTrackClick = false;
            uc.TrackClicked += new EventHandler<TimelineTrackEventClickArgs>((object o, TimelineTrackEventClickArgs args) =>
               {
                   try
                   {
                       if (args.timelineTrackTextMessage != null)
                           contextLinesUc.SetData(args.timelineTrackTextMessage);
                       else if (args.timelineTrackText != null)
                       {
                           TextMessage tm = new TextMessage(new TextLocator("info"), $"\n{args.timelineTrackText}");
                           contextLinesUc.SetData(tm);
                       }
                   }
                   catch (Exception e)
                   {
                       ExceptionHandler.Instance.HandleException(e);
                   }
               });


            //finally put the timetrace control into the panel
            upperPanelControl.Add(uc);
            lowerPanelControl.Add(contextLinesUc);
        }

        private void ExportProjectionJobs(string prefix, Func<Projection, bool> filter, TimeTraceUC uc)
        {
            Color LCol = Color.DarkBlue; //line
            Color TCol1 = Color.Green; //track 
            Color TCol2 = Color.Green;
            Color ECol1 = Color.Yellow; //Event
            Color ECol2 = Color.Yellow;

            switch (prefix)
            {
                case "Sync":
                    TCol1 = Color.Green; TCol2 = Color.DarkGray;
                    ECol1 = Color.FromArgb(70, 210, 70); ECol2 = Color.DarkGray;
                    break;

                case "SyncX":
                    TCol1 = Color.Brown; TCol2 = Color.RosyBrown;
                    ECol1 = TCol1.Darken(-64); ECol2 = Color.DarkGray;
                    break;

                case "AdHoc":
                    TCol1 = Color.Orange; TCol2 = Color.DarkOrange;
                    ECol1 = Color.Orange; ECol2 = Color.Wheat;
                    break;
            }

            var proj = datastore.projectionActivity.projections.Where(filter);
            foreach (var pr in proj)
            {
                uc.AddBlockTrack(prefix + pr.uuid, pr.GetLabel(true), LCol, TCol1, TCol2, ECol1, ECol2);

                List<TimelineTrackEvent> stepList = new List<TimelineTrackEvent>();
                foreach (var step in pr.projectionSteps)
                    stepList.Add(new TimelineTrackEvent(prefix + " step " + step.stepId, step.dtTimestampStart, step.dtTimestampEnd) { additionalData = step.ToString() });

                uc.GetTrack(prefix + pr.uuid)
                    .AddTrackEvents(stepList);
            }
        }
    }
}
