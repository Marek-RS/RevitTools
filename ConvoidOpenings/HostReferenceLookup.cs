using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.ConvoidOpenings
{
    public class HostReferenceLookup : INotifyPropertyChanged
    {
        private string _referenceLookup;
        public string ReferenceLookup 
        { 
            get { return _referenceLookup; }
            set
            {
                if(value != _referenceLookup)
                {
                    _referenceLookup = value;
                    OnPropertyChanged(nameof(ReferenceLookup));
                }
            }
        }

        private string _hostLookup;
        public string HostLookup 
        {
            get { return _hostLookup; }
            set
            {
                if (value != _hostLookup)
                {
                    _hostLookup = value;
                    OnPropertyChanged(nameof(HostLookup));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
