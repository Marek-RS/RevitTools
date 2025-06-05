using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.ConvoidOpenings
{
    public class ExportCoordinateData
    {
        public string SeqNumber { get; set; }
        public string GridInfo { get; set; }
        public string Size { get; set; }
        public string Discipline { get; set; }
        public int Fixed { get; set; }
        public string Coords { get; set; }

        private ElementId _elementId;

        public ExportCoordinateData(ElementId id) 
        {
            _elementId = id;
        }

        public ElementId GetElementId()
        {
            return _elementId;
        }
    }
}
