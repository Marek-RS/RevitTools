using Autodesk.Revit.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.Openings
{
    public class OpeningModel
    {
        public int ElementId { get; set; }
        public string FamilyName { get; set; }
        public string SymbolName { get; set; }

        public FamilyInstance FamilyInstance { get; set; }
        public Dictionary<string, string> CustomParameters { get; set; }
        public Dictionary<string , string> BuiltInParameters { get; set; }

        private List<string> customParameters = new List<string>()
        {
            "TTT - Opening Unique Identifier",
            "TTT - Opening Size",
            "TTT - MEP System Abbreviation",
            "TTT - MEP System",

            "TTT - MEP Type",

            "TTT - Grid Ref - Horizontal",
            "TTT - Grid Ref - Vertical",
            "TTT - Grid Ref - Position EW",
            "TTT - Grid Ref - Position NS",
            "TTT - Intersection Location",
            "TTT - Intersection Orientation",
            "TTT - Building Element Category",
            "TTT - Building Element Id",

            "TTT - Building Element Source File",
            "TTT - MEP Element Category",
            "TTT - MEP Element Id",
            "TTT - MEP Element Source File",
            "TTT - MEP Size",

            "TTT - Opening Area (m2)",
            "TTT - MEP Summary Info",
            "TTT - MEP Discipline",

            "TTT - Review Status - All",
            "TTT - Construction Status",
            "TTT - Nearest Grid Intersection"
        };

        private List<string> builtInParameters = new List<string>()
        {
            "Model Name",
            "View Name" ,
            "Revit ElementId" ,
            "Revit TypeId" ,
            "Comments"  ,
            "Description"  ,
            "Family Name" ,
            "Type Name",
            "Workset"
        };

        public OpeningModel(FamilyInstance familyInstance)
        {
            ElementId = familyInstance.Id.IntegerValue;
            FamilyInstance = familyInstance;
            CustomParameters = new Dictionary<string , string>();
            BuiltInParameters = new Dictionary<string , string>();
        }

        public void Initialize(Document doc)
        {
            GetCustomParameters();
            GetBuiltInParameters(doc);
        }

        public ExportedOpening GetExportedData()
        {
            ExportedOpening result = new ExportedOpening();
            result.marker_element_id = ElementId;

            Dictionary<string, string> total = new Dictionary<string, string>();
            foreach (var pair in CustomParameters) if (!total.ContainsKey(pair.Key)) total.Add(pair.Key, pair.Value);
            foreach (var pair in BuiltInParameters) if (!total.ContainsKey(pair.Key)) total.Add(pair.Key, pair.Value);
            string jsonTotal = JsonConvert.SerializeObject(total);
            result.meta_data = jsonTotal;
            result.comments = BuiltInParameters["Model Name"];
            return result;
        }

        public void GetBuiltInParameters(Document doc)
        {
            string pvalue = "not found";

            foreach (string keyName in builtInParameters)
            {
                switch (keyName)
                {
                    case "Model Name":
                        pvalue = doc.Title;
                        break;
                    case "View Name":
                        pvalue = doc.ActiveView.Name;
                        break;
                    case "Revit ElementId":
                        pvalue = FamilyInstance.Id.IntegerValue.ToString();
                        break;
                    case "Revit TypeId":
                        pvalue = FamilyInstance.Symbol.Id.IntegerValue.ToString();
                        break;
                    case "Comments":
                        string comments = FamilyInstance.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString();
                        if (comments != null) pvalue = comments;
                        break;
                    case "Description":
                        pvalue = "???";
                        break;
                    case "Family Name":
                        pvalue = FamilyInstance.Symbol.Family.Name;
                        FamilyName = pvalue;
                        break;
                    case "Type Name":
                        pvalue = FamilyInstance.Symbol.Name;
                        SymbolName = pvalue;
                        break;
                    case "Workset":
                        string workset = FamilyInstance.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM)?.AsValueString();
                        if (workset != null) pvalue = workset;
                        break;
                    default:
                        break;
                }
                BuiltInParameters.Add(keyName, pvalue);
            }
        }

        private void GetCustomParameters()
        {
            foreach (string pname in customParameters)
            {
                string pvalue = "not found";
                Parameter p = FamilyInstance.LookupParameter(pname);
                if(p != null)
                {
                    switch (p.StorageType)
                    { 
                        case StorageType.String:
                            pvalue = p.AsString();
                            break;
                        case StorageType.Integer:
                            pvalue = p.AsInteger().ToString();
                            break;
                        case StorageType.Double:
                            pvalue = p.AsDouble().ToString();
                            break;
                        default:
                            pvalue = "type not supported: " + Enum.GetName(typeof(StorageType), p.StorageType);
                            break;
                    }
                }
                CustomParameters.Add(pname, pvalue);
            }
        }
    }
}
