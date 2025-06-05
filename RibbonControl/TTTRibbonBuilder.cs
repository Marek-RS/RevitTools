using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.IO;

namespace TeslaRevitTools.RibbonControl
{
    public class TeslaRibbonBuilder
    {
        public Autodesk.Windows.RibbonTab RibbonTab { get; set; }
        public bool TabExists { get; set; }

        const string ribbonTabName = "Tesla Tools .NET";
        UIControlledApplication _uiApp;
        RibbonPanel _ribbonPanelProject;
        RibbonPanel _ribbonPanelGeneral;
        RibbonPanel _ribbonPanelSettings;
        RibbonPanel _ribbonPanelGigabase;


        public TeslaRibbonBuilder(UIControlledApplication uiApp)
        {
            _uiApp = uiApp;
        }

        public void FindCreateTeslaRibbonTab()
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
            _ribbonPanelGigabase = _uiApp.CreateRibbonPanel(ribbonTabName, "Giga Tools");
            _ribbonPanelSettings = _uiApp.CreateRibbonPanel(ribbonTabName, "Settings");
        }

        public void AddBatchPrintButton()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Batch Print", "Batch Print", assemblyPath, "TeslaRevitTools.Commands.BatchPrintCmd");
            pushButton.ToolTip = "Exports selected sheets to pdf and dwg";
            pushButton.LargeImage = GetEmbeddedImage("TeslaRevitTools.Resources.batch_print.png");
            _ribbonPanelProject.AddItem(pushButton);
        }

        public void AddCleanFilesButton()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Clean Files", "Clean Directories", assemblyPath, "TeslaRevitTools.Commands.CleanFilesCmd");
            pushButton.ToolTip = "Deletes files older than n-days from selected directories on revit shut down";
            pushButton.LargeImage = GetEmbeddedImage("TeslaRevitTools.Resources.file_cleaner.png");
            _ribbonPanelGeneral.AddItem(pushButton);
        }

        public void AddFamilyRenameButton()
        {
            string exAssemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyPath = Path.Combine(Path.GetDirectoryName(exAssemblyPath), "CustomRevitTools.dll");
            PushButtonData pushButton = new PushButtonData("Rename families", "Rename families", assemblyPath, "CustomRevitTools.ChangeFamilyNames");
            pushButton.ToolTip = "Renames loaded families names";
            pushButton.LargeImage = GetEmbeddedImage("TeslaRevitTools.Resources.family_rename.png");
            _ribbonPanelProject.AddItem(pushButton);
        }

        public void AddTemplateOverridesButton()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Transfer Filters", "Transfer Filters", assemblyPath, "TeslaRevitTools.Commands.TemplateOverridesCmd");
            pushButton.ToolTip = "Transfers selected filters between view templates";
            pushButton.LargeImage = GetEmbeddedImage("TeslaRevitTools.Resources.filter_transfer.png");
            _ribbonPanelProject.AddItem(pushButton);
        }

        public void AddSettingsButton()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Settings", "Settings", assemblyPath, "TeslaRevitTools.Commands.SettingsCmd");
            pushButton.ToolTip = "Choose settings, update";
            pushButton.LargeImage = GetEmbeddedImage("TeslaRevitTools.Resources.settings.png");
            pushButton.AvailabilityClassName = "TeslaRevitTools.RibbonControl.BtnAlwaysAvailable";
            _ribbonPanelSettings.AddItem(pushButton);
        }

        public void AddPathFinderButton()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Pathfinder", "Pathfinder", assemblyPath, "TeslaRevitTools.Commands.PathFinderCmd");
            pushButton.ToolTip = "Finds paths between two piping system elements";
            pushButton.LargeImage = GetEmbeddedImage("TeslaRevitTools.Resources.tesla_icon_active.png");
            _ribbonPanelProject.AddItem(pushButton);
        }

        public void AddFindSheetsBtn()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("FindSheets", "FindSheets", assemblyPath, "TeslaRevitTools.Commands.FindSheetsCmd");
            pushButton.ToolTip = "Finds sheets containing selected elements";
            pushButton.LargeImage = GetEmbeddedImage("TeslaRevitTools.Resources.find_sheets_32.png");
            _ribbonPanelProject.AddItem(pushButton);
        }

        public void AddGenerateSheetsButton()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Rooms to Views", "Rooms to Views", assemblyPath, "TeslaRevitTools.Commands.CreateViewsCmd");
            pushButton.ToolTip = "Creates views and sheets based on Rooms";
            pushButton.LargeImage = GetEmbeddedImage("TeslaRevitTools.Resources.room_to_views.png");
            _ribbonPanelProject.AddItem(pushButton);
        }

        public void AddModelOptimiserButton()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("ModelOptimiser", "ModelOptimiser", assemblyPath, "TeslaRevitTools.Commands.ModelOptimiserCmd");
            pushButton.ToolTip = "Optimises revit usage";
            pushButton.LargeImage = GetEmbeddedImage("TeslaRevitTools.Resources.tesla_icon_active.png");
            _ribbonPanelGeneral.AddItem(pushButton);
        }

        public void AddVisTableImportButton()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("VisTableImport", "VisTableImport", assemblyPath, "TeslaRevitTools.Commands.VisTableImportCmd");
            pushButton.ToolTip = "VisTable xml import";
            pushButton.LargeImage = GetEmbeddedImage("TeslaRevitTools.Resources.vistable_logo.png");
            pushButton.AvailabilityClassName = "TeslaRevitTools.RibbonControl.BtnAlwaysAvailable";
            _ribbonPanelProject.AddItem(pushButton);
        }

        public void AddViewTemplatesManager()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Templates Manager", "Templates Manager", assemblyPath, "TeslaRevitTools.Commands.ViewTemplatesManager");
            pushButton.ToolTip = "Manage view templates";
            pushButton.LargeImage = GetEmbeddedImage("TeslaRevitTools.Resources.tesla_icon_active.png");
            _ribbonPanelProject.AddItem(pushButton);
        }

        public void AddTagQualityTool()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Tag Quality Check", "Tag Quality Check", assemblyPath, "TeslaRevitTools.Commands.TaqQualityCmd");
            pushButton.ToolTip = "Check equipment tags";
            pushButton.LargeImage = GetEmbeddedImage("TeslaRevitTools.Resources.tag_quality_32.png");
            _ribbonPanelGigabase.AddItem(pushButton);
        }

        public void AddGigaBaseTool()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("GigaBase Browser", "GigaBase Browser", assemblyPath, "TeslaRevitTools.Commands.GigaBaseCmd");
            pushButton.ToolTip = "Find GigaBase item in model";
            pushButton.LargeImage = GetEmbeddedImage("TeslaRevitTools.Resources.tesla_icon_active.png");
            _ribbonPanelGigabase.AddItem(pushButton);
        }

        public void AddGridRefTool()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Grid Reference", "Grid Reference", assemblyPath, "TeslaRevitTools.Commands.GridReferenceCmd");
            pushButton.ToolTip = "Add grid reference to elements";
            pushButton.LargeImage = GetEmbeddedImage("TeslaRevitTools.Resources.grid_reference_32.png");
            pushButton.Image = GetEmbeddedImage("TeslaRevitTools.Resources.grid_reference_32.png");
            _ribbonPanelProject.AddItem(pushButton);
        }

        public void AddOpeningsTool()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("Openings Manager", "Opening Manager", assemblyPath, "TeslaRevitTools.Commands.OpeningsCmd");
            pushButton.ToolTip = "Manage openings";
            pushButton.LargeImage = GetEmbeddedImage("TeslaRevitTools.Resources.giga_hole.png");
            pushButton.Image = GetEmbeddedImage("TeslaRevitTools.Resources.giga_hole.png");
            _ribbonPanelGigabase.AddItem(pushButton);
        }

        public void AddITwoExport()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButton = new PushButtonData("iTwo Export", "iTwo Export", assemblyPath, "TeslaRevitTools.Commands.ITwoExportCmd");
            pushButton.ToolTip = "Export data for iTwo";
            pushButton.LargeImage = GetEmbeddedImage("TeslaRevitTools.Resources.itwo_export.png");
            pushButton.Image = GetEmbeddedImage("TeslaRevitTools.Resources.itwo_export.png");
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
