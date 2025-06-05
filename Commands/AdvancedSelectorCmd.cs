using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTTRevitTools.AdvancedElementSelector;
using TTTRevitTools.RvtExtEvents;

namespace TTTRevitTools.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AdvancedSelectorCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                SelectorViewModel selectorViewModel = SelectorViewModel.Initialize(commandData.Application.ActiveUIDocument.Document);
                AdvancedSelectorExEventHandler evHandler = new AdvancedSelectorExEventHandler();
                ExternalEvent theEvent = ExternalEvent.Create(evHandler);
                selectorViewModel.TheEvent = theEvent;
                App.Instance.SelectorViewModel = selectorViewModel;
                selectorViewModel.DisplayWindow();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
                return Result.Failed;
            }

        }
    }
}
