using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LogInsights.Datastore
{
    public interface IDatastoreView
    {
        string BaseKey { get; }
        DataStore Datastore { get; }
        Exporter LogfileFilterExporter { get; }
        Control.ControlCollection UpperPanelControl { get; }
        Control.ControlCollection LowerPanelControl { get; }
        TreeView NavigationTree { get; }

        void ExportView(string key);

        int GetElementCount(string key);

        int SortOrder { get; }

        IEnumerable<string> Build();
    }
}
