using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.GridReference
{
    public class GridRefModel : INotifyPropertyChanged
    {
        public int ElementId { get; set; }
        public string FamilyName { get; set; }
        public string FamilyType { get; set; }

        private PointType _selectedType;
        public PointType SelectedType
        {
            get => _selectedType;
            set
            {
                if (_selectedType != value)
                {
                    _selectedType = value;
                    OnPropertyChanged(nameof(SelectedType));
                }
            }
        }

        private string _gridReference;
        public string GridReference
        {
            get => _gridReference;
            set
            {
                if (_gridReference != value)
                {
                    _gridReference = value;
                    OnPropertyChanged(nameof(GridReference));
                }
            }
        }

        private string _systemInfo;
        public string SystemInfo
        {
            get => _systemInfo;
            set
            {
                if (_systemInfo != value)
                {
                    _systemInfo = value;
                    OnPropertyChanged(nameof(SystemInfo));
                }
            }
        }
        public FamilyInstance FamilyInstance { get; set; }

        KeyValuePair<Grid, double> _hGridDistancePairMin;
        KeyValuePair<Grid, double> _vGridDistancePairMin;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static GridRefModel Initialize(FamilyInstance familyInstance)
        {
            GridRefModel result = new GridRefModel();
            result.SystemInfo = "";
            result.FamilyInstance = familyInstance;
            result.ElementId = familyInstance.Id.IntegerValue;
            result.FamilyName = familyInstance.Symbol.Family.Name;
            result.FamilyType = familyInstance.Symbol.Name;
            result.SelectedType = PointType.BboxMiddle;
            return result;
        }

        public void GetNearestGrids(KeyValuePair<Grid, double> hGridDistancePairMin, KeyValuePair<Grid, double> vGridDistancePairMin)
        {
            if (_hGridDistancePairMin.Equals(new KeyValuePair<Grid, double>()))
            {
                _hGridDistancePairMin = hGridDistancePairMin;
            }
            else
            {
                if (_hGridDistancePairMin.Value > hGridDistancePairMin.Value)
                {
                    _hGridDistancePairMin = hGridDistancePairMin;
                }
            }

            if (_vGridDistancePairMin.Equals(new KeyValuePair<Grid, double>()))
            {
                _vGridDistancePairMin = vGridDistancePairMin;
            }
            else
            {
                if (_vGridDistancePairMin.Value > vGridDistancePairMin.Value)
                {
                    _vGridDistancePairMin = vGridDistancePairMin;
                }
            }
            GridReference = string.Format("{0}/{1}", _hGridDistancePairMin.Key.Name, _vGridDistancePairMin.Key.Name);
        }
    }
}
