using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTTRevitTools.SelectionMonitor;

namespace TTTRevitTools.RvtExtEvents
{

    public class FindSheetsSelectionMonitorHandler : IExternalEventHandler
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
                //App.Instance.FindSheetsViewModel.TheWindow.CheckButton.IsEnabled = false;
                return;
            }
            List<ElementId> elementIds = new List<ElementId>(_monitorOnIdling.SelectedElementIds);
            if (elementIds.Count == 0)
            {
                //App.Instance.FindSheetsViewModel.TheWindow.CheckButton.IsEnabled = false;
            }
            else
            {
                //App.Instance.FindSheetsViewModel.TheWindow.CheckButton.IsEnabled = true;
            }
            App.Instance.FindSheetsViewModel.GetSelectedElements(elementIds);
        }
    }
}
