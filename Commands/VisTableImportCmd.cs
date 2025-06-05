using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTTRevitTools.AdvancedElementSelector;
using TTTRevitTools.RvtExtEvents;
using TTTRevitTools.VisTableImport;

namespace TTTRevitTools.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class VisTableImportCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                VisTableAppModel viewModel = App.Instance.VisTableAppModel;
                if (commandData.Application.ActiveUIDocument != null) viewModel.GetUiDocReference(commandData.Application.ActiveUIDocument);
                if(viewModel.VisTableAppWindow == null)
                {
                    viewModel.ProjectSettingsManager = ProjectSettingsManager.Initialize();
                    viewModel.ProjectSettingsManager.ReadSharedParameters(commandData.Application);
                    VisTableAppWindow window = new VisTableAppWindow(viewModel);
                    viewModel.VisTableAppWindow = window;
                    window.Show();
                }
                else
                {
                    viewModel.VisTableAppWindow.Show();
                }
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
