using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.Pathfinder
{
    public class PathfinderDatagridItem
    {
        public int NoOfElements { get; set; }
        public List<ElementId> ElementIds { get; set; }
    }
}
