using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.GenerateSheets
{
    public class ViewNamingRule
    {
        public string RuleName { get; set; }
        public ViewType ViewType { get; set; }
        public string Prefix { get; set; }
        public SelectableParameter Parameter1 { get; set; }
        public string Break1 { get; set; }
        public SelectableParameter Parameter2 { get; set; }
        public string Break2 { get; set; }
        public SelectableParameter Parameter3 { get; set; }
        public string Break3 { get; set; }
        public SelectableParameter Parameter4 { get; set; }
        public string Suffix { get; set; }
    }
}
