using Autodesk.Revit.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TTTRevitTools.FindAffectedSheets;

namespace TTTRevitTools.ConvoidOpenings
{
    public class RevitLinkManager
    {
        public List<LinkedModelData> Loaded { get; set; }
        public List<LinkedModelData> NotLoaded { get; set; }
        public List<LinkedModelData> Local { get; set; }
        public List<string> Online { get; set; }

        public RevitLinkManager() 
        {
            Local = new List<LinkedModelData>();
            Loaded = new List<LinkedModelData>();
            NotLoaded = new List<LinkedModelData>();
        }

        public void GetOnlineModelNames()
        {
            //call online resource
        }

        public void ReadLinkDataFiles(string linkFolder)
        {
            string[] paths = Directory.GetFiles(linkFolder);
            foreach (string path in paths)
            {
                if (!path.Contains(".json")) continue;
                try
                {
                    string jsonString = File.ReadAllText(path);
                    LinkedModelData linkModel = JsonConvert.DeserializeObject<LinkedModelData>(jsonString);
                    linkModel.Status = LinkModelStatus.LocalUpToDate;
                    Local.Add(linkModel);
                }
                catch
                {

                }
            }     
        }

        public void SendLinkData(LinkedModelData linkModel)
        {
            try
            {
                string linkJsonString = JsonConvert.SerializeObject(linkModel);
                string url = @"power automate flow http request endpoint";
                Dictionary<string, object> data = new Dictionary<string, object>();
                data.Add("Username", "Yourname");
                data.Add("Content", linkJsonString);
                data.Add("AccessKey", 123456);
                data.Add("LinkName", linkModel.Name);

                string jsonString = JsonConvert.SerializeObject(data);
                StringContent httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

                HttpClient client = new HttpClient();
                client.PostAsync(url, httpContent);
                //string responseBody = response.Content.ReadAsStringAsync().Result;

            }
            catch (Exception)
            {
                
            }

        }

        public void AddLinkData(RevitLinkInstance revitLinkInstance)
        {
            LinkedModelData linkedModelData = new LinkedModelData();
            Document doc = revitLinkInstance.GetLinkDocument();
            if(doc == null)
            {
                linkedModelData.Name = GetNotLoadedName(revitLinkInstance.Name);
                linkedModelData.Status = LinkModelStatus.NotLoaded;
                linkedModelData.LinkType = GetLinkType(revitLinkInstance.Name.Split('-'));
                linkedModelData.Discipline = GetDiscipline(linkedModelData.LinkType);
                linkedModelData.DateCreated = null;
                if (!Local.Select(e => e.Name).Contains(linkedModelData.Name)) NotLoaded.Add(linkedModelData);
            }
            else
            {
                linkedModelData.Name = doc.Title;
                linkedModelData.Status = LinkModelStatus.LoadedInRevit;
                linkedModelData.LinkType = GetLinkType(doc.Title.Split('-'));
                linkedModelData.Discipline = GetDiscipline(linkedModelData.LinkType);
                linkedModelData.DateCreated = DateTime.Now;
                //this will have priority over Local if contains
                linkedModelData.GetModelElements(doc);
                Loaded.Add(linkedModelData);
            }
        }

        private string GetNotLoadedName(string name)
        {
            string result = name;
            string[] nameArray = name.Split('-');
            if (nameArray.Length == 9)
            {
                result = String.Join("-", nameArray, 0, 8) + "-" + nameArray[8].Split('.')[0];
            }
            return result;
        }

        private string GetDiscipline(RevitLinkType revitLinkType)
        {
            string result = "---";
            switch (revitLinkType)
            {
                case RevitLinkType.None:
                    break;
                case RevitLinkType.Architecture:
                    result = "ARCH";
                    break;
                case RevitLinkType.Structure:
                    result = "STRC";
                    break;
                case RevitLinkType.Mechanical:
                    result = "MECH";
                    break;
                case RevitLinkType.Plumbing:
                    result = "PLMB";
                    break;
                case RevitLinkType.Electrical:
                    result = "ELEC";
                    break;
                case RevitLinkType.ProcessPiping:
                    result = "PRPP";
                    break;
                case RevitLinkType.FireProtection:
                    result = "SPRL";
                    break;
                case RevitLinkType.RoofDrainage:
                    result = "RD";
                    break;
                case RevitLinkType.Controls:
                    result = "IC";
                    break;
                default:
                    break;
            }
            return result;
        }
       
        private RevitLinkType GetLinkType(string[] nameArray)
        {
            if (nameArray.Length != 9) return RevitLinkType.None;
            switch (nameArray[6])
            {
                case "A":
                    return RevitLinkType.Architecture;
                case "M":
                    return RevitLinkType.Mechanical;
                case "P":
                    return RevitLinkType.Plumbing;
                case "PP":
                    return RevitLinkType.ProcessPiping;
                case "S":
                    return RevitLinkType.Structure;
                case "E":
                    return RevitLinkType.Electrical;
                case "FP":
                    return RevitLinkType.FireProtection;
                case "IC":
                    return RevitLinkType.Controls;
                default:
                    return RevitLinkType.None;
            }
        }

        public static List<string> GetParameterNames(RevitLinkType linkType)
        {
            List<string> parameters = new List<string>(){"Category","Type","Workset"};

            switch (linkType)
            {
                case RevitLinkType.Architecture:
                case RevitLinkType.Structure:
                    parameters.Add("Fire Rating");
                    break;
                case RevitLinkType.Mechanical:
                case RevitLinkType.Plumbing:
                case RevitLinkType.Electrical:
                case RevitLinkType.ProcessPiping:
                case RevitLinkType.FireProtection:
                case RevitLinkType.RoofDrainage:
                    parameters.Add("System Name");
                    parameters.Add("System Type");
                    parameters.Add("System Classification");
                    parameters.Add("System Abbreviation");
                    parameters.Add("Size");
                    break;
                case RevitLinkType.Controls:
                    break;
                default:
                    break;
            }
            return parameters;
        }
    }
}
