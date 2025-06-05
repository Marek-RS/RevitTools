using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TTTRevitTools.EquipmentTagQuality;

namespace TTTRevitTools.RvtExtEvents
{
    public class TagQualityCheckExEvent : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            //app.DialogBoxShowing += OnDialogBoxShowing;
            try
            {
                TagQualityViewModel viewModel = App.Instance.TagQualityViewModel;
                if (viewModel.TagQualityDataModel.TagQualityAction == TagQualityAction.FindElements)
                {
                    viewModel.TagQualityDataModel.FindElementsByCategory(app.ActiveUIDocument.Document, viewModel.ActiveViewOnly, viewModel.ExcludeSubcomponents);
                }
                if (viewModel.TagQualityDataModel.TagQualityAction == TagQualityAction.SelectElements)
                {
                    viewModel.TagQualityDataModel.SelectAndZoom(app.ActiveUIDocument);
                }
                if (viewModel.TagQualityDataModel.TagQualityAction == TagQualityAction.UpdateAllTags)
                {
                    viewModel.TagQualityDataModel.UpdateAllTags(app.ActiveUIDocument.Document);
                }
                if (viewModel.TagQualityDataModel.TagQualityAction == TagQualityAction.UpdateSelected)
                {
                    viewModel.TagQualityDataModel.UpdateSelectedTags(app.ActiveUIDocument.Document);
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }

            //app.DialogBoxShowing -= OnDialogBoxShowing;

        }

        public string GetName()
        {
            return "TagQualityCheckTool";
        }

        private void OnDialogBoxShowing(object sender, DialogBoxShowingEventArgs e)
        {
            if (e.DialogId == "Dialog_Revit_DocWarnDialog")
            {

            }
        }
    }
}
