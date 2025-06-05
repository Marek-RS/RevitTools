using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using CefSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using TTTRevitTools.VisTableImport;

namespace TTTRevitTools.RvtExtEvents
{
    internal class VisTableImportExEventHandler : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            //if(app.ActiveUIDocument.Document.IsFamilyDocument)
            //{
            //    TaskDialog.Show("Warning!", "Document must be of project type");
            //    return;
            //}
            app.DialogBoxShowing += OnDialogBoxShowing;
            VisTableViewModel viewModel = App.Instance.VisTableViewModel;
            try
            {
                if(viewModel.VisTableAction == VisTableAction.OpenModel)
                {
                    string fixedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "TestDirectory/visTableTestProject.rvt");
                    if (viewModel.ModelName == "SiteLayout")
                    {
                        fixedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "TestDirectory/SiteLayout.rvt");
                    }
                    app.OpenAndActivateDocument(fixedPath);
                }
                if (viewModel.VisTableAction == VisTableAction.ImportFromApp)
                {
                    List<VisElement> visElements = JsonConvert.DeserializeObject<List<VisElement>>(viewModel.JsonImportData);
                    foreach (VisElement visElement in visElements) visElement.IsSelected = true;
                    viewModel.Layout.VisElements = visElements;

                    int portion = 5;
                    int selectedItems = viewModel.Layout.VisElements.Where(e => e.IsSelected).Count();
                    int startIndex = 0;
                    int endIndex = startIndex + portion;
                    bool maxReached = false;
                    int i = 0;
                    while (!maxReached)
                    {
                        List<ElementId> ids = viewModel.Layout.VisElements.Select(e => e.RevitId).Where(e => e != null).Select(e => new ElementId(int.Parse(e))).ToList();
                        if (ids != null && ids.Count > 0)
                        {
                            app.ActiveUIDocument.ShowElements(ids);
                        }
                        if (endIndex > selectedItems)
                        {
                            endIndex = selectedItems;
                            maxReached = true;
                        }
                        viewModel.CreateNewFamiliesAndTypes(app, startIndex, endIndex);
                        viewModel.InsertElementsToRevit(app.ActiveUIDocument.Document, startIndex, endIndex);
                        startIndex += portion;
                        endIndex += portion;
                        i++;
                    }
                    viewModel.RemoveFamilyFiles();
                    viewModel.Browser.ExecuteScriptAsync("setInRevitStatus();");
                }
                if (viewModel.VisTableAction == VisTableAction.LoadXmlIntoRvt)
                {
                    View view = new FilteredElementCollector(app.ActiveUIDocument.Document).OfClass(typeof(View)).Select(e => e as View).Where(e => e.ViewType == ViewType.ThreeD).FirstOrDefault();
                    int portion = 5;
                    int selectedItems = viewModel.Layout.VisElements.Where(e => e.IsSelected).Count();
                    int startIndex = 0;
                    int endIndex = startIndex + portion;
                    bool maxReached = false;
                    int i = 0;
                    while (!maxReached)
                    {
                        viewModel.ProgressText = string.Format("Processed: {0}/{1}", startIndex, selectedItems);
                        List<ElementId> ids = viewModel.Layout.VisElements.Select(e => e.RevitId).Where(e => e != null).Select(e => new ElementId(int.Parse(e))).ToList();
                        if(ids != null && ids.Count > 0)
                        {
                            app.ActiveUIDocument.ShowElements(ids);
                        }
                        DoEvents();
                        if (endIndex > selectedItems)
                        {
                            endIndex = selectedItems;
                            maxReached = true;
                        }
                        viewModel.CreateNewFamiliesAndTypes(app, startIndex, endIndex);
                        viewModel.InsertElementsToRevit(app.ActiveUIDocument.Document, startIndex, endIndex);
                        startIndex += portion;
                        endIndex += portion;
                        i++;
                    }
                    viewModel.ProgressText = string.Format("Processed: {0}/{1}", selectedItems, selectedItems);
                    viewModel.RemoveFamilyFiles();
                    foreach (var item in viewModel.Layout.VisElements) item.IsSelected = false;
                    viewModel.VisTableWindow.MainDataGrid.Items.Refresh();
                }
                if(viewModel.VisTableAction == VisTableAction.LoadFromRvt)
                {

                }
                if(viewModel.VisTableAction == VisTableAction.SelectElement)
                {
                    app.ActiveUIDocument.Selection.SetElementIds(new List<ElementId>() { viewModel.SelectedRevitId });
                    //Document doc = app.ActiveUIDocument.Document;
                    //Transaction tx = new Transaction(doc, "modify");
                    //tx.Start();

                    //tx.Commit();
                }
                if(viewModel.VisTableAction == VisTableAction.ShapeConvexPolygon)
                {
                    viewModel.TryConvexPolygonProfile(app);
                }
                app.DialogBoxShowing -= OnDialogBoxShowing;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
                app.DialogBoxShowing -= OnDialogBoxShowing;
                var result = TaskDialog.Show("Info", "Would you like to close opened family documents?", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);
                if (result == TaskDialogResult.Yes)
                {
                    foreach (Document document in app.Application.Documents)
                    {
                        if (document.IsFamilyDocument) document.Close(false);
                    }
                }
            }
        }

        public string GetName()
        {
            return "VisTableImportExEventHandler";
        }

        private void OnDialogBoxShowing(object sender, DialogBoxShowingEventArgs e)
        {
            if (e.DialogId == "Dialog_Revit_DocWarnDialog")
            {
                e.OverrideResult((int)TaskDialogResult.Ok);
            }
        }

        public void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
            new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        public object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }
    }
}
