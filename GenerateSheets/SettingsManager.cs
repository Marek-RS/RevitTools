using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.GenerateSheets
{
    public static class SettingsManager
    {
        private static List<string> jsonFileNames = new List<string>()
        {
            "titleblock_sheet_parameters.json",
            "sheetview_settings.json",
            "naming_rules_v2.json",
            "modifier_substring.json",
            "modifier_findreplace.json"
        };

        public static void ExportSettings()
        {
            string assemblyDirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string directoryName = dialog.FileName;
                string TTTRevitToolsDirectory = Directory.GetParent(assemblyDirPath).FullName;
                string[] files = Directory.GetFiles(TTTRevitToolsDirectory, "*.json", SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    if(jsonFileNames.Contains(Path.GetFileName(file)))
                    {
                        File.Copy(file, Path.Combine(directoryName, Path.GetFileName(file)), true);
                    }
                }
                Autodesk.Revit.UI.TaskDialog.Show("Success", "Setting files exported!");
            }
            else
            {
                Autodesk.Revit.UI.TaskDialog.Show("Error", "No directory selected!");
            }
        }

        public static void ImportSettings()
        {
            string assemblyDirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dialog.IsFolderPicker = false;
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string TTTRevitToolsDirectory = Directory.GetParent(assemblyDirPath).FullName;
                List<string> files = dialog.FileNames.ToList();
                foreach (string file in files)
                {
                    if (jsonFileNames.Contains(Path.GetFileName(file)))
                    {
                        File.Copy(file, Path.Combine(TTTRevitToolsDirectory, Path.GetFileName(file)), true);
                    }
                }
                Autodesk.Revit.UI.TaskDialog.Show("Success", "Setting files imported!");
            }
            else
            {
                Autodesk.Revit.UI.TaskDialog.Show("Error", "No directory selected!");
            }
        }
    }
}
