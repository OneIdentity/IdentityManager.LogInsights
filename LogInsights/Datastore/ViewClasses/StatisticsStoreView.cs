using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

using LogInsights.Helpers;
using LogInsights.Controls;


namespace LogInsights.Datastore
{
    //prevent loading.. uncomment if needed
    /*
    internal class StatisticsStoreView : DatastoreBaseView
    {
        public StatisticsStoreView(TreeView navigationTree, 
            Control.ControlCollection upperPanelControl,
            Control.ControlCollection lowerPanelControl, 
            DataStore datastore,
            Exporter logfileFilterExporter) :
            base(navigationTree, upperPanelControl, lowerPanelControl, datastore, logfileFilterExporter)
        {
        }

        public override int SortOrder => 600;

        public override IEnumerable<string> Build()
        {
            var result = new List<string>();

            string key = BaseKey;
            TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, "Analyze statistics", "stats", Constants.treenodeBackColorNormal);
            result.Add(key);

            key = $"{BaseKey}/1";
            TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, "Parsing statistics", "stats", Constants.treenodeBackColorNormal);
            result.Add(key);

            key = $"{BaseKey}/2";
            TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, key, "Detectors statistics", "stats", Constants.treenodeBackColorNormal);
            result.Add(key);

            return result;
        }

        public override string BaseKey => $"{base.BaseKey}/Statistics";

        public override int GetElementCount(string key)
        {
            return -1;
        }

        public override void ExportView(string key)
        {
            if (!key.StartsWith(BaseKey))
                return;

            var statisticsStore = Datastore.GetOrAdd<StatisticsStore>();

            int posY = 1;

            if (key == BaseKey || key == BaseKey + "/1")
            {
                ListViewUC uc = new ListViewUC();
                uc.Size = new Size(UpperPanelControl.Owner.Width, 200);
                uc.Location = new Point(1, 1);
                uc.SetupCaption("Parsing statistics for " + statisticsStore.FilesParsed + " file(s)");
                uc.SetupHeaders(new string[] { "Filename", "Filesize (KB)", "read/parse duration", "duration in sum" });

                foreach (var item in statisticsStore.ParseStatistic)
                    uc.AddItemRow(item.filename, new string[] {
                            item.filename,
                            item.filesizeKb.ToString("N"),
                            TimeSpan.FromMilliseconds(item.readAndParseFileDuration).ToHumanString(),
                            TimeSpan.FromMilliseconds(item.overallDuration).ToHumanString()
                    });

                uc.Resume();
                UpperPanelControl.Add(uc);
                posY = 201; ;
            }

            if (key == BaseKey || key == BaseKey + "/2")
            {
                ListViewUC uc = new ListViewUC();
                uc.Size = new Size(UpperPanelControl.Owner.Width, 200);
                uc.Location = new Point(1, posY + 40);
                uc.SetupCaption("Statistics for all participated detectors");
                uc.SetupHeaders(new string[] { "Detector name", "# lines parsed", "# detections", "parse duration", "finalize duration" });

                foreach (var item in statisticsStore.DetectorStatistics)
                    uc.AddItemRow(item.detectorName, new string[] {
                            item.detectorName,
                            item.numberOfLinesParsed.ToString(),
                            item.numberOfDetections.ToString(),
                            TimeSpan.FromMilliseconds(item.parseDuration).ToHumanString(),
                            TimeSpan.FromMilliseconds(item.finalizeDuration).ToHumanString()
                    });

                uc.Resume();
                UpperPanelControl.Add(uc);
            }

            if (UpperPanelControl.Count > 1)
                foreach (Control uc in UpperPanelControl)
                    uc.Dock = DockStyle.None;
        }

    }
    */
}
