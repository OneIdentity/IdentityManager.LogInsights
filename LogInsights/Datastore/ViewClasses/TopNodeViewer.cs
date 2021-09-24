using System.Windows.Forms;
using System.Drawing;
using LogInsights.Helpers;
using System.Collections.Generic;

namespace LogInsights.Datastore
{
    internal class TopNodeViewer : DatastoreBaseView
    {
        public TopNodeViewer(TreeView navigationTree, 
            Control.ControlCollection upperPanelControl,
            Control.ControlCollection lowerPanelControl, 
            DataStore datastore, 
            Exporter logfileFilterExporter) :
            base(navigationTree, upperPanelControl, lowerPanelControl, datastore, logfileFilterExporter)
        {
        }


        public override int GetElementCount(string key)
        {
            return -1;
        }

        public override IEnumerable<string> Build()
        {
            TreeNodeCollectionHelper.CreateNode(NavigationTree.Nodes, BaseKey, "Analyze report", "report", Color.Transparent);
            return new[] { BaseKey };
        }

        public override int SortOrder => 0;

        public override void ExportView(string key)
        {
            //nothing here yet
            return;
        }

    }
}
