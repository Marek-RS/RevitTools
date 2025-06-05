using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.Openings
{
    public class OpeningSymbol
    {
        public string DisplayName { get; set; }
        public string FamilyName { get; set; }
        public string SymbolName { get; set; }

        public bool IsChecked { get; set; }
        public FamilySymbol FamilySymbol { get; set; }

        public OpeningSymbol(FamilySymbol familySymbol)
        {
            FamilySymbol = familySymbol;
            IsChecked = false;
            FamilyName = familySymbol.Family.Name;
            SymbolName = FamilySymbol.Name;
            DisplayName = FamilyName + ": " + SymbolName;
        }
    }
}
