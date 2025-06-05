using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.ViewTemplatesManager
{
    public class DocumentModel
    {
        public string Name { get; set; }
        public Document _doc { get; set; }

        public DocumentModel(Document doc)
        {
            _doc = doc;
            Name = doc.Title;
        }
    }
}
