using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.TemplateOverrides
{
    public class TemplateViewModel : INotifyPropertyChanged
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

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public View View { get; set; }
        public List<FilterViewModel> Filters { get; set; }

        public void GetFilters(Document doc)
        {
            Filters = new List<FilterViewModel>();
            List<ElementId> ids = View.GetFilters().ToList();
            foreach (ElementId id in ids)
            {
                ParameterFilterElement parameterFilterElement = doc.GetElement(id) as ParameterFilterElement;
                if (parameterFilterElement == null) continue;
                FilterViewModel model = new FilterViewModel();
                model.ParameterFilterElement = parameterFilterElement;
                model.TheView = View;
                model.IsSelected = false;
                Filters.Add(model);
            }
        }

        public void SetFilters(List<FilterViewModel> filterModels, Document doc)
        {
            using(Transaction tx = new Transaction(doc, "Transfering filters"))
            {
                tx.Start();
                foreach (FilterViewModel filterModel in filterModels)
                {
                    OverrideGraphicSettings ogs = filterModel.TheView.GetFilterOverrides(filterModel.ParameterFilterElement.Id);
                    View.SetFilterOverrides(filterModel.ParameterFilterElement.Id, ogs);
                }
                tx.Commit();
            }
        }
    }
}
