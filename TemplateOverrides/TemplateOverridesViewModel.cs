using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.TemplateOverrides
{
    public class TemplateOverridesViewModel
    {
        public List<TemplateViewModel> SourceTemplates { get; set; }
        public List<TemplateViewModel> DestinationTemplates { get; set; }
        public List<FilterViewModel> SelectedFilters { get; set; }

        public Document _doc;

        public static TemplateOverridesViewModel Initialize(Document doc)
        {
            TemplateOverridesViewModel result = new TemplateOverridesViewModel();
            result._doc = doc;
            result.GetSourceTemplates();
            result.GetDestinationTemplates();
            result.SelectedFilters = new List<FilterViewModel>();
            return result;
        }

        private void GetSourceTemplates()
        {
            SourceTemplates = new List<TemplateViewModel>();
            List<View> viewTemplates = new FilteredElementCollector(_doc).OfClass(typeof(View)).Select(e => e as View).Where(e => e.IsTemplate).ToList();
            foreach (View view in viewTemplates)
            {
                if (!view.AreGraphicsOverridesAllowed()) continue;
                TemplateViewModel templateViewModel = new TemplateViewModel();
                templateViewModel.View = view;
                templateViewModel.IsSelected = false;
                templateViewModel.GetFilters(_doc);
                SourceTemplates.Add(templateViewModel);
            }
            SourceTemplates = SourceTemplates.OrderBy(e => e.View.Name).ToList();
        }

        private void GetDestinationTemplates()
        {
            DestinationTemplates = new List<TemplateViewModel>();
            List<View> viewTemplates = new FilteredElementCollector(_doc).OfClass(typeof(View)).Select(e => e as View).Where(e => e.IsTemplate).ToList();
            foreach (View view in viewTemplates)
            {
                if (!view.AreGraphicsOverridesAllowed()) continue;
                TemplateViewModel templateViewModel = new TemplateViewModel();
                templateViewModel.View = view;
                templateViewModel.IsSelected = false;
                DestinationTemplates.Add(templateViewModel);
            }
            DestinationTemplates = DestinationTemplates.OrderBy(e => e.View.Name).ToList();
        }
    }
}
