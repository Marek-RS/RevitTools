using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.AdvancedElementSelector
{
    public class RevitCategory
    {        
        public string CategoryName { get; set; }
        public List<RevitElementType> RevitElementTypes { get; set; }
        public Category Category { get; set; }
    }
}
