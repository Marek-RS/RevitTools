using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TTTRevitTools.ITwoExport;

namespace TTTRevitTools.RvtExtEvents
{
    public class iTwoExportExEvent : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            try
            {
                ExportViewModel exportViewModel = App.Instance.ExportViewModel;

                if (exportViewModel.Action == ITwoAction.GetElements)
                {
                    exportViewModel.GetExportedElements(app.ActiveUIDocument.Document);
                }
                if (exportViewModel.Action == ITwoAction.GetMaterials)
                {
                    exportViewModel.GetExportedMaterials(app.ActiveUIDocument.Document);
                }
                if (exportViewModel.Action == ITwoAction.SelectInModel)
                {
                    exportViewModel.SelectInModel(app.ActiveUIDocument);
                }
                if (exportViewModel.Action == ITwoAction.SelectMaterial)
                {
                    exportViewModel.SelectMaterial(app.ActiveUIDocument);
                }
                if (exportViewModel.Action == ITwoAction.SelectElementContaining)
                {
                    exportViewModel.SelectElementContaining(app.ActiveUIDocument);
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }
        }

        public string GetName()
        {
            return "iTwoExportEvent";
        }
    }
}
