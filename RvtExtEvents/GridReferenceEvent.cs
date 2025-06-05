using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTTRevitTools.GridReference;

namespace TTTRevitTools.RvtExtEvents
{
    public class GridReferenceEvent : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            GridRefViewModel viewModel = App.Instance.GridRefViewModel;
            if (viewModel.TheAction == GridRefAction.AddSelection)
            {
                var ids = app.ActiveUIDocument.Selection.GetElementIds();
                Document doc = app.ActiveUIDocument.Document;
                List<FamilyInstance> instances = ids.Select(e => doc.GetElement(e) as FamilyInstance).Where(e => e != null).ToList();
                List<FamilySymbol> symbols = ids.Select(e => doc.GetElement(e) as FamilySymbol).Where(e => e != null).ToList();
                foreach (FamilySymbol fs in symbols)
                {
                    List<FamilyInstance> symbolInstances = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).Select(e => e as FamilyInstance).Where(e => e.Symbol.Id == fs.Id).ToList();
                    instances.AddRange(symbolInstances);
                }
                if (instances.Count > 0)
                {
                    viewModel.AddSelectedItems(instances);
                }
            }
            if (viewModel.TheAction == GridRefAction.FindGrids)
            {
                viewModel.FindNearestGrids(app.ActiveUIDocument.Document);
            }

            if (viewModel.TheAction == GridRefAction.AddParameter)
            {
                viewModel.AddGridsToParameter(app.ActiveUIDocument.Document);
            }
        }

        public string GetName()
        {
            return "GridReferenceEvent";
        }
    }
}
