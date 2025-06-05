using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTTRevitTools.RvtExtEvents;

namespace TTTRevitTools.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class FindSheetsCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                FindSheetsSelectionMonitorHandler handler = new FindSheetsSelectionMonitorHandler();
                App.Instance.FindSheetsViewModel.SelectionChangedSubscriber = ExternalEvent.Create(handler);
                App.Instance.FindSheetsViewModel.DisplayWindow(commandData.Application);
                return Result.Succeeded;
            }
            catch (Exception)
            {
                return Result.Failed;
            }
        }
    }
}
