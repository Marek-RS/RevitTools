using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.Pathfinder
{
    public class PathfinderViewModel
    {
        public List<PathfinderDatagridItem> PathfinderDatagridItems { get; set; }
        public PathfinderDatagridItem SelectedItem { get; set; }
        List<ElementId> _systemElementIds;


        public PathfinderViewModel(List<ElementId> systemElementIds)
        {
            PathfinderDatagridItems = new List<PathfinderDatagridItem>();
            _systemElementIds = systemElementIds;
        }

        public void SelectItems(UIDocument uidoc)
        {
            if(SelectedItem != null)
            {
                uidoc.Selection.SetElementIds(SelectedItem.ElementIds);
            }
        }

        public void AddNewDatagridItem(List<int> indexes)
        {
            List<ElementId> pathElements = new List<ElementId>();
            foreach (int index in indexes)
            {
                pathElements.Add(_systemElementIds[index]);
            }
            PathfinderDatagridItem pathfinderDatagridItem = new PathfinderDatagridItem() { ElementIds = pathElements, NoOfElements = pathElements.Count };
            PathfinderDatagridItems.Add(pathfinderDatagridItem);
        }
    }
}
