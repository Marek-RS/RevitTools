using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTTRevitTools.Settings;
using TTTRevitTools.TemplateOverrides;

namespace TTTRevitTools.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SettingsCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                string version = App.PlugInVersion;
                SettingsViewModel viewModel = new SettingsViewModel() { Version = App.PlugInVersion, IsUpdateAvailable = false, UpdateInfo = "Checking..."};
                VersionUpdater versionUpdater = new VersionUpdater();
                versionUpdater.CheckForUpdates(viewModel, App.AssemblyYear);
                versionUpdater.DisplayWindow();

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
