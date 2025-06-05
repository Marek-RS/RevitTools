using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTTRevitTools.AdvancedElementSelector;

namespace TTTRevitTools.RvtExtEvents
{
    public class AdvancedSelectorExEventHandler : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            try
            {
                SelectorViewModel selectorViewModel = App.Instance.SelectorViewModel;
                if (selectorViewModel.TheAction == ElementSelectorAction.FilterSelection)
                {
                    selectorViewModel.CheckParameterFilters();
                    selectorViewModel.MainWindow.AlignCheckedElementIds();
                    List<ElementId> ids = selectorViewModel.CheckedElementIds;
                    app.ActiveUIDocument.Selection.SetElementIds(ids);
                }
                if (selectorViewModel.TheAction == ElementSelectorAction.SelectItems)
                {
                    selectorViewModel.CheckParameterFilters();
                    selectorViewModel.MainWindow.AlignCheckedElementIds();
                    List<ElementId> ids = selectorViewModel.CheckedElementIds;
                    app.ActiveUIDocument.Selection.SetElementIds(ids);
                }
                if (selectorViewModel.TheAction == ElementSelectorAction.Subscribe)
                {
                    app.Application.DocumentChanged += Application_DocumentChanged;
                }
                if(selectorViewModel.TheAction == ElementSelectorAction.Unsubscribe)
                {
                    app.Application.DocumentChanged -= Application_DocumentChanged;
                }

            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }
        }

        private void Application_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            try
            {
                App.Instance.SelectorViewModel.ReinitializeViewModel(e.GetDocument());
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }
        }

        public string GetName()
        {
            return "AdvancedSelectorEvent";
        }     
    }
}
