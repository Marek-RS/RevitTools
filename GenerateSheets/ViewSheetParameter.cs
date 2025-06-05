using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.GenerateSheets
{
    public class ViewSheetParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public StorageType StorageType { get; set; }
        public Parameter Parameter { get; set; }
        public bool IsShared { get; set; }
    }
}
