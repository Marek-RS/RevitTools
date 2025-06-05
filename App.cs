using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using TTTRevitTools.BatchPrint;
using TTTRevitTools.RibbonControl;
using System.Reflection;
using TTTRevitTools.Pathfinder;
using TTTRevitTools.GenerateSheets;
using TTTRevitTools.RvtExtEvents;
using TTTRevitTools.VisTableImport;
using TTTRevitTools.EquipmentTagQuality;
using NavisworksGggbaseTTT;
using System.Windows.Threading;
using System;
using TTTRevitTools.GridReference;
using TTTRevitTools.Openings;
using TTTRevitTools.ITwoExport;
using TTTRevitTools.FindAffectedSheets;
using TTTRevitTools.ConvoidOpenings;

namespace TTTRevitTools
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class App : IExternalApplication
    {
        #region Configuration and assembly version

#if DEBUG2020 || RELEASE2020
        public const string AssemblyYear = "2020";
#elif DEBUG2023 || RELEASE2023
        public const string AssemblyYear = "2023";
#endif

        //increment on every release build
        public const string AssemblyMinorVersion = "5"; //new functions
        public const string AssemblyRevisionVersion = "7"; //significant fixes and improvements
        public const string AssemblyBuildVersion = "0"; //small fixes and improvements

        public static string PlugInVersion
        {
            get
            {
                return string.Format("{0}.{1}.{2}.{3}",
                                AssemblyYear,
                                AssemblyMinorVersion,
                                AssemblyRevisionVersion,
                                AssemblyBuildVersion);
            }
        }
        #endregion

        public GridRefViewModel GridRefViewModel { get; set; }
        public BatchPrintViewModel BatchPrintViewModel { get; set; }

        public PathfinderViewModel PathfinderViewModel { get; set; }
        public GenerateSheetsViewModel GenerateSheetsViewModel { get; set;}
        public AdvancedElementSelector.SelectorViewModel SelectorViewModel { get; set; }
        //public VisTableViewModel VisTableViewModel { get; set; }
        public VisTableAppModel VisTableAppModel { get; set; }
        public TagQualityViewModel TagQualityViewModel { get; set; }
        public GggbaseViewModel GggbaseViewModel { get; set; }
        public OpeningsViewModel OpeningsViewModel { get; set; }
        public ExportViewModel ExportViewModel { get; set; }
        public FindSheetsViewModel FindSheetsViewModel { get; set; }
        public ConvoidViewModel ConvoidViewModel { get; set; }

        public static UIApplication UIApp;
        public static UIControlledApplication UIContApp;
        internal static App _app = null;
        public static App Instance
        {
            get { return _app; }
        }
        public Result OnStartup(UIControlledApplication app)
        {
            
            _app = this;           
            UIContApp = app;
            UIApp = GetUiApplication();      
            BatchPrintViewModel = new BatchPrintViewModel();
            FindSheetsViewModel = new FindSheetsViewModel();
            TTTRibbonBuilder TTTRibbonBuilder = new TTTRibbonBuilder(app);
            GenerateSheetsViewModel = new GenerateSheetsViewModel();
            CreateViewsExEventHandler createViewsExEventHandler = new CreateViewsExEventHandler();
            ExternalEvent createViewsExEvent = ExternalEvent.Create(createViewsExEventHandler);
            GenerateSheetsViewModel.TheEvent = createViewsExEvent;
            VisTableAppModel = new VisTableAppModel();
            VisTableAppModel.SetExternalEvent();
            GridRefViewModel = new GridRefViewModel();
            OpeningsViewModel = new OpeningsViewModel();
            iTwoExportExEvent iTwoExportExEvent = new iTwoExportExEvent();
            ExportViewModel = ExportViewModel.Initialize(ExternalEvent.Create(iTwoExportExEvent));
            ConvoidViewModel = new ConvoidViewModel();
            ConvoidViewModel.DownloadAndExtractData();
            ConvoidOpeningsEvent convoidOpeningsEvent = new ConvoidOpeningsEvent();
            ConvoidViewModel.TheEvent = ExternalEvent.Create(convoidOpeningsEvent);
            ConvoidViewModel.CheckAppData();
            app.ControlledApplication.DocumentOpened += ConvoidViewModel.ControlledApplication_DocumentOpened;
            //app.ControlledApplication.LinkedResourceOpened += ControlledApplication_LinkedResourceOpened;
            DockPanelModel dockPanel = new DockPanelModel();
            dockPanel.RegisterDockPanel(app, ConvoidViewModel);
            TTTRibbonBuilder.FindCreateTTTRibbonTab();
            TTTRibbonBuilder.AddProjectToolsPanel();
            TTTRibbonBuilder.AddBatchPrintButton();
            TTTRibbonBuilder.AddCleanFilesButton();
            TTTRibbonBuilder.AddFamilyRenameButton();
            TTTRibbonBuilder.AddTemplateOverridesButton();
            TTTRibbonBuilder.AddSettingsButton();
            //TTTRibbonBuilder.AddPathFinderButton();
            TTTRibbonBuilder.AddGenerateSheetsButton();
            TTTRibbonBuilder.AddFindSheetsBtn();
            //TTTRibbonBuilder.AddModelOptimiserButton();
            TTTRibbonBuilder.AddViewTemplatesManager();
            TTTRibbonBuilder.AddTagQualityTool();
            TTTRibbonBuilder.AddGggBaseTool();
            TTTRibbonBuilder.AddGridRefTool();
            TTTRibbonBuilder.AddOpeningsTool();
#if DEBUG2023 || RELEASE2023
            TTTRibbonBuilder.AddVisTableImportButton();
            TTTRibbonBuilder.AddITwoExport();
#endif
            return Result.Succeeded;
        }

        private void ControlledApplication_LinkedResourceOpened(object sender, Autodesk.Revit.DB.Events.LinkedResourceOpenedEventArgs e)
        {
            //TaskDialog.Show("Info", "Opened linked resource");
        }

        public Result OnShutdown(UIControlledApplication app)
        {
            if (GggbaseViewModel != null && GggbaseViewModel.WindowThread != null)
            {
                try
                {
                    if (GggbaseViewModel.GggbaseWindow != null)
                    {
                        Dispatcher.FromThread(GggbaseViewModel.WindowThread).Invoke(new Action(() => { 
                            GggbaseViewModel.GggbaseWindow.Close();
                            GggbaseViewModel.Browser.Dispose();
                        }));
                    }
                    CefSharp.Cef.Shutdown();
                    GggbaseViewModel.WindowThread.Abort();
                }
                catch (Exception ex)
                {

                }
            }
            FileCleaner.FileCleanerModel.DeleteFiles();
            return Result.Succeeded;
        }

        private static UIApplication GetUiApplication()
        {
            var versionNumber = UIContApp.ControlledApplication.VersionNumber;
            var fieldName = string.Empty;
            switch (versionNumber)
            {
                case "2020":
                    fieldName = "m_uiapplication";
                    break;
                case "2023":
                    fieldName = "m_uiapplication";
                    break;
            }
            var fieldInfo = UIContApp.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            var uiApplication = (UIApplication)fieldInfo?.GetValue(UIContApp);
            return uiApplication;
        }
    }
}
