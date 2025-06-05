using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace TTTRevitTools.AdvancedElementSelector
{
    public class ExpanderModel : INotifyPropertyChanged
    {
        public Category Category { get; set; }
        public List<ParameterModel> ElementParameters { get; set; }
        public string ExpanderName { get; set; }
        public ElementId ElementId { get; set; }
        public Element Element { get; set; }
        private ObservableCollection<ExpanderModel> _subExpanders;

        public ObservableCollection<ExpanderModel> SubExpanders
        {
            get => _subExpanders;
            set
            {
                if (_subExpanders != value)
                {
                    _subExpanders = value;
                    OnPropertyChanged(nameof(SubExpanders));
                }
            }
        }

        private int _itemsCount;
        public int ItemsCount
        {
            get => _itemsCount;
            set
            {
                if (_itemsCount != value)
                {
                    _itemsCount = value;
                    OnPropertyChanged(nameof(ItemsCount));
                }
            }
        }

        private bool? _isSelected;
        public bool? IsSelected
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

        public ExpanderModel()
        {
            IsSelected = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool GetParameters()
        {
            ElementParameters = new List<ParameterModel>();
            Element element = SubExpanders.FirstOrDefault()?.SubExpanders.FirstOrDefault()?.Element;
            if (element == null) return false;
            foreach (Parameter p in element.Parameters)
            {
                if (ElementParameters.Select(e => e.Parameter.Id).Contains(p.Id)) continue;
                ParameterModel parameterModel = new ParameterModel(p);
                parameterModel.GetDisplayUnitType();
                switch (p.StorageType)
                {
                    case StorageType.None:
                        break;
                    case StorageType.Integer:
                        parameterModel.Operators = new List<string>() { "equals", "does not equal" };
                        ElementParameters.Add(parameterModel);
                        break;
                    case StorageType.Double:
                        parameterModel.Operators = new List<string>() { "equals", "does not equal", "is greater than", "is less than" };
                        ElementParameters.Add(parameterModel);
                        break;
                    case StorageType.String:
                        parameterModel.Operators = new List<string>() { "equals", "does not equal", "contains", "does not contain" };
                        ElementParameters.Add(parameterModel);
                        break;
                    case StorageType.ElementId:
                        parameterModel.Operators = new List<string>() { "equals", "does not equal", "contains", "does not contain" };
                        ElementParameters.Add(parameterModel);
                        break;
                    default:
                        break;
                }
            }
            ElementParameters = ElementParameters.OrderBy(e => e.ParameterName).ToList();
            return true;
        }
    }
}
