using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.PathFinderPro
{
    public class SystemItem
    {
        public SystemCategory SystemCategory { get; set; }
        public List<Element> Elements { get; set; }
    }
}
