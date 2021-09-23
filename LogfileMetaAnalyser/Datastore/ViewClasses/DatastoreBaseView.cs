using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace LogfileMetaAnalyser.Datastore
{
    public abstract class DatastoreBaseView: IDatastoreView
    {
        protected DatastoreBaseView(TreeView navigationTree,
            Control.ControlCollection upperPanelControl,
            Control.ControlCollection lowerPanelControl,
            DataStore datastore,
            Exporter logfileFilterExporter)
        {
            NavigationTree = navigationTree;
            UpperPanelControl = upperPanelControl;
            LowerPanelControl = lowerPanelControl;
            Datastore = datastore;
            LogfileFilterExporter = logfileFilterExporter;
        }

        public TreeView NavigationTree
        {
            get;
        }


        public DataStore Datastore
        {
            get;
        }

        public Exporter LogfileFilterExporter
        {
            get;
        }

        public Control.ControlCollection UpperPanelControl
        {
            get;
        }

        public Control.ControlCollection LowerPanelControl
        {
            get;
        }

        public abstract int SortOrder 
        { 
            get;
        }


        public abstract void ExportView(string key);

        public abstract int GetElementCount(string key);

        public abstract IEnumerable<string> Build();

        public virtual string BaseKey => "top";

        protected Color GetBgColor(bool conditionForGood)
        {
            return conditionForGood ? Constants.treenodeBackColorNormal : Constants.treenodeBackColorSuspicious;
        }
    }
}
