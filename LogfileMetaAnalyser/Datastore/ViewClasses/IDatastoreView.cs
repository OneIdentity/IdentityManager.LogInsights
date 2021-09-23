using System;
using System.Windows.Forms;

namespace LogfileMetaAnalyser.Datastore
{
    public interface IDatastoreView
    {
        string BaseKey { get; }
        DataStore datastore { set; }
        Exporter logfileFilterExporter { set; }

        Control.ControlCollection upperPanelControl { set; }
        Control.ControlCollection lowerPanelControl { set; }

        void ExportView(string key);

        int GetElementCount(string key);
    }
}
