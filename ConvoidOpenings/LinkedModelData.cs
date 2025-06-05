using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using DocumentFormat.OpenXml.Vml.Office;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.ConvoidOpenings
{
    //easily serializable object
    public class LinkedModelData
    {
        //name to track from list
        public string Name { get; set; }
        public string Discipline { get; set; } = "---";
        public DateTime? DateCreated { get; set; }
        public RevitLinkType LinkType { get; set; }
        public LinkModelStatus Status { get; set; }

        //dictionary to quickly query property by name
        public List<LinkedModelElement> ModelElements { get; set; }

        public LinkedModelData() { }

        public bool IsHostModel()
        {
            bool result = false;
            switch (LinkType)
            {
                case RevitLinkType.Architecture:
                case RevitLinkType.Structure:
                    result = true;
                    break;
                default:
                    break;
            }
            return result;
        }

        public bool IsReferenceModel()
        {
            bool result = false;
            switch (LinkType)
            {
                case RevitLinkType.Mechanical:
                case RevitLinkType.Plumbing:
                case RevitLinkType.Electrical:
                case RevitLinkType.ProcessPiping:
                case RevitLinkType.FireProtection:
                case RevitLinkType.RoofDrainage:
                case RevitLinkType.Controls:
                    result = true;
                    break;
                default:
                    break;
            }
            return result;
        }

        private void LoopElements(List<Element> elements, List<string> parameterNames)
        {
            foreach (Element el in elements)
            {
                LinkedModelElement linkedModelElement = new LinkedModelElement();
                linkedModelElement.InitializeModelElement(el);
                //linkedModelElement.GetElementGeometry();
                linkedModelElement.GetProperties(parameterNames);
                ModelElements.Add(linkedModelElement);
            }
        }

        public void GetModelElements(Document linkedDocument)
        {
            List<string> parameterNames = RevitLinkManager.GetParameterNames(LinkType);
            ModelElements = new List<LinkedModelElement>();
            switch (LinkType)
            {
                case RevitLinkType.None:
                    break;
                case RevitLinkType.Architecture:
                case RevitLinkType.Structure:
                    List<Element> walls = new FilteredElementCollector(linkedDocument).OfClass(typeof(Wall)).ToList();
                    List<Element> floors = new FilteredElementCollector(linkedDocument).OfClass(typeof(Floor)).ToList();
                    List<Element> roofs = new FilteredElementCollector(linkedDocument).OfCategory(BuiltInCategory.OST_Roofs).WhereElementIsNotElementType().ToList();

                    LoopElements(walls, parameterNames);
                    LoopElements(floors, parameterNames);
                    LoopElements(roofs, parameterNames);

                    break;

                case RevitLinkType.Mechanical:
                case RevitLinkType.Plumbing:
                case RevitLinkType.ProcessPiping:
                case RevitLinkType.FireProtection:

                    List<Element> m_pipes = new FilteredElementCollector(linkedDocument).OfClass(typeof(Pipe)).ToList();
                    List<Element> m_ducts = new FilteredElementCollector(linkedDocument).OfClass(typeof(Duct)).ToList();

                    List<Element> m_flexPipe = new FilteredElementCollector(linkedDocument).OfClass(typeof(FlexPipe)).ToList();
                    List<Element> m_flexDuct = new FilteredElementCollector(linkedDocument).OfClass(typeof(FlexDuct)).ToList();

                    List<Element> m_familyInstances = new FilteredElementCollector(linkedDocument).OfClass(typeof(FamilyInstance)).ToList();

                    List<Element> m_pipeFittings = m_familyInstances.Where(e => e.Category != null && e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting).ToList();
                    List<Element> m_pipeAccessories = m_familyInstances.Where(e => e.Category != null && e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory).ToList();
                    List<Element> m_pipeInsulations = new FilteredElementCollector(linkedDocument).OfClass(typeof(PipeInsulation)).ToList();

                    //04.04.2024 - adding plumbing fixtures
                    List<Element> m_plumbingFixtures = m_familyInstances.Where(e => e.Category != null && e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PlumbingFixtures).ToList();


                    List<Element> m_ductFittings = m_familyInstances.Where(e => e.Category != null && e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctFitting).ToList();
                    List<Element> m_ductAccessories = m_familyInstances.Where(e => e.Category != null && e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctAccessory).ToList();
                    List<Element> m_ductInsulations = new FilteredElementCollector(linkedDocument).OfClass(typeof(DuctInsulation)).ToList();

                    LoopElements(m_pipes, parameterNames);
                    LoopElements(m_ducts, parameterNames);

                    LoopElements(m_flexPipe, parameterNames);
                    LoopElements(m_flexDuct, parameterNames);

                    LoopElements(m_pipeFittings, parameterNames);
                    LoopElements(m_pipeAccessories, parameterNames);
                    LoopElements(m_pipeInsulations, parameterNames);
                    LoopElements(m_plumbingFixtures, parameterNames);

                    LoopElements(m_ductFittings, parameterNames);
                    LoopElements(m_ductAccessories, parameterNames);
                    LoopElements(m_ductInsulations, parameterNames);

                    break;
                case RevitLinkType.Electrical:
                    List<Element> e_cableTrays = new FilteredElementCollector(linkedDocument).OfClass(typeof(CableTray)).ToList();
                    List<Element> e_conduits = new FilteredElementCollector(linkedDocument).OfClass(typeof(Conduit)).ToList();

                    List<Element> e_familyInstances = new FilteredElementCollector(linkedDocument).OfClass(typeof(FamilyInstance)).ToList();
                    List<Element> e_cableTrayFittings = e_familyInstances.Where(e => e.Category != null && e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_CableTrayFitting).ToList();
                    List<Element> e_conduitFittings = e_familyInstances.Where(e => e.Category != null && e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ConduitFitting).ToList();
                    List<Element> e_elecEquipment = e_familyInstances.Where(e => e.Category != null && e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ElectricalEquipment).ToList();

                    List<Element> e_communicationDevices = e_familyInstances.Where(e => e.Category != null && e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_CommunicationDevices).ToList();
                    List<Element> e_curtainPanels = e_familyInstances.Where(e => e.Category != null && e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_CurtainWallPanels).ToList();
                    List<Element> e_dataDevices = e_familyInstances.Where(e => e.Category != null && e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DataDevices).ToList();
                    List<Element> e_elecFixtures = e_familyInstances.Where(e => e.Category != null && e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ElectricalFixtures).ToList();

                    List<Element> e_lightDevices = e_familyInstances.Where(e => e.Category != null && e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_LightingDevices).ToList();
                    List<Element> e_lightFixtures = e_familyInstances.Where(e => e.Category != null && e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_LightingFixtures).ToList();

                    LoopElements(e_cableTrays, parameterNames);
                    LoopElements(e_conduits, parameterNames);
                    LoopElements(e_cableTrayFittings, parameterNames);
                    LoopElements(e_conduitFittings, parameterNames);
                    LoopElements(e_elecEquipment, parameterNames);

                    LoopElements(e_communicationDevices, parameterNames);
                    LoopElements(e_curtainPanels, parameterNames);
                    LoopElements(e_dataDevices, parameterNames);
                    LoopElements(e_elecFixtures, parameterNames);
                    LoopElements(e_lightDevices, parameterNames);
                    LoopElements(e_lightFixtures, parameterNames);

                    break;
                default:
                    break;
            }
        }
    }
}
