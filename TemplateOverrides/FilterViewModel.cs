using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.TemplateOverrides
{
    public class FilterViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        public event PropertyChangedEventHandler PropertyChanged;

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

        public View TheView { get; set; }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ParameterFilterElement ParameterFilterElement { get; set;}
    }
}
