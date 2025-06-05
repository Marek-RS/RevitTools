using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.ConvoidOpenings
{
    public class LinkedModelElement
    {
        public int Id { get; set; }
        public Dictionary<string, string> Properties { get; set; } //<name, value>
        public List<GeometryFace> Faces { get; set; }

        private Element _element;

        //initialize after creating new object, constructor remains public to work with serialization
        public void InitializeModelElement(Element element)
        {
            _element = element;
            Properties = new Dictionary<string, string>();
            Id = element.Id.IntegerValue;
        }

        public void GetElementGeometry()
        {
            Faces = new List<GeometryFace>();
            Options opt = new Options();
            opt.IncludeNonVisibleObjects = false;
            GeometryElement geoElem = _element.get_Geometry(opt);

            foreach (GeometryObject geoObj in geoElem)
            {
                Solid solid = geoObj as Solid;
                if (solid != null)
                {
                    if (solid.Volume > 0)
                    {
                        foreach (Face face in solid.Faces)
                        {
                            GeometryFace geometryFace = new GeometryFace();
                            geometryFace.Vertices = face.Triangulate(0)?.Vertices.ToList();
                            geometryFace.Normals = face.Triangulate(0)?.GetNormals().ToList();
                            Faces.Add(geometryFace);                            
                        }
                    }
                }

            }
        }

        public void GetProperties(List<string> propertyNames)
        {
            foreach (string pName in propertyNames)
            {
                Parameter p = _element.LookupParameter(pName);
                if(pName == "Fire Rating")
                { 
                    //todo: Different initialization for different types, in case of Host elements get access to type element
                }
                if (p != null)
                {
                    switch (p.StorageType)
                    {
                        case StorageType.None:
                            break;
                        case StorageType.Integer:
                            Properties.Add(pName, p.AsValueString());
                            break;
                        case StorageType.Double:
                            break;
                        case StorageType.String:
                            Properties.Add(pName, p.AsString());
                            break;
                        case StorageType.ElementId:
                            Properties.Add(pName, p.AsValueString());
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
