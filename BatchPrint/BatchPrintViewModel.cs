using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.ComponentModel;
using TTTRevitTools.BatchPrint.GUI;

namespace TTTRevitTools.BatchPrint
{
    public class BatchPrintViewModel : INotifyPropertyChanged
    {
        private bool _printPdfs { get; set; }
        public bool PrintPdfs
        {
            get => _printPdfs;
            set
            {
                if (_printPdfs != value)
                {
                    _printPdfs = value;
                    OnPropertyChanged(nameof(PrintPdfs));
                }
            }
        }
        private bool _printDwgs { get; set; }
        public bool PrintDwgs
        {
            get => _printDwgs;
            set
            {
                if (_printDwgs != value)
                {
                    _printDwgs = value;
                    OnPropertyChanged(nameof(PrintDwgs));
                }
            }
        }

        private bool _overridePrinterAccess { get; set; }
        public bool OverridePrinterAccess
        {
            get => _overridePrinterAccess;
            set
            {
                if (_overridePrinterAccess != value)
                {
                    _overridePrinterAccess = value;
                    OnPropertyChanged(nameof(OverridePrinterAccess));
                }
            }
        }

        public List<ViewSheetModel> ViewSheetModels { get; set; }
        public ExternalEvent SelectionChangedSubscriber { get; set; }
        public Dictionary<string, string> PrintNameDictionary { get; set; }
        public ExternalEvent PrintingEvent { get; set; }
        public Document ActiveDocument { get; set; }

        private List<ViewSheet> _viewSheets;
        public List<ViewSheet> ViewSheets
        {
            get => _viewSheets;
            set
            {
                if (_viewSheets != value)
                {
                    _viewSheets = value;
                    OnPropertyChanged(nameof(ViewSheets));
                }
            }
        }

        public BatchPrintViewModel()
        {
            ViewSheets = new List<ViewSheet>();
            PrintDwgs = false;
            PrintPdfs = true;
            OverridePrinterAccess = true;
        }

        public PrintSelectionWindow TheWindow;

        public void RefreshViewItems()
        {
            TheWindow.MainDataGrid.Items.Refresh();
        }

        public void GetSelectedViewSheets(List<ElementId> selectedIds)
        {
            ViewSheets = new List<ViewSheet>();
            foreach (ElementId id in selectedIds)
            {
                ViewSheet vs = ActiveDocument.GetElement(id) as ViewSheet;
                if (vs != null) ViewSheets.Add(vs);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static BatchPrintViewModel Initialize(List<ViewSheet> viewSheets)
        {
            BatchPrintViewModel result = new BatchPrintViewModel();
            result.SetViewSheetModels(viewSheets);
            return result;
        }

        private void SetViewSheetModels(List<ViewSheet> viewSheets)
        {
            ViewSheetModels = new List<ViewSheetModel>();
            foreach (ViewSheet vs in viewSheets)
            {
                ViewSheetModel model = ViewSheetModel.Initialize(vs);
                ViewSheetModels.Add(model);
            }
        }

        public void SetPrintNameDictionary()
        {
            PrintNameDictionary = new Dictionary<string, string>();
            foreach (ViewSheet vs in ViewSheets)
            {
                string sheetName = vs.Name.Replace("/", "-");
                var revisionId = vs.GetCurrentRevision();
                var currentRevision = ActiveDocument.GetElement(revisionId);
                string revisionNumber = "00"; // default number if no revision on sheet
                if(currentRevision != null && currentRevision.Id != ElementId.InvalidElementId)
                {
                    revisionNumber = vs.GetRevisionNumberOnSheet(currentRevision.Id);
                    if(revisionNumber.Length == 1)
                    {
                        revisionNumber = "0" + revisionNumber;
                    }
                }                
                string sheetNumber = vs.SheetNumber.Replace("/", "-");
                string predictedNameEN = ActiveDocument.Title + " - Sheet - " + sheetNumber + " - " + sheetName;
                string predictedNameDE = ActiveDocument.Title + " - Plan - " + sheetNumber + " - " + sheetName;

                string desiredName = string.Format("{0}[{1}] - {2}" , sheetNumber, revisionNumber, sheetName );
                PrintNameDictionary.Add(predictedNameEN, desiredName);
                PrintNameDictionary.Add(predictedNameDE, desiredName);
            }
        }
    }
}
