using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

namespace TTTRevitTools.Pathfinder
{
    public class PipingSystemPath
    {       
        public List<ElementId> SystemElementIds { get; set; }
        public Dictionary<int, List<int>> ElementConnections { get; set; }
        public PathfinderViewModel PathfinderViewModel { get; set; }
        public ExternalEvent TheEvent { get; set; }

        UIDocument _uidoc;
        ElementSet _elementSet;
        Pipe _startPipe;
        Pipe _endPipe;

        public PipingSystemPath(UIDocument uidoc)
        {
            _uidoc = uidoc;
        }

        public void SelectElements(List<int> indexes)
        {
            List<ElementId> pathElements = new List<ElementId>();
            foreach (int index in indexes)
            {
                pathElements.Add(SystemElementIds[index]);
            }
            _uidoc.Selection.SetElementIds(pathElements);
        }

        public void FindAllPaths()
        {
            PathfinderViewModel = new PathfinderViewModel(SystemElementIds);
            GraphAnalyzer g = new GraphAnalyzer(ElementConnections.Sum(e => e.Value.Count), this);
            foreach (KeyValuePair<int, List<int>> pair in ElementConnections)
            {
                foreach (int connection in pair.Value)
                {
                    g.AddEdge(pair.Key, connection);
                }
            }

            int start = SystemElementIds.IndexOf(_startPipe.Id);
            int end = SystemElementIds.IndexOf(_endPipe.Id);
            g.GetAllPaths(start, end);
        }

        public void DisplayWindow()
        {
            PathfinderWindow window = new PathfinderWindow(PathfinderViewModel, TheEvent);
            window.Show();
        }

        public void GetSystemElementsIds()
        {
            Reference reference1 =_uidoc.Selection.PickObject(ObjectType.Element, "Select start element:");
            _startPipe = _uidoc.Document.GetElement(reference1) as Pipe;
            if(_startPipe == null)
            {
                TaskDialog.Show("Error", "Element must be of pipe instance");
                return;
            }

            Reference reference2 = _uidoc.Selection.PickObject(ObjectType.Element, "Select end element:");
            _endPipe = _uidoc.Document.GetElement(reference2) as Pipe;
            if (_endPipe == null)
            {
                TaskDialog.Show("Error", "Element must be of pipe instance");
                return;
            }

            SystemElementIds = new List<ElementId>();
            _elementSet = GetElements();
            foreach (Element e in _elementSet)
            {
                SystemElementIds.Add(e.Id);
            }
        }

        public void GetElementConnections()
        {
            ElementConnections = new Dictionary<int, List<int>>();

            foreach (Element el in _elementSet)
            {
                int parentIndex = SystemElementIds.IndexOf(el.Id);
                List<ElementId> connectedElementIds = GetConnectedElements(el);
                List<int> connectedIndexes = SystemElementIds.Where(e => connectedElementIds.Contains(e)).Select(e => SystemElementIds.IndexOf(e)).ToList();
                ElementConnections.Add(parentIndex, connectedIndexes);
            }
        }

        private ElementSet GetElements() //returns all elements from selected system
        {
            PipingSystem ps = _startPipe.MEPSystem as PipingSystem;
            return ps.PipingNetwork;
        }

        private List<ElementId> GetConnectedElements(Element element) //find all elements of defined category connected to parent element
        {
            List<ElementId> result = new List<ElementId>();
            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory)
            {
                FamilyInstance fi = element as FamilyInstance;
                foreach (Connector c1 in fi.MEPModel.ConnectorManager.Connectors)
                {
                    if (c1.IsConnected)
                    {
                        foreach (Connector c2 in c1.AllRefs)
                        {
                            Element owner = c2.Owner;
                            if (owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves)
                            {
                                result.Add(owner.Id);
                            }
                            if (owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting || owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory)
                            {
                                if (owner.Id != element.Id)
                                {
                                    result.Add(owner.Id);
                                }
                            }
                        }
                    }
                }
            }

            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves)
            {
                Pipe p = element as Pipe;
                foreach (Connector c1 in p.ConnectorManager.Connectors)
                {
                    if (c1.IsConnected)
                    {
                        foreach (Connector c2 in c1.AllRefs)
                        {
                            Element owner = c2.Owner;
                            if (owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting || owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory)
                            {
                                result.Add(owner.Id);
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
