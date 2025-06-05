using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.RvtExtEvents
{
    public class GigaBaseEvent : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            if(App.Instance.GigabaseViewModel.GigabaseAction == Gigabase.GigabaseAction.Select)
            {
                App.Instance.GigabaseViewModel.SelectByMarkInCustomParameters(app.ActiveUIDocument);
            }
            if (App.Instance.GigabaseViewModel.GigabaseAction == Gigabase.GigabaseAction.Collect)
            {
                App.Instance.GigabaseViewModel.CollectModelItems(app.ActiveUIDocument.Document);
            }
        }

        public string GetName()
        {
            return "GigaBaseBrowser";
        }
    }
}
