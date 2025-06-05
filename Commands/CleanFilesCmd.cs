using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;
using TTTRevitTools.FileCleaner;

namespace TTTRevitTools.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CleanFilesCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
           // MessageBox.Show("Clean directories started!");
            //FileCleanerWindow window = new FileCleanerWindow();
            //window.CreateTestLinks();
            //window.ShowDialog();
            FileCleanerModel fileCleanerModel = FileCleanerModel.Initialize();
            fileCleanerModel.DisplayWindow();
            return Result.Succeeded ;
        }
    }
}
