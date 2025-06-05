using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTTRevitTools.ITwoExport;

namespace TTTRevitTools.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class ITwoExportCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            ExportViewModel exportViewModel = App.Instance.ExportViewModel;
            exportViewModel.CheckCategories(commandData.Application.ActiveUIDocument.Document);
            exportViewModel.TryGetExcelKeynotes();
            exportViewModel.TryGetOptions();
            exportViewModel.ResetDataGrids();
            exportViewModel.SetDefaultPreviewColumns();

            iTwoExportWindow window = new iTwoExportWindow(exportViewModel, commandData.Application.MainWindowHandle);
            window.Show();
            return Result.Succeeded;
        }
    }
}
