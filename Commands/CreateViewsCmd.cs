using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTTRevitTools.GenerateSheets;

namespace TTTRevitTools.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateViewsCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            GenerateSheetsViewModel generateSheetsViewModel = App.Instance.GenerateSheetsViewModel;
            generateSheetsViewModel.GenerateSheetsAction = GenerateSheetsAction.Initialize;
            generateSheetsViewModel.TheEvent.Raise();
            return Result.Succeeded;
        }
    }
}
