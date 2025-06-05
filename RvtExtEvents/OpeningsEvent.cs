using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTTRevitTools.Openings;

namespace TTTRevitTools.RvtExtEvents
{
    public class OpeningsEvent : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            try
            {
                OpeningsViewModel viewModel = App.Instance.OpeningsViewModel;
                switch (viewModel.OpeningsAction)
                {
                    case OpeningsAction.None:
                        break;
                    case OpeningsAction.ReInitialize:
                        break;
                    case OpeningsAction.FindOpenings:
                        App.Instance.OpeningsViewModel.FindOpenings(app.ActiveUIDocument);
                        break;
                    case OpeningsAction.SelectOpenings:
                        bool result = App.Instance.OpeningsViewModel.SelectOpeningInModel(app.ActiveUIDocument);
                        if(!result)
                        {
                            if (app.ActiveUIDocument.ActiveView.ViewType != Autodesk.Revit.DB.ViewType.ThreeD)
                            {
                                TaskDialog.Show("Info", "Please open 3d view and try again to see intersection location");
                            }
                            else
                            {
                                var dialogResult = TaskDialog.Show("Info", "Would you like to outline intersection location?", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);
                                if(dialogResult == TaskDialogResult.Yes) viewModel.ShowLocationOn3d(app.ActiveUIDocument);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }
        }

        public string GetName()
        {
            return "OpeningsEvent";
        }
    }
}
