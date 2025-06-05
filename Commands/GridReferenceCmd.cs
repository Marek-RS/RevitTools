using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTTRevitTools.GridReference;

namespace TTTRevitTools.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GridReferenceCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            
            GridRefViewModel gridRefViewModel = App.Instance.GridRefViewModel;
            gridRefViewModel.InitializeViewModel();
            gridRefViewModel.DisplayWindow(commandData.Application.MainWindowHandle);
            return Result.Succeeded;
        }
    }
}
