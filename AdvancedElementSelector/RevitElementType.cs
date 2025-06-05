using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.AdvancedElementSelector
{
    public class RevitElementType
    {
        public string ElementTypeName { get; set; }
        public List<Element> Elements { get; set; }

    }
}
