using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.ConvoidOpenings
{
    public class GeometryFace
    {
        public List<XYZ> Normals { get; set; }
        public List<XYZ> Vertices { get; set; }
    }
}
