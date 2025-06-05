using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using RevitEvents = Autodesk.Revit.DB.Events;
using Forms = System.Windows.Forms;
using System.Text;
using SpreadsheetLight;
using System.IO.Compression;
using System.Net;
using DocumentFormat.OpenXml.Office2013.Excel;

namespace TTTRevitTools.ConvoidOpenings
{
    public class ConvoidViewModel : INotifyPropertyChanged
    {
        public ElementId SelectedId { get; set; }

        private string _updaterSummaryInfo;
        public string UpdaterSummaryInfo
        {
            get { return _updaterSummaryInfo; }
            set
            {
                if (_updaterSummaryInfo != value)
                {
                    _updaterSummaryInfo = value;
                    OnPropertyChanged(nameof(UpdaterSummaryInfo));
                }
            }
        }
        public List<HostReferenceLookup> HostReferenceLookups { get; set; }
        public static List<SingleCoords> CoordsTransform { get; set; }
        public ObservableCollection<UpdaterFamily> UpdaterFamilies { get; set; }
        public UpdaterFamily SingleChecked { get; set; }
        public List<string> UniqueIdentifiers { get; set; }
        public Dictionary<string, double> ModelLevels { get; set; }
        public List<Element> SelectedOpeningElements { get; set; }
        public ObservableCollection<ExportCoordinateData> ExportCoordinates { get; set; }
        private string _updaterStatus;
        public string UpdaterStatus
        {
            get { return _updaterStatus; }
            set
            {
                if(_updaterStatus != value)
                {
                    _updaterStatus = value;
                    OnPropertyChanged(nameof(UpdaterStatus));
                }
            }
        }

        public List<UpdaterRvtModel> UpdaterModels { get; set; }
        public List<LinkedModelData> HostLinkedModels { get; set; }
        public List<LinkedModelData> ReferenceLinkedModels { get; set; }
        public List<Grid> HorizontalGrids { get; set; }
        public List<Grid> VerticalGrids { get; set; }
        public RevitLinkManager RevitLinkManager { get; set; }

        private List<Definition> _definitions;
        
        private string _logInfo;
        public string LogInfo
        {
            get { return _logInfo; }
            set
            {
                if( _logInfo != value )
                {
                    _logInfo = value;
                    OnPropertyChanged(nameof(LogInfo));
                }
            }
        }

        public ConvoidOpeningsAction Action { get; set; }
        public ExternalEvent TheEvent { get; set; }

        private static string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TTTOpeningUpdater");
        private static string modelsFileName = "updater_models.json";
        private static string sharedParameterFileName = "OpeningUpdaterSharedParameters.txt";

        private BackgroundWorker _workerOnStart;
        private UIDocument _uidoc;

        internal OpeningUpdater OpeningUpdater { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ConvoidViewModel() 
        {
            ViewModelInitialize();
            CoordsTransform = new List<SingleCoords>();
            GetDefaultCoords();
        }

        public async void DownloadAndExtractData()
        {
            try
            {
                string savePath = Path.Combine(appDataPath, "opening_updater.zip");
                if (Directory.Exists(appDataPath)) return;
                Directory.CreateDirectory(appDataPath);
                string downloadPath = "http download link";
                Uri uri = new Uri(downloadPath);
                using (var client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(uri, savePath);
                }
                ZipFile.ExtractToDirectory(savePath, appDataPath);
                LogInfo += "Successfully downloaded iupdater files..." + Environment.NewLine;
            }
            catch (Exception ex)
            {

                LogInfo += ex.ToString() + Environment.NewLine;
            }

        }

        public void AddCurentModel()
        {
            if (_openedDocument == null) return;
            UpdaterRvtModel updaterRvtModel = new UpdaterRvtModel() { Name = _openedDocument.Title, Guid = "NA" };
            if (_openedDocument.IsWorkshared)
            {
                var guid = _openedDocument?.WorksharingCentralGUID;
                updaterRvtModel.Guid = guid.ToString();
            }
            UpdaterModels.Add(updaterRvtModel);
        }

        public void GetDefaultCoords()
        {
            CoordsTransform.Clear();
            CoordsTransform.Add(new SingleCoords() { Name = "BasisX", X = 0, Y = 1, Z = 0 });
            CoordsTransform.Add(new SingleCoords() { Name = "BasisY", X = -1, Y = 0, Z = 0 });
            CoordsTransform.Add(new SingleCoords() { Name = "BasisZ", X = 0, Y = 0, Z = 1 });
            CoordsTransform.Add(new SingleCoords() { Name = "Vector", X = 7564.528, Y = 5373.974, Z = 0 });
        }

        private void ViewModelInitialize()
        {
            UpdaterSummaryInfo = "Not activated";
            UpdaterStatus = "Status: Inactive";
            ExportCoordinates = new ObservableCollection<ExportCoordinateData>();
            SelectedOpeningElements = new List<Element>();
            HostLinkedModels = new List<LinkedModelData>();
            ReferenceLinkedModels = new List<LinkedModelData>();
            RevitLinkManager = new RevitLinkManager();
            UpdaterFamilies = new ObservableCollection<UpdaterFamily>();
            HostReferenceLookups = new List<HostReferenceLookup>
            {
                new HostReferenceLookup()
                {
                    HostLookup = "Host Elements",
                    ReferenceLookup = "Reference Elements"
                },
                new HostReferenceLookup()
                {
                    HostLookup = "TTT - Building Element Id",
                    ReferenceLookup = "TTT - MEP Element Id"
                }
            };
        }

        public void ViewModelReinitialize()
        {
            UpdaterSummaryInfo = "Not activated";
            UpdaterStatus = "Status: Inactive";
            ExportCoordinates.Clear();
            SelectedOpeningElements.Clear();
            HostLinkedModels.Clear();
            ReferenceLinkedModels.Clear();
            RevitLinkManager = new RevitLinkManager();
            UpdaterFamilies.Clear();
        }

        public void SearchOpeningFamilies(Document doc)
        {           
            List<Family> genericFamilies = new FilteredElementCollector(doc).OfClass(typeof(Family)).Select(e => e as Family).Where(e => e.FamilyCategory?.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel).ToList();
            foreach (var f in genericFamilies.OrderBy(e => e.Name))
            {
                if (UpdaterFamilies.Select(e => e.Name).Contains(f.Name)) continue;
                UpdaterFamily family = UpdaterFamily.Initialize(f);
                UpdaterFamilies.Add(family);
            }

            try
            {
                string familiesPath = Path.Combine(appDataPath, "updater_families.json");
                if(File.Exists(familiesPath))
                {
                    string jsonString = File.ReadAllText(familiesPath);
                    List<UpdaterFamily> families = JsonConvert.DeserializeObject<List<UpdaterFamily>>(jsonString);
                    foreach (UpdaterFamily family in UpdaterFamilies) 
                    {
                        UpdaterFamily savedFamily = families.Where(e => e.Name == family.Name).FirstOrDefault();
                        if (savedFamily != null) family.IsChecked = savedFamily.IsChecked;
                    }
                }
            }
            catch (Exception ex)
            {
                LogInfo += ex.ToString() + Environment.NewLine;
            }
        }

        public void SaveUpdaterFamilies()
        {
            try
            {
                string familiesPath = Path.Combine(appDataPath, "updater_families.json");
                string jsonString = JsonConvert.SerializeObject(UpdaterFamilies.Where(e => e.IsChecked).ToList());
                File.WriteAllText(familiesPath, jsonString);
            }
            catch (Exception ex)
            {
                LogInfo += ex.ToString() + Environment.NewLine;
            }
        }

        public void UpdateOpeningsData(List<FamilyInstance> openingFamilies)
        {
            foreach (FamilyInstance fi in openingFamilies)
            {
                try
                {
                    OpeningModel op = OpeningModel.Initialize(fi);
                    op.SetModifiedOpeningParameters();
                    op.GetLocationPoint();
                    op.GetHostElementsData(HostLinkedModels, HostReferenceLookups);
                    op.GetReferenceElementsData(ReferenceLinkedModels, HostReferenceLookups);
                    op.SetGridsData(HorizontalGrids, VerticalGrids);
                    op.SetLevelsData(ModelLevels);
                    string addedId = op.SetUniqueIdentifier(UniqueIdentifiers);
                    if (addedId != null) UniqueIdentifiers.Add(addedId);
                }
                catch (Exception ex)
                {
                    LogInfo += fi.Symbol.Name + ": " + fi.Id.IntegerValue.ToString() + Environment.NewLine + ex.ToString() + Environment.NewLine;
                }

            }
        }

        public void AddExportedOpeningData()
        {
            //58518ccf-2ed6-46cd-b03e-56bd2397ddd4 - opening type string Round, Rectangular
            int index = GetSeqNumber(ExportCoordinates.Select(e => e.SeqNumber).ToList());
            foreach (var item in SelectedOpeningElements)
            {
                string oType = GetParameterValue("58518ccf-2ed6-46cd-b03e-56bd2397ddd4", "DEV_OpeningType", item);
                string gridInfo = GetParameterValue("71e80743-7892-44e3-9b9f-3b0867ff9e23", "TTT - Nearest Grid Intersection", item);
                string discipline = GetParameterValue("fc977b48-f0b7-47c3-8cb0-11747978d80f", "TTT - MEP Discipline", item);
                string coords = GetParameterValue("301cbf36-67d2-4017-b572-653622865592", "DEV_BL_TopCoordinates", item);

                if (oType == "Round")
                {
                    string size = new string(GetParameterValue("6943c8a0-7c60-4716-ba22-ee426e3a9b90", "TTT - Opening Size", item).ToArray());
                    ExportCoordinateData data = new ExportCoordinateData(item.Id)
                    {
                        GridInfo = String.Format("[{0}]", gridInfo), //71e80743-7892-44e3-9b9f-3b0867ff9e23
                        Size = String.Format("[{0}ø]", size.Split(new char[] {'x','X' }).FirstOrDefault()), //6943c8a0-7c60-4716-ba22-ee426e3a9b90
                        Discipline = String.Format("[{0}]", discipline),  //fc977b48-f0b7-47c3-8cb0-11747978d80f
                        Fixed = 99,
                        Coords = coords //301cbf36-67d2-4017-b572-653622865592
                    };
                    data.SeqNumber = String.Format("{0}.0", index);
                    ExportCoordinates.Add(data);
                    index++;
                }

                if (oType == "Rectangular")
                {
                    string size = GetParameterValue("6943c8a0-7c60-4716-ba22-ee426e3a9b90", "TTT - Opening Size", item);
                    if (coords.Split(';').Length == 5)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            string singleCoords = coords.Split(';')[i];

                            ExportCoordinateData data = new ExportCoordinateData(item.Id)
                            {
                                GridInfo = String.Format("[{0}]", gridInfo), //71e80743-7892-44e3-9b9f-3b0867ff9e23
                                Size = String.Format("[{0}x{1}]", size.Split('x')[0], size.Split('x')[1]), //6943c8a0-7c60-4716-ba22-ee426e3a9b90
                                Discipline = String.Format("[{0}]", discipline),  //fc977b48-f0b7-47c3-8cb0-11747978d80f
                                Fixed = 99,
                                Coords = singleCoords.Replace(Environment.NewLine, "") //301cbf36-67d2-4017-b572-653622865592
                            };
                            data.SeqNumber = String.Format("{0}.{1}", index, i + 1);
                            ExportCoordinates.Add(data);
                        }
                        index++;
                    }

                }

            }
            SelectedOpeningElements.Clear();
        }

        public void SaveExportedOpeningsAs()
        {
            try
            {
                Forms.SaveFileDialog saveFileDialog = new Forms.SaveFileDialog();
                saveFileDialog.Filter = "PKT Files (*.pkt)|*.pkt";
                saveFileDialog.DefaultExt = "csv";
                saveFileDialog.AddExtension = true;
                saveFileDialog.FileName = String.Format("{0}_CoordinatesExport_{1}", _openedDocument.Title, DateTime.Now.ToString("dd-MM-yy_hh-mm-ss"));

                if (saveFileDialog.ShowDialog() == Forms.DialogResult.OK)
                {
                    using (StreamWriter sw = new StreamWriter(new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write), Encoding.UTF8))
                    {
                        foreach (ExportCoordinateData data in ExportCoordinates)
                        {
                            string line = data.SeqNumber + data.GridInfo.Replace(" ", "") + data.Size + data.Discipline + " " + data.Fixed + " " + data.Coords;
                            sw.WriteLine(line);
                        }
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                LogInfo += ex.ToString() + Environment.NewLine;
            }

        }

        private string GetParameterValue(string guid, string parameterName, Element element)
        {
            string value = "N/A";
            Parameter p1 = element.get_Parameter(new Guid(guid));
            if(p1 == null)
            {
                Parameter p2 = element.LookupParameter(parameterName);
                value = p2.AsString();
            }
            else
            {
                value = p1.AsString();
            }
            return value;
        }

        private int GetSeqNumber(List<string> seqNumbers)
        {
            List<string> indexes = seqNumbers.Select(e => e.Split('.')[0]).Distinct().ToList();
            return indexes.Count + 1;
        }

        public void CheckAppData()
        {
            if(!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);
        }

        private Document _openedDocument;
        private Document _openedLinkDocument;

        public void GetOpenedLinkModelData(Document openedLinkDocument)
        {
            _openedLinkDocument = openedLinkDocument;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWorkOnLinkLoaded;
            worker.RunWorkerAsync();
        }

        public void GetLinkedModelsData(Document openedDocument)
        {
            _openedDocument = openedDocument;
            _workerOnStart = new BackgroundWorker();
            _workerOnStart.DoWork += Worker_DoWorkOnStart;
            _workerOnStart.WorkerReportsProgress = true;
            _workerOnStart.ProgressChanged += Worker_OnStartProgressChanged;
            _workerOnStart.RunWorkerAsync();
        }

        public List<Definition> GetSharedParameters(Application app)
        {
            _definitions = new List<Definition>();
            string sharedParameterGroupName = "UpdaterParameters";
            string TTTParameters = "TTTOpenings";
            string shPath = Path.Combine(appDataPath, sharedParameterFileName);
            app.SharedParametersFilename = shPath;
            DefinitionFile definitionFile = app.OpenSharedParameterFile();

            List<DefinitionGroup> defGroups = definitionFile.Groups.Where(e => e.Name == sharedParameterGroupName || e.Name == TTTParameters).ToList();
            if (defGroups.Count == 0) return _definitions;

            foreach (DefinitionGroup defGroup in defGroups)
            {
                foreach (Definition definition in defGroup.Definitions.OrderBy(e => e.Name))
                {
                    _definitions.Add(definition);
                }
            }
            return _definitions;
        }

        public void SaveUpdaterModels()
        {
            string filePath = Path.Combine(appDataPath, modelsFileName);
            string jsonString = JsonConvert.SerializeObject(UpdaterModels);
            try
            {
                File.WriteAllText(filePath, jsonString);
                LogInfo += "Saved updater models data..." + Environment.NewLine;
            }
            catch (Exception ex)
            {
                LogInfo += "Can't save udpater models data..." + Environment.NewLine + ex.ToString() + Environment.NewLine;
            }
        }

        public void GetUpdaterModels()
        {
            string filePath = Path.Combine(appDataPath, modelsFileName);
            if (File.Exists(filePath))
            {
                try
                {
                    string jsonString = File.ReadAllText(filePath);
                    UpdaterModels = JsonConvert.DeserializeObject<List<UpdaterRvtModel>>(jsonString);
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", ex.ToString());
                    LogInfo += "Error while deserializing updater models json file..." + Environment.NewLine + ex.ToString() + Environment.NewLine;
                    UpdaterModels = new List<UpdaterRvtModel>();
                }
            }
            else
            {
                UpdaterModels = new List<UpdaterRvtModel>();
            }
        }

        public void CheckSingleFamily(Application app, Document doc)
        {
            if (app != null)
            {
                List<string> info = new List<string>();
                if (_definitions == null || _definitions.Count == 0) GetSharedParameters(app);

                    SingleChecked.CheckDefinitions(_definitions, doc);
                    if (SingleChecked.HasMissingDefinitions()) info.Add(SingleChecked.Name);
                
                if (info.Count > 0)
                {
                    string warning = "Following families are missing parameters required by Opening Updater!"
                                        + Environment.NewLine + Environment.NewLine + String.Join(Environment.NewLine, info)
                                            + Environment.NewLine + Environment.NewLine + "Would you like to load parameters now?";
                    var result = TaskDialog.Show("Warning", warning, TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);
                    if (result == TaskDialogResult.Yes)
                    {
                        if (SingleChecked.HasMissingDefinitions())
                        {
                            Family newFamily = SingleChecked.AddDefinitions(doc);
                        SingleChecked.SetFamily(newFamily);
                        }                  
                    }
                }
            }
        }

        public void CheckUpdaterFamilies(Application app, Document doc)
        {
            if (app != null)
            {
                List<string> info = new List<string>();
                if(_definitions == null || _definitions.Count == 0) GetSharedParameters(app);
                foreach (UpdaterFamily uf in UpdaterFamilies.Where(x => x.IsChecked))
                {
                    uf.CheckDefinitions(_definitions, doc);
                    if (uf.HasMissingDefinitions()) info.Add(uf.Name);
                }
                if (info.Count > 0)
                {
                    string warning = "Following families are missing parameters required by Opening Updater!"
                                        + Environment.NewLine + Environment.NewLine + String.Join(Environment.NewLine, info)
                                            + Environment.NewLine + Environment.NewLine + "Would you like to load parameters now?";
                    var result = TaskDialog.Show("Warning", warning, TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);
                    if (result == TaskDialogResult.Yes)
                    {
                        foreach (UpdaterFamily uf in UpdaterFamilies.Where(x => x.IsChecked))
                        {
                            if (uf.HasMissingDefinitions())
                            {
                                Family newFamily = uf.AddDefinitions(doc);
                                uf.SetFamily(newFamily);
                            }
                        }
                    }
                }
            }
        }

        public void ControlledApplication_DocumentOpened(object sender, RevitEvents.DocumentOpenedEventArgs e)
        {
            LogInfo += "Document opened: " + e.Document.Title + Environment.NewLine;
            if (e.Document != null)
            {    
                if(_openedDocument == null) _openedDocument = e.Document;
                if (UpdaterModels.Select(x => x.Name).Contains(e.Document.Title))
                {
                    if (OpeningUpdater != null)
                    {
                        TaskDialog.Show("Warning", "Can't activate. Opening updater has been activated for another document!");
                    }
                    else
                    {
                        OpeningUpdater = OpeningUpdater.Initialize(e.Document, this);
                        OpeningUpdater.ActivateUpdater("Opening updater auto activation...");
                        GetLinkedModelsData(e.Document);
                        SearchOpeningFamilies(e.Document);
                        //CheckUpdaterFamilies(sender as Application, e.Document);
                        UpdaterSummaryInfo = "Activated for: " + e.Document.Title;
                        e.Document.Application.FamilyLoadedIntoDocument += Application_FamilyLoadedIntoDocument;
                        e.Document.DocumentClosing += Document_DocumentClosing;
                    }
                }
            }
            else
            {
                LogInfo += "Opening updater initializing failed..." + Environment.NewLine;
            }
        }

        private void Application_FamilyLoadedIntoDocument(object sender, RevitEvents.FamilyLoadedIntoDocumentEventArgs e)
        {
            try
            {
                SearchOpeningFamilies(e.Document);
            }
            catch (Exception ex)
            {
                LogInfo += ex.ToString() + Environment.NewLine;
            }
        }

        public void Document_DocumentClosing(object sender, RevitEvents.DocumentClosingEventArgs e)
        {
            LogInfo += "Document closing: " + e.Document.Title + Environment.NewLine;
            if (e.Document != null)
            {
                if (UpdaterModels.Select(x => x.Name).Contains(e.Document.Title))
                {
                    OpeningUpdater.DeactivateUpdater("Deactivated on document closing...");
                    OpeningUpdater = null;
                    ViewModelReinitialize(); //Reinitialize objects
                    e.Document.DocumentClosing -= Document_DocumentClosing;
                }
            }
            else
            {
                LogInfo += "Opening updater deactivation failed..." + Environment.NewLine;
            }
        }

        private bool? IsHorizontal(Grid grid)
        {
            Line gridLine = grid.Curve as Line;
            if (gridLine == null)
            {
                TaskDialog.Show("Error", "Grid is not a line!" + Environment.NewLine + grid.Id.IntegerValue.ToString());
            }
            XYZ direction = gridLine.Direction;
            XYZ absDirection = new XYZ(Math.Abs(direction.X), Math.Abs(direction.Y), Math.Abs(direction.Z));
            if (absDirection.IsAlmostEqualTo(new XYZ(1, 0, 0)))
            {
                return true;
            }
            if (absDirection.IsAlmostEqualTo(new XYZ(0, 1, 0)))
            {
                return false;
            }
            else
            {
                return null;
            }
        }

        private void Worker_OnStartProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            LogInfo += e.UserState.ToString() + Environment.NewLine;

            foreach (LinkedModelData data in RevitLinkManager.Loaded)
            {
                if (data.IsReferenceModel()) ReferenceLinkedModels.Add(data);
                if (data.IsHostModel()) HostLinkedModels.Add(data);
            }

            foreach (LinkedModelData data in RevitLinkManager.Local)
            {
                if (data.IsReferenceModel() && !ReferenceLinkedModels.Select(x => x.Name).Contains(data.Name)) ReferenceLinkedModels.Add(data);
                if (data.IsHostModel() && !HostLinkedModels.Select(x => x.Name).Contains(data.Name)) HostLinkedModels.Add(data);
            }

            foreach (LinkedModelData data in RevitLinkManager.NotLoaded)
            {
                if (data.IsReferenceModel() && !ReferenceLinkedModels.Select(x => x.Name).Contains(data.Name)) ReferenceLinkedModels.Add(data);
                if (data.IsHostModel() && !HostLinkedModels.Select(x => x.Name).Contains(data.Name)) HostLinkedModels.Add(data);
            }
        }

        private void GetUniqueIdentifiers(Document openedDocument)
        {
            UniqueIdentifiers = new List<string>();
            Guid pGuid = new Guid("f0261f82-a41f-4571-ac69-73660a6ede5f"); //f0261f82-a41f-4571-ac69-73660a6ede5f unique identifier parameter
            List <FamilyInstance> openingsInstances = new FilteredElementCollector(openedDocument).OfClass(typeof(FamilyInstance))
                                                                                                    .Where(e => e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel)
                                                                                                        .Select(e => e as FamilyInstance).Where(e => e.get_Parameter(pGuid) != null).ToList();
            foreach (FamilyInstance fi in openingsInstances)
            {
                Parameter p = fi.get_Parameter(pGuid);
                if (!String.IsNullOrEmpty(p.AsString())) UniqueIdentifiers.Add(p.AsString());
            }
        }

        public void SetViewTagRefs(Document doc)
        {
            string[] _familyNames = UpdaterFamilies.Where(e => e.IsChecked).Select(e => e.Name).ToArray();
            var openingInstances = new FilteredElementCollector(doc, doc.ActiveView.Id).OfClass(typeof(FamilyInstance))
                                                                    .Select(e => e as FamilyInstance)
                                                                    .Where(e => _familyNames.Contains(e.Symbol.Family.Name)).ToList();
            List<int> usedTagRefsOnView = GetAllTagRefs(doc);

            using(Transaction tx = new Transaction(doc, "Set TagRefs"))
            {
                tx.Start();
                int tagRefIndex = 1;
                foreach (FamilyInstance fi in openingInstances.OrderBy(e => (e.Location as LocationPoint).Point.Z).OrderBy(e => (e.Location as LocationPoint).Point.Y).OrderBy(e => (e.Location as LocationPoint).Point.X))
                {
                    Guid pGuid = new Guid("ddea1a78-1945-4111-9b1e-30663895d189"); //ddea1a78-1945-4111-9b1e-30663895d189 unique tag ref
                    Parameter p = fi.get_Parameter(pGuid);
                    if (p == null) continue;
                    var tagref = p.AsString();
                    if (Int32.TryParse(tagref, out var tag))
                    {
                        continue;
                    }
                    else
                    {
                        while (usedTagRefsOnView.Contains(tagRefIndex))
                        {
                            tagRefIndex++;
                        }
                        p.Set(tagRefIndex.ToString());
                        usedTagRefsOnView.Add(tagRefIndex);
                    }
                    
                }
                tx.Commit();
            }
        }

        public void ExportOpenings(List<FamilyInstance> familyInstances)
        {
            Dictionary<string, Guid> exportParameters = new Dictionary<string, Guid>();
            exportParameters.Add("TTT - Tag Ref", new Guid("ddea1a78-1945-4111-9b1e-30663895d189")); //TTT - Tag Ref / ddea1a78-1945-4111-9b1e-30663895d189
            exportParameters.Add("TTT - Opening Unique Identifier", new Guid("f0261f82-a41f-4571-ac69-73660a6ede5f"));//TTT - Opening Unique Identifier / f0261f82-a41f-4571-ac69-73660a6ede5f
            exportParameters.Add("TTT - Opening Size", new Guid("6943c8a0-7c60-4716-ba22-ee426e3a9b90"));
            exportParameters.Add("TTT - MEP System Abbreviation", new Guid("9cac14a4-de0a-48e8-ac01-2d408234b5b0"));
            exportParameters.Add("TTT - MEP System", new Guid("5a28e083-f6a2-4620-9f20-67b7fe2bb976"));
            exportParameters.Add("TTT - MEP Type", new Guid("51b64c76-a4d6-457c-9780-6db53e6ff794"));
            exportParameters.Add("TTT - Grid Ref - Horizontal", new Guid("5cb42d75-99fd-4d98-9c06-79a23ac81646"));
            exportParameters.Add("TTT - Grid Ref - Vertical", new Guid("379b6e83-48a3-4678-81ac-6b35176ca510"));
            exportParameters.Add("TTT - Grid Ref - Position EW", new Guid("ba63fd02-087d-4972-9f79-db867613b214"));
            exportParameters.Add("TTT - Grid Ref - Position NS", new Guid("ea1f66c3-0b7a-41f5-8db8-56f1f45b88db"));
            exportParameters.Add("TTT - Relevant Level", new Guid("c6597d59-0383-41d9-a50c-8299f94ff7c7"));
            exportParameters.Add("TTT - Nearest Level Below", new Guid("65d3b25a-8ce6-40a4-ac3f-3ef743f659d2"));
            exportParameters.Add("TTT - Elevation from Nearest Level Below", new Guid("c36f602e-7220-4c63-ae05-933cb59334f7"));
            exportParameters.Add("TTT - Intersection Location", new Guid("7c5efcb5-44f3-4cb7-8da0-8e79769d828b"));
            exportParameters.Add("TTT - Intersection Orientation", new Guid("7ea3a9c8-442e-43ff-8811-634cd93a73a2"));
            exportParameters.Add("TTT - Building Element Category", new Guid("e20e771b-da9a-4070-945a-f581b337775c"));
            exportParameters.Add("TTT - Building Element Id", new Guid("483ee3f4-1fba-46f3-8c9f-07c406e22e0d"));
            exportParameters.Add("TTT - Building Element Type", new Guid("94d26f96-0ddc-465c-b0f2-8d1aa3c61151"));
            exportParameters.Add("TTT - Building Element Source File", new Guid("a88619d6-7faa-4fa0-9b7e-5c64d26945b5"));
            exportParameters.Add("TTT - MEP Element Category", new Guid("6aefa1d1-642a-4c5e-9b4a-bf0a44187f79"));
            exportParameters.Add("TTT - MEP Element Id", new Guid("01c9b9c1-019d-4052-a54a-1a3be6058521"));
            exportParameters.Add("TTT - MEP Element Source File", new Guid("2848d97e-99fe-47da-98b0-ee041eb33a21"));
            exportParameters.Add("TTT - MEP Element Workset", new Guid("87e942d2-3c95-435a-8a87-9f6864f86f73"));
            exportParameters.Add("TTT - MEP Discipline", new Guid("fc977b48-f0b7-47c3-8cb0-11747978d80f"));
            try
            {
                //string path = Path.Combine(appDataPath, "OpeningExportTemplate.xlsx");
                SLDocument sLDocument = new SLDocument();
                int headerColumn = 1;
                foreach (var pair in exportParameters)
                {
                    sLDocument.SetCellValue(1, headerColumn, pair.Key);
                    headerColumn++;
                }
                int startrow = 2;
                int row = 0;
                for (int i = 0; i < familyInstances.Count; i++)
                {
                    row = startrow + i;

                    FamilyInstance fi = familyInstances[i];

                    int column = 1;
                    foreach (var pair in exportParameters)
                    {
                        Parameter p = fi.get_Parameter(pair.Value);
                        if (p != null)
                        {
                            switch(p.StorageType)
                            {
                                case StorageType.String:
                                    string s = p.AsString();
                                    if(!String.IsNullOrEmpty(s)) sLDocument.SetCellValue(row, column, s);
                                    break;
                                case StorageType.Double:
                                    double d = UnitUtils.ConvertFromInternalUnits(p.AsDouble(), UnitTypeId.Millimeters);
                                    sLDocument.SetCellValue(row, column, d);
                                    break;
                                default:
                                    break;
                            }
                        }
                        column++;
                    }
                }

                Forms.SaveFileDialog saveFileDialog = new Forms.SaveFileDialog();
                saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                saveFileDialog.DefaultExt = "xlsx";
                saveFileDialog.AddExtension = true;
                if (saveFileDialog.ShowDialog() == Forms.DialogResult.OK)
                {
                    sLDocument.SaveAs(saveFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                LogInfo += ex.ToString() + Environment.NewLine;
            }

        }

        private List<int> GetAllTagRefs(Document doc)
        {
            List<int> result = new List<int>();
            Guid pGuid = new Guid("ddea1a78-1945-4111-9b1e-30663895d189"); //ddea1a78-1945-4111-9b1e-30663895d189 unique tag ref
            List<FamilyInstance> openingsInstances = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance))
                                                                                                    .Where(e => e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel)
                                                                                                        .Select(e => e as FamilyInstance).Where(e => e.get_Parameter(pGuid) != null).ToList();
            foreach (FamilyInstance fi in openingsInstances)
            {
                Parameter p = fi.get_Parameter(pGuid);
                if (!String.IsNullOrEmpty(p.AsString()))
                {
                    if (Int32.TryParse(p.AsString(), out int refValue))
                    {
                        result.Add(refValue);
                    }
                }
            }
            return result;
        }

        public void RefreshLinksData()
        {
            try
            {
                string documentDataPath = Path.Combine(appDataPath, _openedDocument.Title);
                if (!Directory.Exists(documentDataPath)) Directory.CreateDirectory(documentDataPath);
                List<RevitLinkInstance> rvtLinks = new FilteredElementCollector(_openedDocument).OfClass(typeof(RevitLinkInstance)).Select(x => x as RevitLinkInstance).ToList();
                RevitLinkManager.ReadLinkDataFiles(documentDataPath);
                foreach (RevitLinkInstance l in rvtLinks) RevitLinkManager.AddLinkData(l);
                LogInfo += "Refreshing link model data..." + Environment.NewLine;

                foreach (LinkedModelData data in RevitLinkManager.Loaded)
                {
                    string jsonString = JsonConvert.SerializeObject(data);
                    string filePath = Path.Combine(documentDataPath, data.Name + ".json");
                    File.WriteAllText(filePath, jsonString);
                }
                foreach (LinkedModelData data in RevitLinkManager.Loaded)
                {
                    if (data.IsReferenceModel() && !ReferenceLinkedModels.Select(x => x.Name).Contains(data.Name)) ReferenceLinkedModels.Add(data);
                    if (data.IsHostModel() && !HostLinkedModels.Select(x => x.Name).Contains(data.Name)) HostLinkedModels.Add(data);
                }

                foreach (LinkedModelData data in RevitLinkManager.Local)
                {
                    if (data.IsReferenceModel() && !ReferenceLinkedModels.Select(x => x.Name).Contains(data.Name)) ReferenceLinkedModels.Add(data);
                    if (data.IsHostModel() && !HostLinkedModels.Select(x => x.Name).Contains(data.Name)) HostLinkedModels.Add(data);
                }

                foreach (LinkedModelData data in RevitLinkManager.NotLoaded)
                {
                    if (data.IsReferenceModel() && !ReferenceLinkedModels.Select(x => x.Name).Contains(data.Name)) ReferenceLinkedModels.Add(data);
                    if (data.IsHostModel() && !HostLinkedModels.Select(x => x.Name).Contains(data.Name)) HostLinkedModels.Add(data);
                }
            }
            catch (Exception ex)
            {
                LogInfo += ex.ToString() + Environment.NewLine;
            }
        }

        private void DoBackgroundWork()
        {
            string documentDataPath = Path.Combine(appDataPath, _openedDocument.Title);
            if (!Directory.Exists(documentDataPath)) Directory.CreateDirectory(documentDataPath);
            List<RevitLinkInstance> rvtLinks = new FilteredElementCollector(_openedDocument).OfClass(typeof(RevitLinkInstance)).Select(x => x as RevitLinkInstance).ToList();
            RevitLinkManager.ReadLinkDataFiles(documentDataPath);
            foreach (RevitLinkInstance l in rvtLinks) RevitLinkManager.AddLinkData(l);
            _workerOnStart.ReportProgress(10, "Updating links data...");

            foreach (LinkedModelData data in RevitLinkManager.Loaded)
            {
                string jsonString = JsonConvert.SerializeObject(data);
                string filePath = Path.Combine(documentDataPath, data.Name + ".json");
                LogInfo += "Saving link model data: " + data.Name + Environment.NewLine;
                File.WriteAllText(filePath, jsonString);
            }

            GetGridsData(_openedDocument);
            GetLevelData(_openedDocument);
            GetUniqueIdentifiers(_openedDocument);

            LogInfo += "Data reading completed. Opening updater ready..." + Environment.NewLine;
            Action = ConvoidOpeningsAction.AddDevParameters;
            TheEvent.Raise();
        }

        private void Worker_DoWorkOnStart(object sender, DoWorkEventArgs e)
        {
            DoBackgroundWork();
        }

        private void Worker_DoWorkOnLinkLoaded(object sender, DoWorkEventArgs e)
        {
            //work on single link loaded
        }

        public static double DistanceToProjectBase = 0;

        private void GetLevelData(Document doc)
        {
            List<Level> levels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().Select(x => x as Level).ToList();
            ModelLevels = new Dictionary<string, double>();
            foreach (var level in levels)
            {
                ModelLevels[level.Name] = level.ProjectElevation;
            }
            Level testLevel = levels.FirstOrDefault();
            if (testLevel != null) DistanceToProjectBase = testLevel.ProjectElevation - testLevel.Elevation;
        }

        public void GetGridsData(Document doc)
        {
            List<Grid> hostGrids = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Grids).WhereElementIsNotElementType().Select(x => x as Grid).ToList();
            HorizontalGrids = new List<Grid>();
            VerticalGrids = new List<Grid>();

            foreach (Grid grid in hostGrids)
            {
                double parsingResult = 0;
                if (double.TryParse(grid.Name, out parsingResult)) continue;
                if (IsHorizontal(grid) == true)
                {
                    HorizontalGrids.Add(grid);
                }
                if (IsHorizontal(grid) == false)
                {
                    VerticalGrids.Add(grid);
                }
            }
        }
    }
}
