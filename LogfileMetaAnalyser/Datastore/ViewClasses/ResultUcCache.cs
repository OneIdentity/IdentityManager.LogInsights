using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using LogfileMetaAnalyser.Helpers;
using LogfileMetaAnalyser.Datastore;

namespace LogfileMetaAnalyser
{
    class ResultUcCache
    {
        public Dictionary<string, Control[]> upperPanelCache = new Dictionary<string, Control[]>();
        public Dictionary<string, Control[]> lowerPanelCache = new Dictionary<string, Control[]>();

        public ResultUcCache()
        { }

        public bool HasData(string key)
        {
            return upperPanelCache.ContainsKey(key) && lowerPanelCache.ContainsKey(key);
        }

        public void AddToCache(string key, Control.ControlCollection upperPanelControl, Control.ControlCollection lowerPanelControl)
        {
            List<Control> lst = new List<Control>();

            for (int i = 0; i < upperPanelControl.Count; i++)
                lst.Add(upperPanelControl[i]);

            upperPanelCache.Add(key, lst.ToArray());

            lst.Clear();
            for (int i = 0; i < lowerPanelControl.Count; i++)
                lst.Add(lowerPanelControl[i]);

            lowerPanelCache.Add(key, lst.ToArray());
        }

        public void GetFromCache(string key, Control.ControlCollection upperPanelControl, Control.ControlCollection lowerPanelControl)
        {
            upperPanelControl.AddRange(upperPanelCache[key].ToArray());
            lowerPanelControl.AddRange(lowerPanelCache[key].ToArray());
        }

        public void ClearCache()
        {
            upperPanelCache.Clear();
            lowerPanelCache.Clear();
        }
    }
}
