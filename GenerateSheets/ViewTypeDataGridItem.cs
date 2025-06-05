using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.GenerateSheets
{
    public class ViewTypeDataGridItem : INotifyPropertyChanged
    {
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
        //public string TypeName { get; set; }
        public List<ViewFamilyType> ViewFamilyTypes { get; set; }
        public List<View> ViewTemplates { get; set; }
        public ViewType ViewType { get; set; }
        public ViewFamily ViewFamily { get; set; }
        public ViewFamilyType SelectedViewFamilyType { get; set; }
        public View SelectedViewTemplate { get; set; }
        public int PreSelectedTemplateIndex { get; set; } = 0;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ViewTypeDataGridItem()
        {

        }

        private void FindPreselectedTemplateIndex()
        {
            int index;
            switch (ViewType)
            {
                case ViewType.FloorPlan:
                     index = ViewTemplates.FindIndex(e => e.Name == "TTT_LPH5_A_FP");
                    if(index > 0)
                    {
                        PreSelectedTemplateIndex = index;
                    }
                    break;
                case ViewType.CeilingPlan:
                    index = ViewTemplates.FindIndex(e => e.Name == "TTT_LPH5_A_RCP");
                    if (index > 0)
                    {
                        PreSelectedTemplateIndex = index;
                    }
                    break;
                case ViewType.Elevation:
                    index = ViewTemplates.FindIndex(e => e.Name == "TTT_LPH5_A_EL");
                    if (index > 0)
                    {
                        PreSelectedTemplateIndex = index;
                    }
                    break;
                case ViewType.Section:
                    index = ViewTemplates.FindIndex(e => e.Name == "TTT_LPH5_A_EL");
                    if (index > 0)
                    {
                        PreSelectedTemplateIndex = index;
                    }
                    break;
                case ViewType.ThreeD:
                    index = ViewTemplates.FindIndex(e => e.Name == "TTT_LPH5_A_3D");
                    if (index > 0)
                    {
                        PreSelectedTemplateIndex = index;
                    }
                    break;
                default:
                    break;
            }
        }

        public void GetViewFamilyTypes(Document doc)
        {
            FilteredElementCollector ficol = new FilteredElementCollector(doc);
            ViewFamilyTypes = ficol.OfClass(typeof(ViewFamilyType)).Select(e => e as ViewFamilyType).Where(e => e.ViewFamily == ViewFamily).ToList();
        }

        public void GetViewTemplates(Document doc)
        {
            FilteredElementCollector ficol = new FilteredElementCollector(doc);
            if(ViewType == ViewType.Elevation || ViewType == ViewType.Section)
            {
                ViewTemplates = ficol.OfClass(typeof(View)).Select(e => e as View).Where(e => (e.ViewType == ViewType || e.ViewType == ViewType.Section || e.ViewType == ViewType.Elevation) && e.IsTemplate).ToList();
            }
            else
            {
                ViewTemplates = ficol.OfClass(typeof(View)).Select(e => e as View).Where(e => e.ViewType == ViewType && e.IsTemplate).ToList();
            }
            FindPreselectedTemplateIndex();
        }
    }
}
