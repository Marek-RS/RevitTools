using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.Gggbase
{
    public class CheckedModelItem
    {
        public Element RevitElement { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public CheckedModelItem(Element revitElement)
        {
            RevitElement = revitElement;
            Properties = new Dictionary<string, string>();
        }
    }
}
