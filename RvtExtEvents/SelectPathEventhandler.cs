using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.RvtExtEvents
{
    public class SelectPathEventhandler : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            App.Instance.PathfinderViewModel.SelectItems(app.ActiveUIDocument);
        }

        public string GetName()
        {
            return "SelectPathEvent";
        }
    }
}
