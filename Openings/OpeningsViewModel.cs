using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CefSharp.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TTTRevitTools.Openings
{
    public class OpeningsViewModel : INotifyPropertyChanged
    {
        public IDictionary<string, object> WebInfoDictionary { get; set; }
        public string SelectedMode { get; set; }
        public List<string> SearchModes { get; set; }
        public List<OpeningModel> OpeningModels { get; set; }
        public List<OpeningSymbol> OpeningSymbols { get; set; }
        public OpeningsAction OpeningsAction { get; set; }

        public ManualResetEvent SignalEvent = new ManualResetEvent(false);
        public ExternalEvent TheEvent { get; set; }
        public Thread WindowThread { get; set; }
        OpeningsWindow OpeningsWindow { get; set; }
        public ChromiumWebBrowser Browser { get; set; }

        private string _logInfo;
        public string LogInfo
        {
            get => _logInfo;
            set
            {
                if (_logInfo != value)
                {
                    _logInfo = value;
                    OnPropertyChanged(nameof(LogInfo));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public OpeningsViewModel()
        {

        }

        public void InitializeViewModel(Document doc)
        {
            LogInfo = "View model initialized!" + Environment.NewLine;
            OpeningSymbols = new List<OpeningSymbol>();
            OpeningModels = new List<OpeningModel>();
            List<FamilySymbol> symbols = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Select(e => e as FamilySymbol).Where(e => e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel).ToList();
            foreach (FamilySymbol fs in symbols)
            {
                OpeningSymbol os = new OpeningSymbol(fs);
                OpeningSymbols.Add(os);
            }
            SearchModes = new List<string>() { "Active Document", "Active View", "Current Selection" };

        }

        public void FindOpenings(UIDocument uidoc)
        {
            OpeningModels.Clear();
            switch (SelectedMode)
            {
                case "Active Document":
                    List<FamilyInstance> instancesAll = new FilteredElementCollector(uidoc.Document).OfClass(typeof(FamilyInstance)).Select(e => e as FamilyInstance).Where(e => e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel).ToList();
                    foreach (OpeningSymbol os in OpeningSymbols)
                    {
                        if (os.IsChecked)
                        {
                            foreach (var instance in instancesAll.Where(e => e.Symbol.Id == os.FamilySymbol.Id).ToList())
                            {
                                OpeningModel openingModel = new OpeningModel(instance);
                                openingModel.Initialize(uidoc.Document);
                                OpeningModels.Add(openingModel);
                            }
                        }
                    }
                    break;
                case "Active View":
                    List<FamilyInstance> instancesView = new FilteredElementCollector(uidoc.Document, uidoc.Document.ActiveView.Id).OfClass(typeof(FamilyInstance)).Select(e => e as FamilyInstance).Where(e => e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel).ToList();
                    foreach (OpeningSymbol os in OpeningSymbols)
                    {
                        if (os.IsChecked)
                        {
                            foreach (var instance in instancesView.Where(e => e.Symbol.Id == os.FamilySymbol.Id).ToList())
                            {
                                OpeningModel openingModel = new OpeningModel(instance);
                                openingModel.Initialize(uidoc.Document);
                                OpeningModels.Add(openingModel);
                            }
                        }
                    }
                    break;
                case "Current Selection":
                    List<Element> selectedElements = uidoc.Selection.GetElementIds().Select(e => uidoc.Document.GetElement(e)).ToList();
                    List<FamilyInstance> instancesSelection = selectedElements.Where(e => (e as FamilyInstance) != null).Select(e => e as FamilyInstance).ToList();
                    foreach (OpeningSymbol os in OpeningSymbols)
                    {
                        if (os.IsChecked)
                        {
                            foreach (var instance in instancesSelection.Where(e => e.Symbol.Id == os.FamilySymbol.Id).ToList())
                            {
                                OpeningModel openingModel = new OpeningModel(instance);
                                openingModel.Initialize(uidoc.Document);
                                OpeningModels.Add(openingModel);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            SignalEvent.Set();
        }

        public void SendOpening(OpeningModel model)
        {
            ExportedOpening exportedOpening = model.GetExportedData();
            string jsonText = JsonConvert.SerializeObject(exportedOpening);
            System.Windows.MessageBox.Show(jsonText);

            string url = "http://10.79.27.131:8088/api/openings";
            StringContent httpContent = new StringContent(jsonText, Encoding.UTF8, "application/json");
            HttpClientPost(url, httpContent);
        }

        private void HttpClientPost(string url, StringContent stringContent)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.PostAsync(url, stringContent).Result;
            string responseBody = response.Content.ReadAsStringAsync().Result;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                System.Windows.MessageBox.Show("Success sending elements to GigaHole!");
            }
            else
            {
                System.Windows.MessageBox.Show("Error while sending objects to GigaHole!");
            }
            System.Windows.MessageBox.Show(responseBody);
        }

        public bool SelectOpeningInModel(UIDocument uidoc)
        {
            bool result = false;
            View view = null;
            int elementIdInt = 0;

            object viewName = WebInfoDictionary["ViewName"];
            if(viewName != null)
            {
                view = new FilteredElementCollector(uidoc.Document).OfClass(typeof(View)).Select(e => e as View).Where(e => !e.IsTemplate && e.ViewType != ViewType.DrawingSheet && e.Name == viewName.ToString()).FirstOrDefault();
            }
            object idString = WebInfoDictionary["RevitElementId"];
            if(idString != null)
            {
                elementIdInt = int.Parse(idString.ToString());
            }
            
            if(view != null && elementIdInt > 0)
            {
                ElementId elementId = new ElementId(elementIdInt);
                if(uidoc.Document.GetElement(elementId) as FamilyInstance != null)
                {
                    uidoc.ActiveView = view;
                    uidoc.Selection.SetElementIds(new List<ElementId>() { elementId });
                    uidoc.ShowElements(elementId);
                    result = true;
                }
            }
            else
            {
                TaskDialog.Show("Info!", "Opening not found!");
            }

            SignalEvent.Set();
            return result;
        }

        public void ShowLocationOn3d(UIDocument uidoc)
        {
            object location = WebInfoDictionary["IntersectionLocation"];
            if(location != null)
            {
                string locationString = location.ToString();
                double[] coords = locationString.Split(' ').Select(e => double.Parse(e)).ToArray();
                double x = UnitUtils.ConvertToInternalUnits(coords[0], UnitTypeId.Meters);
                double y = UnitUtils.ConvertToInternalUnits(coords[1], UnitTypeId.Meters);
                double z = UnitUtils.ConvertToInternalUnits(coords[2], UnitTypeId.Meters);
                XYZ intersection = new XYZ(x,y,z);

                BoundingBoxXYZ box = new BoundingBoxXYZ();
                box.Min = new XYZ(-2, -2, -2);
                box.Max = new XYZ(2, 2, 2);
                Transform transform = Transform.CreateTranslation(intersection);

                box.Transform = transform;
                View3D active3dView = uidoc.ActiveView as View3D;
                if(active3dView != null)
                {
                    using (Transaction tx = new Transaction(uidoc.Document, "Set section box"))
                    {
                        tx.Start();
                        active3dView.SetSectionBox(box);
                        active3dView.IsSectionBoxActive = true;
                        tx.Commit();
                    }
                    IList<UIView> uiviews = uidoc.GetOpenUIViews();
                    UIView activeUiView = null;
                    foreach (UIView uiv in uiviews)
                    {
                        View view = uidoc.Document.GetElement(uiv.ViewId) as View;
                        if (view.Id.IntegerValue == active3dView.Id.IntegerValue) activeUiView = uiv;
                    }
                    activeUiView.ZoomToFit();
                }
            }
            SignalEvent.Set();
        }

        public void DisplayWindow()
        {
            Thread windowThread = new Thread(delegate ()
            {
                OpeningsWindow = new OpeningsWindow(this);
                OpeningsWindow.Show();
                Dispatcher.Run();
            });
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.IsBackground = true;
            windowThread.Start();
            WindowThread = windowThread;
        }
    }
}
