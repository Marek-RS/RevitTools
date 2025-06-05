using Autodesk.Revit.DB;
using System.ComponentModel;

namespace TTTRevitTools.BatchPrint
{
    public class ViewSheetModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Number { get; set; }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        public string SaveFileName { get; set; }
        public ViewSheet ViewSheet { get; set; }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ViewSheetModel()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static ViewSheetModel Initialize(ViewSheet viewSheet)
        {
            ViewSheetModel result = new ViewSheetModel();
            result.Name = viewSheet.Name;
            result.ViewSheet = viewSheet;
            result.Number = viewSheet.SheetNumber;
            result.IsSelected = false;
            result.GenerateSaveFileName();
            return result;
        }

        public void GenerateSaveFileName()
        {
            SaveFileName = "";
        }
    }
}
