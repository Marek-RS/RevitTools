using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.Settings
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public string Version { get; set; }
        private string _updateInfo;
        public string UpdateInfo
        {
            get { return _updateInfo; }
            set
            {
                if (_updateInfo != value)
                {
                    _updateInfo = value;
                    OnPropertyChanged(nameof(UpdateInfo));
                }
            }
        }

        private bool _isUpdateAvailable;
        public bool IsUpdateAvailable
        {
            get { return _isUpdateAvailable; }
            set
            {
                if (_isUpdateAvailable != value)
                {
                    _isUpdateAvailable = value;
                    OnPropertyChanged(nameof(IsUpdateAvailable));
                }
            }
        }

        public SettingsViewModel()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
