using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTTRevitTools.FindAffectedSheets;

namespace TTTRevitTools.RvtExtEvents
{
    public class FindSheetsEventHandler : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            try
            {
                FindSheetsViewModel viewModel = App.Instance.FindSheetsViewModel;
                if (viewModel.Action == FindSheetsAction.FindSheets)
                {
                    viewModel.FindAffectedSheetViews();
                }
                if (viewModel.Action == FindSheetsAction.SelectView)
                {
                    View view = App.Instance.FindSheetsViewModel.SelectedView;
                    if (view != null)
                    {
                        app.ActiveUIDocument.ActiveView = view;
                    }
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }

        }

        public string GetName()
        {
            return "FindSheetEventHandler";
        }
    }
}
