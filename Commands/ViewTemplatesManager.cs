using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TTTRevitTools.ViewTemplatesManager;

namespace TTTRevitTools.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class ViewTemplatesManager : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Document doc = commandData.Application.ActiveUIDocument.Document;
                List<View> viewTemplates = new FilteredElementCollector(doc).OfClass(typeof(View)).Select(e => e as View).Where(e => e.IsTemplate).ToList();
                TemplatesManagerViewModel templatesManagerViewModel = TemplatesManagerViewModel.Initialize(viewTemplates, commandData.Application.ActiveUIDocument.Document, commandData.Application.Application.Documents);
                templatesManagerViewModel.MainThread = Thread.CurrentThread;
                return templatesManagerViewModel.DisplayWindow(commandData.Application.MainWindowHandle);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
                return Result.Failed;
            }

        }
    }
}
