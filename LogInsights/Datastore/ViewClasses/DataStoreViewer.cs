using LogInsights.Composition;
using LogInsights.ExceptionHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;


// ReSharper disable LocalizableElement
namespace LogInsights.Datastore
{
    public class DataStoreViewer
    {      
        private readonly DataStore datastore;
        private readonly Exporter logfileFilterExporter;
        private readonly ResultUcCache ucCache = new();
        private readonly Form ownerForm;
        private readonly Control.ControlCollection upperPanelControl;
        private readonly Control.ControlCollection lowerPanelControl;

        private readonly Dictionary<string, IDatastoreView> responsibleViewerClass = new();


        public DataStoreViewer(DataStore datastore, Exporter logfileFilterExporter, Form ownerForm, Control.ControlCollection upperPanelControl, Control.ControlCollection lowerPanelControl)
        {
            this.datastore = datastore;
            this.logfileFilterExporter = logfileFilterExporter;
            this.ownerForm = ownerForm;
            this.upperPanelControl = upperPanelControl;
            this.lowerPanelControl = lowerPanelControl;

            datastore.StoreInvalidation += (_, _) => 
            {
                try
                {
                    ucCache.ClearCache();
                }
                catch (Exception e)
                {
                    ExceptionHandler.Instance.HandleException(e);
                }
            };
        }


        public string ExportAsJson()
        {
            if (datastore == null || !datastore.HasData())
                return ("Datastore is empty because no analysis was yet performed!");

            try
            {
                return datastore.AsJson();
            }
            catch (Exception E)
            {
                return $"Export not possible: {E.Message}";
            }
        }

        public void ExportAsNavigationTreeView(TreeView tw)
        {
            try
            {
                tw.BeginUpdate();
                tw.Nodes.Clear();
                responsibleViewerClass.Clear();

                var dataStoreViewers = Composer.BuildRepo<IDatastoreView>(
                        new object[] {
                            tw,
                            upperPanelControl,
                            lowerPanelControl,
                            datastore,
                            logfileFilterExporter
                        },
                        "*Detector.dll")
                    .OrderBy(v => v.SortOrder);

                foreach (IDatastoreView dataStoreViewer in dataStoreViewers)
                {
                    var keys = dataStoreViewer.Build();
                    foreach (string key in keys)
                        responsibleViewerClass.Add(key, dataStoreViewer);
                }
            }
            finally
            {
                if (tw.Nodes.Count > 0)
                    tw.Nodes[0].Expand();
                tw.EndUpdate();
            }
        }


        public async Task ExportAsViewContent(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            await Task.Run(() =>
            {

                Helpers.GuiHelper.SetGuiSave(ownerForm, () =>
                {

                    try
                    {
                        ownerForm.Cursor = Cursors.WaitCursor;

                        upperPanelControl.Clear();
                        lowerPanelControl.Clear();

                        if (ucCache.HasData(key))
                            ucCache.GetFromCache(key, upperPanelControl, lowerPanelControl);
                        else
                        {

                 
                            if (ownerForm.IsDisposed || ownerForm.Disposing)
                                return;

                            if (responsibleViewerClass.TryGetValue(key, out var vc))
                                vc.ExportView(key);
                            else
                            {
                                var viewer = responsibleViewerClass.Where(v => key.StartsWith(v.Value.BaseKey)).ToArray();

                                if (viewer.Length>0)
                                    viewer[0].Value.ExportView(key);
                                else
                                {
                                    //Fallback :(                                    
                                    foreach (var viewercls in responsibleViewerClass.Values.Distinct())
                                    {
                                        viewercls.ExportView(key);

                                        if (upperPanelControl.Count > 0)
                                            break;
                                    }

                                    if (upperPanelControl.Count > 0)
                                        MessageBox.Show($"no viewer class found to handle key '{key}'", Constants.AppDisplay);
                                }
                            }

                            ucCache.AddToCache(key, upperPanelControl, lowerPanelControl);
                      
                            
                        }
               
                    }
                    finally
                    {
                        Helpers.GuiHelper.SetGuiSave(ownerForm, () =>
                        {
                            ownerForm.Cursor = Cursors.Default;
                        });
                    }
                });
            }).ConfigureAwait(false);
        }

    }
}
