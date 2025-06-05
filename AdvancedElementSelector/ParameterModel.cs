using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TTTRevitTools.AdvancedElementSelector
{
    public class ParameterModel : INotifyPropertyChanged
    {
        public Parameter Parameter { get; set; }
        public List<string> Operators { get; set; }
        public string SelectedOperator { get; set; }
        public string ParameterName { get; set; }
        public StorageType StorageType { get; set; }
#if DEBUG2020 || RELEASE2020
        public string UnitType { get; set; }
#elif DEBUG2023 || RELEASE2023
        public string UnitType { get; set; }
#endif
        public string DisplayUnitType { get; set; }

        private string _value;
        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public ParameterModel(Parameter parameter)
        {
            Parameter = parameter;
            ParameterName = parameter.Definition.Name;
            StorageType = parameter.StorageType;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void GetDisplayUnitType()
        {
#if DEBUG2020 || RELEASE2020
            UnitType = Enum.GetName(typeof(UnitType), Parameter.Definition.UnitType);
#elif DEBUG2023 || RELEASE2023
            UnitType = Parameter.GetUnitTypeId().TypeId;
#endif

            try
            {
#if DEBUG2020 || RELEASE2020
             DisplayUnitType = Enum.GetName(typeof(UnitType), Parameter.Definition.UnitType);
#elif DEBUG2023 || RELEASE2023
                DisplayUnitType = Parameter.GetUnitTypeId().TypeId;
#endif
            }
            catch (Exception)
            {
                DisplayUnitType = "DUT-Exception (Text)";
            }
        }

        public bool CheckValue(string input)
        {
            bool result = false;
            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show("Parameters with empty value will be ignored!");
                return result;
            }
            switch (StorageType)
            {
                case StorageType.None:
                    break;
                case StorageType.Integer:
                    result = int.TryParse(input, out int integer);
                    if (!result) MessageBox.Show("Integer value was expected!");
                    break;
                case StorageType.Double:
                    result = double.TryParse(input, out double number);
                    if (!result) MessageBox.Show("Double value was expected!");
                    break;
                case StorageType.String:
                    result = true;
                    break;
                case StorageType.ElementId:
                    result = true;
                    break;
                default:
                    break;
            }
            return result;
        }

        public bool CheckParameterWithValue(Parameter checkedParameter)
        {
            bool result = false;
            switch (StorageType)
            {
                case StorageType.Integer:
                    if(SelectedOperator == "equals")
                    {
                        if (checkedParameter.AsInteger() == int.Parse(Value)) result = true;
                    }
                    if (SelectedOperator == "does not equal")
                    {
                        if (checkedParameter.AsInteger() != int.Parse(Value)) result = true;
                    }
                    break;
                case StorageType.Double:
#if DEBUG2020 || RELEASE2020
                    if (SelectedOperator == "equals")
                    {
                        if (checkedParameter.AsDouble() == UnitUtils.ConvertToInternalUnits(double.Parse(Value), Parameter.DisplayUnitType)) result = true;
                    }
                    if (SelectedOperator == "does not equal")
                    {
                        if (checkedParameter.AsDouble() != UnitUtils.ConvertToInternalUnits(double.Parse(Value), Parameter.DisplayUnitType)) result = true;
                    }
                    if (SelectedOperator == "is less than")
                    {
                        if (checkedParameter.AsDouble() < UnitUtils.ConvertToInternalUnits(double.Parse(Value), Parameter.DisplayUnitType)) result = true;
                    }
                    if (SelectedOperator == "is greater than")
                    {
                        if (checkedParameter.AsDouble() > UnitUtils.ConvertToInternalUnits(double.Parse(Value), Parameter.DisplayUnitType)) result = true;
                    }
#elif DEBUG2023 || RELEASE2023
                    if (SelectedOperator == "equals")
                    {
                        if (checkedParameter.AsDouble() == UnitUtils.ConvertToInternalUnits(double.Parse(Value), Parameter.GetUnitTypeId())) result = true;
                    }
                    if (SelectedOperator == "does not equal")
                    {
                        if (checkedParameter.AsDouble() != UnitUtils.ConvertToInternalUnits(double.Parse(Value), Parameter.GetUnitTypeId())) result = true;
                    }
                    if (SelectedOperator == "is less than")
                    {
                        if (checkedParameter.AsDouble() < UnitUtils.ConvertToInternalUnits(double.Parse(Value), Parameter.GetUnitTypeId())) result = true;
                    }
                    if (SelectedOperator == "is greater than")
                    {
                        if (checkedParameter.AsDouble() > UnitUtils.ConvertToInternalUnits(double.Parse(Value), Parameter.GetUnitTypeId())) result = true;
                    }
#endif
                    break;
                case StorageType.ElementId:
                    if (SelectedOperator == "equals")
                    {
                        if (checkedParameter.AsValueString() == Value) result = true;
                    }
                    if (SelectedOperator == "does not equal")
                    {
                        if (checkedParameter.AsValueString() != Value) result = true;
                    }
                    if (SelectedOperator == "contains")
                    {
                        if (checkedParameter.AsValueString()?.Contains(Value) == true) result = true;
                    }
                    if (SelectedOperator == "does not contain")
                    {
                        if (!checkedParameter.AsValueString()?.Contains(Value) == true) result = true;
                    }
                    break;
                default:
                    if (SelectedOperator == "equals")
                    {
                        if (checkedParameter.AsString() == Value) result = true;
                    }
                    if (SelectedOperator == "does not equal")
                    {
                        if (checkedParameter.AsString() != Value) result = true;
                    }
                    if (SelectedOperator == "contains")
                    {
                        if (checkedParameter.AsString()?.Contains(Value) == true) result = true;
                    }
                    if (SelectedOperator == "does not contain")
                    {
                        if (!checkedParameter.AsString()?.Contains(Value) == true) result = true;
                    }
                    break;
            }
            return result;
        }
    }
}
