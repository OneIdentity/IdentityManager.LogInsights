using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

using LogfileMetaAnalyser.Helpers;
using LogfileMetaAnalyser.Controls;


namespace LogfileMetaAnalyser.Datastore
{
    class StatisticsStoreView : DatastoreBaseView, IDatastoreView
    {
        public StatisticsStoreView() { }

        public override string BaseKey => $"{base.BaseKey}/Statistics";

        public int GetElementCount(string key)
        {
            return -1;
        }

        public void ExportView(string key)
        {
            if (!key.StartsWith(BaseKey))
                return;

            int posY = 1;

            if (key == BaseKey || key == BaseKey + "/1")
            {
                ListViewUC uc = new ListViewUC();
                uc.Size = new Size(upperPanelControl.Owner.Width, 200);
                uc.Location = new Point(1, 1);
                uc.SetupCaption("Parsing statistics for " + datastore.statistics.filesParsed + " file(s)");
                uc.SetupHeaders(new string[] { "Filename", "Filesize (KB)", "read/parse duration", "duration in sum" });

                foreach (var item in datastore.statistics.parseStatistic)
                    uc.AddItemRow(item.filename, new string[] {
                            item.filename,
                            item.filesizeKb.ToString("N"),
                            TimeSpan.FromMilliseconds(item.readAndParseFileDuration).ToHumanString(),
                            TimeSpan.FromMilliseconds(item.overallDuration).ToHumanString()
                    });

                uc.Resume();
                upperPanelControl.Add(uc);
                posY = 201; ;
            }

            if (key == BaseKey || key == BaseKey + "/2")
            {
                ListViewUC uc = new ListViewUC();
                uc.Size = new Size(upperPanelControl.Owner.Width, 200);
                uc.Location = new Point(1, posY + 40);
                uc.SetupCaption("Statistics for all participated detectors");
                uc.SetupHeaders(new string[] { "Detector name", "# lines parsed", "# detections", "parse duration", "finalize duration" });

                foreach (var item in datastore.statistics.detectorStatistics)
                    uc.AddItemRow(item.detectorName, new string[] {
                            item.detectorName,
                            item.numberOfLinesParsed.ToString(),
                            item.numberOfDetections.ToString(),
                            TimeSpan.FromMilliseconds(item.parseDuration).ToHumanString(),
                            TimeSpan.FromMilliseconds(item.finalizeDuration).ToHumanString()
                    });

                uc.Resume();
                upperPanelControl.Add(uc);
            }

            if (upperPanelControl.Count > 1)
                foreach (Control uc in upperPanelControl)
                    uc.Dock = DockStyle.None;
        }

    }
}
