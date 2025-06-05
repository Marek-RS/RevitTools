using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DocumentFormat.OpenXml.InkML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTTRevitTools.ConvoidOpenings;

namespace TTTRevitTools.RvtExtEvents
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class ConvoidOpeningsEvent : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            try
            {
                ConvoidViewModel convoidViewModel = App.Instance.ConvoidViewModel;
                List<string> verticalOpeningNames = null;
                List<FamilyInstance> openingInstances = null;
                string[] _familyNames = null;
                switch (convoidViewModel.Action)
                {
                    case ConvoidOpeningsAction.AddDevParametersSingle:
                        convoidViewModel.CheckSingleFamily(app.Application, app.ActiveUIDocument.Document);
                        break;
                    case ConvoidOpeningsAction.AddDevParameters:
                        convoidViewModel.CheckUpdaterFamilies(app.Application, app.ActiveUIDocument.Document);
                        break;

                    case ConvoidOpeningsAction.DeactivateUpdater:
                        convoidViewModel.OpeningUpdater.DeactivateUpdater("Manually deactivated...");
                        convoidViewModel.OpeningUpdater = null;
                        convoidViewModel.ViewModelReinitialize();
                        break;

                    case ConvoidOpeningsAction.ActivateUpdater:
                        if (convoidViewModel.OpeningUpdater != null)
                        {
                            TaskDialog.Show("Warning", "Can't activate. Opening updater has been activated for another document!");
                        }
                        else
                        {
                            convoidViewModel.OpeningUpdater = OpeningUpdater.Initialize(app.ActiveUIDocument.Document, convoidViewModel);
                            convoidViewModel.OpeningUpdater.ActivateUpdater("Opening updater manual activation...");
                            convoidViewModel.GetLinkedModelsData(app.ActiveUIDocument.Document);
                            convoidViewModel.GetGridsData(app.ActiveUIDocument.Document);
                            convoidViewModel.SearchOpeningFamilies(app.ActiveUIDocument.Document);
                            convoidViewModel.UpdaterSummaryInfo = "Activated for: " + app.ActiveUIDocument.Document.Title;

                            List<string> info = new List<string>();
                            List<Definition> definitions = convoidViewModel.GetSharedParameters(app.Application);
                            foreach (UpdaterFamily uf in convoidViewModel.UpdaterFamilies.Where(x => x.IsChecked))
                            {
                                uf.CheckDefinitions(definitions, app.ActiveUIDocument.Document);
                                if (uf.HasMissingDefinitions()) info.Add(uf.Name);
                            }
                            if (info.Count > 0)
                            {
                                string warning = "Following families are missing parameters required by Opening Updater!" + Environment.NewLine + Environment.NewLine + String.Join(Environment.NewLine, info) + Environment.NewLine + Environment.NewLine + "Would you like to load parameters now?";
                                var result = TaskDialog.Show("Warning", warning, TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);
                                if (result == TaskDialogResult.Yes)
                                {
                                    foreach (UpdaterFamily uf in convoidViewModel.UpdaterFamilies.Where(e => e.IsChecked))
                                    {
                                        if (uf.HasMissingDefinitions()) uf.AddDefinitions(app.ActiveUIDocument.Document);
                                    }
                                }
                            }
                            app.ActiveUIDocument.Document.DocumentClosing += convoidViewModel.Document_DocumentClosing;
                        }
                        break;

                    case ConvoidOpeningsAction.GetSelectedOpenings:
                        var selectedIds = app.ActiveUIDocument.Selection.GetElementIds();
                        convoidViewModel.SelectedOpeningElements.Clear();
                        foreach (var id in selectedIds)
                        {
                            Element el = app.ActiveUIDocument.Document.GetElement(id);
                            if (el.Category != null && el.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel)
                            {
                                FamilyInstance fi = el as FamilyInstance;
                                if (fi != null)
                                {
                                    if (fi.Symbol.Family.Name.Contains("Vertical") && convoidViewModel.UpdaterFamilies.Where(e => e.IsChecked).Select(e => e.Name).Contains(fi.Symbol.Family.Name))
                                    {
                                        convoidViewModel.SelectedOpeningElements.Add(fi);
                                    }
                                }
                            }
                        }
                        convoidViewModel.AddExportedOpeningData();
                        break;

                    case ConvoidOpeningsAction.GetAllOpenings:
                        verticalOpeningNames = convoidViewModel.UpdaterFamilies.Where(x => x.IsChecked).Select(x => x.Name).Where(x => x.ToUpper().Contains("VERTICAL")).ToList();
                        openingInstances = new FilteredElementCollector(app.ActiveUIDocument.Document)
                                                                    .OfClass(typeof(FamilyInstance))
                                                                    .Where(e => e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel)
                                                                    .Select(e => e as FamilyInstance).Where(e => verticalOpeningNames.Contains(e.Symbol.Family.Name)).ToList();

                        foreach (FamilyInstance fi in openingInstances) convoidViewModel.SelectedOpeningElements.Add(fi);
                        convoidViewModel.AddExportedOpeningData();
                        break;

                    case ConvoidOpeningsAction.GetViewOpenings:
                        verticalOpeningNames = convoidViewModel.UpdaterFamilies.Where(x => x.IsChecked).Select(x => x.Name).Where(x => x.ToUpper().Contains("VERTICAL")).ToList();
                        openingInstances = new FilteredElementCollector(app.ActiveUIDocument.Document, app.ActiveUIDocument.Document.ActiveView.Id)
                                                .OfClass(typeof(FamilyInstance))
                                                .Where(e => e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel)
                                                .Select(e => e as FamilyInstance).Where(e => verticalOpeningNames.Contains(e.Symbol.Family.Name)).ToList();

                        foreach (FamilyInstance fi in openingInstances) convoidViewModel.SelectedOpeningElements.Add(fi);
                        convoidViewModel.AddExportedOpeningData();
                        break;

                    case ConvoidOpeningsAction.UpdateAllOpenings:
                        _familyNames = convoidViewModel.UpdaterFamilies.Where(e => e.IsChecked).Select(e => e.Name).ToArray();
                        Document doc = app.ActiveUIDocument.Document;
                        openingInstances = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance))
                                                                            .Select(e => e as FamilyInstance)
                                                                            .Where(e => _familyNames.Contains(e.Symbol.Family.Name)).ToList();
                        using (Transaction tx = new Transaction(doc, "Update all openings"))
                        {
                            tx.Start();
                            convoidViewModel.UpdateOpeningsData(openingInstances);
                            tx.Commit();
                        }
                        break;

                    case ConvoidOpeningsAction.UpdateViewOpenings:
                        _familyNames = convoidViewModel.UpdaterFamilies.Where(e => e.IsChecked).Select(e => e.Name).ToArray();
                        doc = app.ActiveUIDocument.Document;
                        openingInstances = new FilteredElementCollector(doc, doc.ActiveView.Id).OfClass(typeof(FamilyInstance))
                                                                                .Select(e => e as FamilyInstance)
                                                                                .Where(e => _familyNames.Contains(e.Symbol.Family.Name)).ToList();
                        using (Transaction tx = new Transaction(doc, "Update view openings"))
                        {
                            tx.Start();
                            convoidViewModel.UpdateOpeningsData(openingInstances);
                            tx.Commit();
                        }
                        break;

                    case ConvoidOpeningsAction.UpdateSelectedOpenings:
                        _familyNames = convoidViewModel.UpdaterFamilies.Where(e => e.IsChecked).Select(e => e.Name).ToArray();
                        doc = app.ActiveUIDocument.Document;
                        var ids = app.ActiveUIDocument.Selection.GetElementIds();
                        if (ids == null || ids.Count == 0) return;
                        openingInstances = new List<FamilyInstance>();

                        foreach (var id in ids)
                        {
                            FamilyInstance fi = doc.GetElement(id) as FamilyInstance;
                            if (fi != null && _familyNames.Contains(fi.Symbol.Family.Name))
                                openingInstances.Add(fi);
                        }

                        if (openingInstances.Count == 0) return;

                        using (Transaction tx = new Transaction(doc, "Update selected openings"))
                        {
                            tx.Start();
                            convoidViewModel.UpdateOpeningsData(openingInstances);
                            tx.Commit();
                        }
                        break;

                    case ConvoidOpeningsAction.SelectInRevit:
                        app.ActiveUIDocument.Selection.SetElementIds(new List<ElementId> { convoidViewModel.SelectedId });
                        app.ActiveUIDocument.ShowElements(new List<ElementId> { convoidViewModel.SelectedId });
                        break;
                    case ConvoidOpeningsAction.SetViewTagRefs:
                        convoidViewModel.SetViewTagRefs(app.ActiveUIDocument.Document);
                        break;
                    case ConvoidOpeningsAction.ExportAll:
                        _familyNames = convoidViewModel.UpdaterFamilies.Where(e => e.IsChecked).Select(e => e.Name).ToArray();
                        openingInstances = new FilteredElementCollector(app.ActiveUIDocument.Document).OfClass(typeof(FamilyInstance))
                                                                            .Select(e => e as FamilyInstance)
                                                                            .Where(e => _familyNames.Contains(e.Symbol.Family.Name)).ToList();
                        convoidViewModel.ExportOpenings(openingInstances);          
                        break;
                    case ConvoidOpeningsAction.ExportView:
                        _familyNames = convoidViewModel.UpdaterFamilies.Where(e => e.IsChecked).Select(e => e.Name).ToArray();
                        openingInstances = new FilteredElementCollector(app.ActiveUIDocument.Document, app.ActiveUIDocument.Document.ActiveView.Id).OfClass(typeof(FamilyInstance))
                                                                            .Select(e => e as FamilyInstance)
                                                                            .Where(e => _familyNames.Contains(e.Symbol.Family.Name)).ToList();
                        convoidViewModel.ExportOpenings(openingInstances);
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
            return "ConvoidOpeningsEvent";
        }
    }
}
