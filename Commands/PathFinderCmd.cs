using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTTRevitTools.Pathfinder;
using TTTRevitTools.RvtExtEvents;

namespace TTTRevitTools.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PathFinderCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                PipingSystemPath pipingSystemPath = new PipingSystemPath(commandData.Application.ActiveUIDocument);
                SelectPathEventhandler selectPathEventhandler = new SelectPathEventhandler();
                ExternalEvent theEvent = ExternalEvent.Create(selectPathEventhandler);
                pipingSystemPath.TheEvent = theEvent;
                pipingSystemPath.GetSystemElementsIds();
                pipingSystemPath.GetElementConnections();
                pipingSystemPath.FindAllPaths();
                pipingSystemPath.DisplayWindow();
                App.Instance.PathfinderViewModel = pipingSystemPath.PathfinderViewModel;

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
