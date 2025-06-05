using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTTRevitTools.GenerateSheets;

namespace TTTRevitTools.RvtExtEvents
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateViewsExEventHandler : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            try
            {
                GenerateSheetsViewModel generateSheetsViewModel = App.Instance.GenerateSheetsViewModel;
                if (generateSheetsViewModel.GenerateSheetsAction == GenerateSheetsAction.Initialize)
                {
                    generateSheetsViewModel.SetViewModel(app.ActiveUIDocument.Document);
                    generateSheetsViewModel.GetViewTypes();
                    generateSheetsViewModel.GetNamingRulesList();
                    generateSheetsViewModel.GetViewports();
                    generateSheetsViewModel.GetSettings();
                    generateSheetsViewModel.GetTitleBlockSharedParameters();
                    generateSheetsViewModel.DisplayWindow();
                }
                if (generateSheetsViewModel.GenerateSheetsAction == GenerateSheetsAction.CreateViewsAndSheets)
                {
                    generateSheetsViewModel.CreateSomeViews();
                    generateSheetsViewModel.CreateSomeSheets();
                }
                if (generateSheetsViewModel.GenerateSheetsAction == GenerateSheetsAction.OpenSheetView)
                {
                    generateSheetsViewModel.SelectedRoom.OpenSheetView(app.ActiveUIDocument);
                }
                if (generateSheetsViewModel.GenerateSheetsAction == GenerateSheetsAction.RemoveViews)
                {
                    generateSheetsViewModel.SelectedRoom.DeleteViews(app.ActiveUIDocument);
                }
                if (generateSheetsViewModel.GenerateSheetsAction == GenerateSheetsAction.OpenAllViews)
                {
                    generateSheetsViewModel.SelectedRoom.OpenAllViews(app.ActiveUIDocument);
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }
        }

        public string GetName()
        {
            return "CreateViewsTools";
        }
    }
}
