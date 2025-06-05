using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NavisworksGigabaseTTT;
using System;
using TTTRevitTools.RvtExtEvents;

namespace TTTRevitTools.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GigaBaseCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                GigaBaseEvent gigaBaseEvent = new GigaBaseEvent();
                App.Instance.GigabaseViewModel = new GigabaseViewModel();
                App.Instance.GigabaseViewModel.TheEvent = ExternalEvent.Create(gigaBaseEvent);
                App.Instance.GigabaseViewModel.DisplayWindow();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
                return Result.Failed;
            }
        }
    }
}
