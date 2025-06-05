using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using TTTRevitTools.SelectionMonitor;

namespace TTTRevitTools.RvtExtEvents
{
    public class SelectionMonitorExEventHandler : IExternalEventHandler
    {
        private static MonitorOnIdling _monitorOnIdling;
        private static bool _subscribed;

        public void Execute(UIApplication app)
        {
            if (_subscribed)
            {
                _monitorOnIdling.SelectionChanged -= SelectionChangedEvent;
                App.UIContApp.Idling -= _monitorOnIdling.OnIdlingEvent;
                _subscribed = false;
            }
            else
            {
                if (_monitorOnIdling == null)
                {
                    _monitorOnIdling = new MonitorOnIdling();
                }
                _monitorOnIdling.SelectionChanged += SelectionChangedEvent;
                App.UIContApp.Idling += _monitorOnIdling.OnIdlingEvent;
                _subscribed = true;
            }
        }

        public string GetName()
        {
            return "SelectionMonitor";
        }

        private static void SelectionChangedEvent(object sender, EventArgs e)
        {
            if (_monitorOnIdling.SelectedElementIds == null)
            {
                App.Instance.BatchPrintViewModel.TheWindow.BtnPrint.IsEnabled = false;
                return;
            }			
            List<ElementId> elementIds = new List<ElementId>(_monitorOnIdling.SelectedElementIds);
            if (elementIds.Count == 0)
            {
                App.Instance.BatchPrintViewModel.TheWindow.BtnPrint.IsEnabled = false;
            }
            else
            {
                App.Instance.BatchPrintViewModel.TheWindow.BtnPrint.IsEnabled = true;
            }
            App.Instance.BatchPrintViewModel.GetSelectedViewSheets(elementIds);           
            App.Instance.BatchPrintViewModel.RefreshViewItems();
            //MessageBox.Show("test");
        }
    }
}
