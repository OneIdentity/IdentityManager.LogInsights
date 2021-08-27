using System;
using System.Windows.Forms;
using System.Drawing;

namespace LogfileMetaAnalyser.Datastore
{
    public class DatastoreBaseView
    {
        public DatastoreStructure datastore { set; protected get; }

        public Exporter logfileFilterExporter { set; protected get; }

        public Control.ControlCollection upperPanelControl { get; set; }
        public Control.ControlCollection lowerPanelControl { get; set; }

        //public DatastoreBaseView();

        public virtual string BaseKey => "top";

        protected Color GetBgColor(bool conditionForGood)
        {
            return conditionForGood ? Constants.treenodeBackColorNormal : Constants.treenodeBackColorSuspicious;
        }
    }
}
