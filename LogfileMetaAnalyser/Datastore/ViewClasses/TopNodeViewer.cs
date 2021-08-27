using System; 
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

using LogfileMetaAnalyser.Helpers;
using LogfileMetaAnalyser.Controls;

namespace LogfileMetaAnalyser.Datastore
{
    class TopNodeViewer : DatastoreBaseView, IDatastoreView
    {
        public TopNodeViewer()
        { }

        public int GetElementCount(string key)
        {
            return -1;
        }

        public void ExportView(string key)
        {
            //nothing here yet
            return;
        }
    }
}
