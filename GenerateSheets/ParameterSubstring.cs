using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.GenerateSheets
{
    public class ParameterSubstring : INotifyPropertyChanged
    {
        private ParameterOption _parameterOption;
        public ParameterOption ParameterOption
        {
            get => _parameterOption;
            set
            {
                if (_parameterOption != value)
                {
                    _parameterOption = value;
                    OnPropertyChanged(nameof(ParameterOption));
                }
            }
        }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
