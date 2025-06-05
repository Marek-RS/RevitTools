using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Settings;
using TTTRevitTools.EquipmentTagQuality;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using CefSharp;
using TTTRevitTools.Gggbase;
using MS.WindowsAPICodePack.Internal;

namespace NavisworksGggbaseTTT
{
    public class GggbaseViewModel : INotifyPropertyChanged
    {
        public Thread WindowThread { get; set; }
        public GggbaseAction GggbaseAction { get; set; }
        //public ObservableCollection<ExportedElement> ExportedElements { get; set; }
        public ExportManager ExportManager { get; set; }
        public ChromiumWebBrowser Browser { get; set; }
        public List<object> MarksSelection { get; set; }
        public BackgroundWorker Worker { get; set; }
        public ExternalEvent TheEvent { get; set; }

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

        public GggbaseWindow GggbaseWindow { get; set; }
        public bool IsBusy { get; set; } = false;
        public ManualResetEvent SignalEvent = new ManualResetEvent(false);

        public GggbaseViewModel()
        {
            LogInfo = "View model initialized!" + Environment.NewLine;
        }

        public void InitializeExportManager()
        {
            ExportManager = new ExportManager();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SubscribeSelectionChanged()
        {
            //Application.ActiveDocument.CurrentSelection.Changed += CurrentSelection_Changed;
        }

        public void UnsubscribeSelectionChanged()
        {
            //Application.ActiveDocument.CurrentSelection.Changed -= CurrentSelection_Changed;
        }

        public void SendSelectedElements()
        {
        //    ModelItemCollection selection = Application.ActiveDocument.CurrentSelection.SelectedItems;

        //    List<ExportedElement> exportedElements = new List<ExportedElement>();

        //    foreach (ModelItem item in selection)
        //    {
        //        ExportedElement exportedElement = new ExportedElement();
        //        exportedElement.InitializeInstance(item);
        //        exportedElements.Add(exportedElement);
        //    }
        //    string jsonString = JsonConvert.SerializeObject(exportedElements);
        //    LogInfo += jsonString + Environment.NewLine;
        //    Browser.ExecuteScriptAsync($"importNavisData('{jsonString}')");
        }

        public void ZoomSelection()
        {
            //ComApi.InwOpState10 comState = ComApiBridge.ComApiBridge.State;
            //ModelItemCollection modelItemCollectionIn = new ModelItemCollection(Application.ActiveDocument.CurrentSelection.SelectedItems);
            //ComApi.InwOpSelection comSelectionOut = ComApiBridge.ComApiBridge.ToInwOpSelection(modelItemCollectionIn);
            //comState.ZoomInCurViewOnSel(comSelectionOut);
        }

        private bool IsOfExpectedCategory(Element element)
        {
            if(element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctTerminal 
                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory 
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_MechanicalEquipment 
                        || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctAccessory)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SelectByMarkInCustomParameters(UIDocument uidoc)
        {           
            List<string> searchMarks = new List<string>();
            foreach (object mark in MarksSelection)
            {
                string markString = mark.ToString();
                searchMarks.Add(markString);
            }
            LogInfo += "Marks to select: ";
            foreach (string mark in searchMarks) LogInfo += mark + ", ";
            LogInfo += Environment.NewLine;

            List<string> tagsFound = new List<string>();

            SettingsViewModel settingsViewModel = SettingsViewModel.Initialize();
            settingsViewModel.GetSearchParameters();

            List<Element> elements = new FilteredElementCollector(uidoc.Document).OfClass(typeof(FamilyInstance)).Where(e => IsOfExpectedCategory(e)).ToList();
            List<Element> foundelements = new List<Element>();

            //foreach (var item in settingsViewModel.SearchParameters)
            //{

                    //string pName = item.ParameterName;

                    List<Element> checkList = checkedModelItems.Where(e => e.Properties.Values.Any(x => searchMarks.Any(y => y == x))).Select(e => e.RevitElement).ToList();

                    //List<Element> checkList2 = elements.Where(e => e.LookupParameter(pName) != null && searchMarks.Contains(e.LookupParameter(pName).AsString())).ToList();
                    foreach (Element element in checkList)
                    {
                        //LogInfo += "Found something: " + element.Id + "; " + element.LookupParameter(pName).AsString() + Environment.NewLine;
                        //tagsFound.Add(element.LookupParameter(pName).AsString());
                        foundelements.Add(element);
                    }
                

            //}

            if(foundelements.Count > 0)
            {
                uidoc.Selection.SetElementIds(foundelements.Select(e => e.Id).ToList());
                uidoc.ShowElements(foundelements.Select(e => e.Id).ToList());
            }

            List<string> notFound = searchMarks.Where(e => !tagsFound.Contains(e)).ToList();
            Browser.ExecuteScriptAsync($"console.log('var found = {JsonConvert.SerializeObject(tagsFound)}; var not_found = {JsonConvert.SerializeObject(notFound)};');");
            SignalEvent.Set();
            //Dispatcher.FromThread(MainThread).Invoke(new Action(() =>
            //{
            //    List<ModelItem> modelItems = LoopCustomParameters(searchMarks, ref tagsFound);
            //    LogInfo += String.Format("Found {0} elements meeting search criteria...", modelItems.Count) + Environment.NewLine;
            //    if (modelItems.Count > 0)
            //    { 
            //        Application.ActiveDocument.CurrentSelection.Clear();
            //        Application.ActiveDocument.CurrentSelection.CopyFrom(modelItems);
            //        ZoomSelection();
            //    }
            //    IsBusy = false;
            //    SignalEvent.Set();
            //}));

            //SignalEvent.WaitOne();
            //SignalEvent.Reset();
            //GggbaseWindow.SubscribeEvents();
            //List<string> notFound = searchMarks.Where(e => !tagsFound.Contains(e)).ToList();
            //GggbaseWindow.Browser.ExecuteScriptAsync($"console.log('var found = {JsonConvert.SerializeObject(tagsFound)}; var not_found = {JsonConvert.SerializeObject(notFound)};');");
        }

        List<CheckedModelItem> checkedModelItems = new List<CheckedModelItem>();

        public void CollectModelItems(Document doc)
        {

            checkedModelItems = new List<CheckedModelItem>();
            SettingsViewModel settingsViewModel = SettingsViewModel.Initialize();
            settingsViewModel.GetSearchParameters();

            List<Element> elements = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).Where(e => IsOfExpectedCategory(e)).ToList();

            int pIndex = 1;

            foreach (SearchParameter sp in settingsViewModel.SearchParameters)
            {
                Worker.ReportProgress(0, string.Format("Searching parameters: {0} of {1}", pIndex, settingsViewModel.SearchParameters.Count));
                string categoryName = sp.CategoryName;
                string parameterName = sp.ParameterName;
                int minValueLength = sp.MinLength;



                int progressReportStep = 0;
                int progress = 0;


                foreach (Element item in elements)
                {
                    CheckedModelItem checkedModelItem = new CheckedModelItem(item);
                    string parameterValue = null;

                    Parameter p = item.LookupParameter(parameterName);
                    if (p != null) parameterValue = p.AsString();

                    checkedModelItem.Properties.Add(categoryName + parameterName, parameterValue);

                    progress++;
                    progressReportStep++;
                    if (progressReportStep == 100)
                    {
                        progressReportStep = 0;
                        double percentage = ((double)progress / (double)elements.Count) * 100;
                        Worker.ReportProgress((int)percentage, string.Format("Searching parameters: {0} of {1}", pIndex, settingsViewModel.SearchParameters.Count));
                    }
                    if (parameterValue == null)
                    {
                        continue;
                    }
                    if (string.IsNullOrEmpty(parameterValue) || parameterValue.Length < minValueLength)
                    {
                        continue;
                    }
                    checkedModelItems.Add(checkedModelItem);
                }
                    
                pIndex++;
            }
            Worker.ReportProgress(100, "Ready"); 
            SignalEvent.Set();
        }

        private List<Element> LoopCustomParameters(List<string> searchMarks, ref List<string> found)
        {
            List<Element> result = new List<Element>();
            SettingsViewModel settingsViewModel = SettingsViewModel.Initialize();
            settingsViewModel.GetSearchParameters();

            //if (checkedModelItems.Count > 0)
            //{

            //    foreach (CheckedModelItem item in checkedModelItems)
            //    {
            //        foreach (SearchParameter sp in settingsViewModel.SearchParameters)
            //        {
            //            if (item.Properties.ContainsKey(sp.CategoryName + sp.ParameterName))
            //            {
            //                foreach (string markString in searchMarks)
            //                {
            //                    string value = item.Properties[sp.CategoryName + sp.ParameterName];
            //                    if(value.Length < sp.MinLength + 2) continue;
            //                    if (markString.Contains(value))
            //                    {
            //                        LogInfo += String.Format("{0} - {1}: {2} , item name: {3}", sp.CategoryName, sp.ParameterName, value, item.ModelItem.DisplayName) + Environment.NewLine;
            //                        if (!result.Select(e => e.GetHashCode()).Contains(item.GetHashCode()))
            //                        {
            //                            found.Add(markString);
            //                            result.Add(item.ModelItem);
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            return result;
        }

        public void DisplayWindow()
        {
            Thread windowThread = new Thread(delegate ()
            {
                GggbaseWindow = new GggbaseWindow(this);
                GggbaseWindow.Show();
                Dispatcher.Run();
            });
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.IsBackground = true;
            windowThread.Start();
            WindowThread = windowThread;
        }
    }
}
