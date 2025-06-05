using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.IO;

namespace TTTRevitTools.RibbonControl
{
    public class TTTRibbonBuilder
    {
        public Autodesk.Windows.RibbonTab RibbonTab { get; set; }
        public bool TabExists { get; set; }

        const string ribbonTabName = "TTT Tools .NET";
        UIControlledApplication _uiApp;
        RibbonPanel _ribbonPanelProject;
        RibbonPanel _ribbonPanelGeneral;
        RibbonPanel _ribbonPanelSettings;
        RibbonPanel _ribbonPanelgggbase;


        public TTTRibbonBuilder(UIControlledApplication uiApp)
        {
            _uiApp = uiApp;
        }

        public void FindCreateTTTRibbonTab()
        {
            TabExists = false;
            foreach (Autodesk.Windows.RibbonTab tab in Autodesk.Windows.ComponentManager.Ribbon.Tabs)
            {
                if (tab.Title == ribbonTabName)
                {
                    TabExists = true;
                    RibbonTab = tab;
                    break;
                }
            }
            if(!TabExists)
            {
                _uiApp.CreateRibbonTab(ribbonTabName);
            }           
        }

        public void AddProjectToolsPanel()
        {
            _ribbonPanelProject = _uiApp.CreateRibbonPanel(ribbonTabName, "Project Tools");
            _ribbonPanelGeneral = _uiApp.CreateRibbonPanel(ribbonTabName, "General Tools");
            _ribbonPanelgggbase = _uiApp.CreateRibbonPanel(ribbonTabName, "ggg Tools");
            _ribbonPanelSettings = _uiApp.CreateRibbonPanel(ribbonTabName, "Settings");
        }

        public void AddBatchPrintButton()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Batch Print", "Batch Print", assemblyPath, "TTTRevitTools.Commands.BatchPrintCmd");
            pushButton.ToolTip = "Exports selected sheets to pdf and dwg";
            pushButton.LargeImage = GetEmbeddedImage("TTTRevitTools.Resources.batch_print.png");
            _ribbonPanelProject.AddItem(pushButton);
        }

        public void AddCleanFilesButton()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Clean Files", "Clean Directories", assemblyPath, "TTTRevitTools.Commands.CleanFilesCmd");
            pushButton.ToolTip = "Deletes files older than n-days from selected directories on revit shut down";
            pushButton.LargeImage = GetEmbeddedImage("TTTRevitTools.Resources.file_cleaner.png");
            _ribbonPanelGeneral.AddItem(pushButton);
        }

        public void AddFamilyRenameButton()
        {
            string exAssemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyPath = Path.Combine(Path.GetDirectoryName(exAssemblyPath), "CustomRevitTools.dll");
            PushButtonData pushButton = new PushButtonData("Rename families", "Rename families", assemblyPath, "CustomRevitTools.ChangeFamilyNames");
            pushButton.ToolTip = "Renames loaded families names";
            pushButton.LargeImage = GetEmbeddedImage("TTTRevitTools.Resources.family_rename.png");
            _ribbonPanelProject.AddItem(pushButton);
        }

        public void AddTemplateOverridesButton()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Transfer Filters", "Transfer Filters", assemblyPath, "TTTRevitTools.Commands.TemplateOverridesCmd");
            pushButton.ToolTip = "Transfers selected filters between view templates";
            pushButton.LargeImage = GetEmbeddedImage("TTTRevitTools.Resources.filter_transfer.png");
            _ribbonPanelProject.AddItem(pushButton);
        }

        public void AddSettingsButton()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Settings", "Settings", assemblyPath, "TTTRevitTools.Commands.SettingsCmd");
            pushButton.ToolTip = "Choose settings, update";
            pushButton.LargeImage = GetEmbeddedImage("TTTRevitTools.Resources.settings.png");
            pushButton.AvailabilityClassName = "TTTRevitTools.RibbonControl.BtnAlwaysAvailable";
            _ribbonPanelSettings.AddItem(pushButton);
        }

        public void AddPathFinderButton()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Pathfinder", "Pathfinder", assemblyPath, "TTTRevitTools.Commands.PathFinderCmd");
            pushButton.ToolTip = "Finds paths between two piping system elements";
            pushButton.LargeImage = GetEmbeddedImage("TTTRevitTools.Resources.TTT_icon_active.png");
            _ribbonPanelProject.AddItem(pushButton);
        }

        public void AddFindSheetsBtn()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("FindSheets", "FindSheets", assemblyPath, "TTTRevitTools.Commands.FindSheetsCmd");
            pushButton.ToolTip = "Finds sheets containing selected elements";
            pushButton.LargeImage = GetEmbeddedImage("TTTRevitTools.Resources.find_sheets_32.png");
            _ribbonPanelProject.AddItem(pushButton);
        }

        public void AddGenerateSheetsButton()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Rooms to Views", "Rooms to Views", assemblyPath, "TTTRevitTools.Commands.CreateViewsCmd");
            pushButton.ToolTip = "Creates views and sheets based on Rooms";
            pushButton.LargeImage = GetEmbeddedImage("TTTRevitTools.Resources.room_to_views.png");
            _ribbonPanelProject.AddItem(pushButton);
        }

        public void AddModelOptimiserButton()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("ModelOptimiser", "ModelOptimiser", assemblyPath, "TTTRevitTools.Commands.ModelOptimiserCmd");
            pushButton.ToolTip = "Optimises revit usage";
            pushButton.LargeImage = GetEmbeddedImage("TTTRevitTools.Resources.TTT_icon_active.png");
            _ribbonPanelGeneral.AddItem(pushButton);
        }

        public void AddVisTableImportButton()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("VisTableImport", "VisTableImport", assemblyPath, "TTTRevitTools.Commands.VisTableImportCmd");
            pushButton.ToolTip = "VisTable xml import";
            pushButton.LargeImage = GetEmbeddedImage("TTTRevitTools.Resources.vistable_logo.png");
            pushButton.AvailabilityClassName = "TTTRevitTools.RibbonControl.BtnAlwaysAvailable";
            _ribbonPanelProject.AddItem(pushButton);
        }

        public void AddViewTemplatesManager()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Templates Manager", "Templates Manager", assemblyPath, "TTTRevitTools.Commands.ViewTemplatesManager");
            pushButton.ToolTip = "Manage view templates";
            pushButton.LargeImage = GetEmbeddedImage("TTTRevitTools.Resources.TTT_icon_active.png");
            _ribbonPanelProject.AddItem(pushButton);
        }

        public void AddTagQualityTool()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Tag Quality Check", "Tag Quality Check", assemblyPath, "TTTRevitTools.Commands.TaqQualityCmd");
            pushButton.ToolTip = "Check equipment tags";
            pushButton.LargeImage = GetEmbeddedImage("TTTRevitTools.Resources.tag_quality_32.png");
            _ribbonPanelgggbase.AddItem(pushButton);
        }

        public void AddgggBaseTool()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("gggBase Browser", "gggBase Browser", assemblyPath, "TTTRevitTools.Commands.gggBaseCmd");
            pushButton.ToolTip = "Find gggBase item in model";
            pushButton.LargeImage = GetEmbeddedImage("TTTRevitTools.Resources.TTT_icon_active.png");
            _ribbonPanelgggbase.AddItem(pushButton);
        }

        public void AddGridRefTool()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Grid Reference", "Grid Reference", assemblyPath, "TTTRevitTools.Commands.GridReferenceCmd");
            pushButton.ToolTip = "Add grid reference to elements";
            pushButton.LargeImage = GetEmbeddedImage("TTTRevitTools.Resources.grid_reference_32.png");
            pushButton.Image = GetEmbeddedImage("TTTRevitTools.Resources.grid_reference_32.png");
            _ribbonPanelProject.AddItem(pushButton);
        }

        public void AddOpeningsTool()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Openings Manager", "Opening Manager", assemblyPath, "TTTRevitTools.Commands.OpeningsCmd");
            pushButton.ToolTip = "Manage openings";
            pushButton.LargeImage = GetEmbeddedImage("TTTRevitTools.Resources.ggg_hole.png");
            pushButton.Image = GetEmbeddedImage("TTTRevitTools.Resources.ggg_hole.png");
            _ribbonPanelgggbase.AddItem(pushButton);
        }

        public void AddITwoExport()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("iTwo Export", "iTwo Export", assemblyPath, "TTTRevitTools.Commands.ITwoExportCmd");
            pushButton.ToolTip = "Export data for iTwo";
            pushButton.LargeImage = GetEmbeddedImage("TTTRevitTools.Resources.itwo_export.png");
            pushButton.Image = GetEmbeddedImage("TTTRevitTools.Resources.itwo_export.png");
            _ribbonPanelProject.AddItem(pushButton);
        }

        private BitmapSource GetEmbeddedImage(string name)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                System.IO.Stream stream = assembly.GetManifestResourceStream(name);
                return BitmapFrame.Create(stream);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
