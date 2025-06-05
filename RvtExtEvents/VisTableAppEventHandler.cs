using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using CefSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TTTRevitTools.VisTableImport;

namespace TTTRevitTools.RvtExtEvents
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class VisTableAppEventHandler : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            //logic with new view model and new window
            VisTableAppModel viewModel = App.Instance.VisTableAppModel;
            try
            {
                if (app.ActiveUIDocument != null) viewModel.GetUiDocReference(app.ActiveUIDocument);
                if(viewModel.VisTableAction == VisTableAction.AddMaterialWorksetItemsAuto)
                {
                    if (app.ActiveUIDocument != null) 
                    {
                        viewModel.GetUiDocReference(app.ActiveUIDocument);
                        viewModel.VisTableAppWindow.AddRowsAutoAwaited();
                    }

                }
                if (viewModel.VisTableAction == VisTableAction.ImportFromApp)
                {
                    app.DialogBoxShowing += OnDialogBoxShowing;
                    if (app.ActiveUIDocument == null)
                    {
                        TaskDialog.Show("Warning!", "Document not found!");
                        viewModel.Browser.ExecuteScriptAsync("setTextInfo('Import cancelled. Document not found!');");
                        return;
                    }
                    if (app.ActiveUIDocument.Document.IsFamilyDocument)
                    {
                        TaskDialog.Show("Warning!", "Document is a family document!");
                        viewModel.Browser.ExecuteScriptAsync("setTextInfo('Import cancelled. Document is a family document!');");
                        return;
                    }
                    Document doc = app.ActiveUIDocument.Document;
                    if (!CheckDocumentBinding(viewModel, doc)) return;
                    if (!app.ActiveUIDocument.Document.IsWorkshared)
                    {
                        TaskDialog.Show("Warning!", "Document is not workshared. Please turn on worksharing!");
                        viewModel.Browser.ExecuteScriptAsync("setTextInfo('Import cancelled. Document is not workshared!');");
                        return;
                    }
                    if (!viewModel.FindLevels()) return;

                    viewModel.DeserializeJsonData();
                    viewModel.SetRevitFamiliesManager();
                    int failedCount = viewModel.CreateNewFamiliesAndTypes(app, false);
                    //viewModel.Browser.ExecuteScriptAsync("setTextInfo('Placing elements in transformed positions...');");
                    viewModel.DoEvents();
                    //viewModel.InsertElementsToRevit(app.ActiveUIDocument.Document);
                    viewModel.Browser.ExecuteScriptAsync("setTextInfo('Setting materials and worksets...');");
                    viewModel.DoEvents();
                    MaterialWorksetManager.AddMaterialsAndWorksets(doc, viewModel);
                    List<VisElement> successElements = viewModel.ImportedVisElements.Where(e => e.Status == "Inserted into revit").ToList();
                    Dictionary<string, string> visRevIds = new Dictionary<string, string>();
                    foreach (VisElement ve in successElements) visRevIds.Add(ve.Id, ve.RevitId);
                    string jsonString = JsonConvert.SerializeObject(visRevIds);
                    viewModel.Browser.ExecuteScriptAsync($"setInRevitStatus({jsonString}, 'imported new layout objects into Revit');");
                    app.DialogBoxShowing -= OnDialogBoxShowing;
                    viewModel.RemoveFamilyFiles();
                    viewModel.Browser.ExecuteScriptAsync(string.Format("finalizedInfo({0});", failedCount));
                }
                if(viewModel.VisTableAction == VisTableAction.SynchronizeChanges)
                {
                    app.DialogBoxShowing += OnDialogBoxShowing;
                    if (app.ActiveUIDocument == null)
                    {
                        TaskDialog.Show("Warning!", "Document not found!");
                        viewModel.Browser.ExecuteScriptAsync("setTextInfo('Import cancelled. Document not found!');");
                        return;
                    }
                    if (app.ActiveUIDocument.Document.IsFamilyDocument)
                    {
                        TaskDialog.Show("Warning!", "Document is a family document!");
                        viewModel.Browser.ExecuteScriptAsync("setTextInfo('Import cancelled. Document is a family document!');");
                        return;
                    }
                    Document doc = app.ActiveUIDocument.Document;
                    if (!CheckDocumentBinding(viewModel, doc)) return;
                    if (!app.ActiveUIDocument.Document.IsWorkshared)
                    {
                        TaskDialog.Show("Warning!", "Document is not workshared. Please turn on worksharing!");
                        viewModel.Browser.ExecuteScriptAsync("setTextInfo('Import cancelled. Document is not workshared!');");
                        return;
                    }
                    if (!viewModel.FindLevels())
                    {
                        viewModel.Browser.ExecuteScriptAsync("setTextInfo('Import cancelled. Not all levels found!');");
                        return;
                    }

                    List<FamilyInstance> familyInstances = new FilteredElementCollector(app.ActiveUIDocument.Document).OfClass(typeof(FamilyInstance)).Where(e => e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel && e.LookupParameter("TTT_VisTable") != null).Select(e => e as FamilyInstance).ToList();
                    viewModel.Browser.ExecuteScriptAsync($"addTextToInfo('- Found {familyInstances.Count} existing layout objects.');");
                    viewModel.DeserializeJsonData();
                    viewModel.Browser.ExecuteScriptAsync($"addTextToInfo('Checking online approved data:');");
                    List<VisElement> newElements = viewModel.ImportedVisElements.Where(e => e.Status.Contains("new") && e.Status.Contains("approved")).ToList();
                    viewModel.Browser.ExecuteScriptAsync($"addTextToInfo('- There are {newElements.Count} new layout objects to import');");

                    List<VisElement> changedElements = viewModel.ImportedVisElements.Where(e => e.Status.Contains("in-revit") && e.Status.Contains("changed") && e.Status.Contains("approved") && !e.Status.Contains("unchanged")).ToList();
                    viewModel.Browser.ExecuteScriptAsync($"addTextToInfo('- There are {changedElements.Count} layout objects to update');");

                    List<VisElement> removedElements = viewModel.ImportedVisElements.Where(e => e.Status.Contains("in-revit") && e.Status.Contains("removed") && e.Status.Contains("approved")).ToList();
                    viewModel.Browser.ExecuteScriptAsync($"addTextToInfo('- There are {removedElements.Count} layout objects to remove');");

                    if(newElements.Count > 0)
                    {
                        viewModel.SetRevitFamiliesManager();
                        viewModel.ImportedVisElements = newElements;
                        int failedCount = viewModel.CreateNewFamiliesAndTypes(app, true);
                        //viewModel.InsertElementsToRevit(app.ActiveUIDocument.Document);
                        int importedElements = newElements.Count - failedCount;
                        viewModel.Browser.ExecuteScriptAsync($"addTextToInfo('Imported: {importedElements} new layout objects.');");
                        if(failedCount > 0) viewModel.Browser.ExecuteScriptAsync($"addTextToInfo('Failed to import: {failedCount} layout objects.');");
                        List<VisElement> successElements = viewModel.ImportedVisElements.Where(e => e.Status == "Inserted into revit").ToList();
                        Dictionary<string, string> visRevIds = new Dictionary<string, string>();
                        foreach (VisElement ve in successElements) visRevIds.Add(ve.Id, ve.RevitId);
                        string jsonString = JsonConvert.SerializeObject(visRevIds);
                        viewModel.Browser.ExecuteScriptAsync($"setInRevitStatus({jsonString}, 'imported new layout objects into Revit');");
                    }
                    if (changedElements.Count > 0)
                    {
                        using (Transaction tx = new Transaction(doc, "Remove layout objects"))
                        {
                            tx.Start();
                            foreach (var item in changedElements)
                            {
                                Element e = doc.GetElement(item.RevitId);
                                Parameter parDateCreated = e.LookupParameter("TTT_DateCreated");
                                Parameter parCreatedBy = e.LookupParameter("TTT_CreatedBy");

                                if (e != null)
                                {
                                    FamilyInstance fi = e as FamilyInstance;
                                    if (fi != null)
                                    {
                                        if (parDateCreated != null && !string.IsNullOrEmpty(parDateCreated.AsString())) item.DateCreated = parDateCreated.AsString();
                                        if (parCreatedBy != null && !string.IsNullOrEmpty(parCreatedBy.AsString())) item.CreatedBy = parCreatedBy.AsString();

                                        ElementId symbolId = fi.Symbol.Id;
                                        ElementId familyId = fi.Symbol.Family.Id;
                                        doc.Delete(fi.Id);
                                        doc.Delete(familyId);
                                    }
                                }
                            }
                            tx.Commit();
                        }
                        viewModel.ImportedVisElements = changedElements;
                        viewModel.SetRevitFamiliesManager();
                        int failedCount = viewModel.CreateNewFamiliesAndTypes(app, true);
                        //viewModel.InsertElementsToRevit(app.ActiveUIDocument.Document);
                        int changedElementsCount = changedElements.Count - failedCount;
                        viewModel.Browser.ExecuteScriptAsync($"addTextToInfo('Updated: {changedElementsCount} layout objects.');");
                        if (failedCount > 0) viewModel.Browser.ExecuteScriptAsync($"addTextToInfo('Failed to change: {failedCount} layout objects.');");
                        List<VisElement> successElements = viewModel.ImportedVisElements.Where(e => e.Status == "Inserted into revit").ToList();
                        Dictionary<string, string> visRevIds = new Dictionary<string, string>();
                        foreach (VisElement ve in successElements) visRevIds.Add(ve.Id, ve.RevitId);
                        string jsonString = JsonConvert.SerializeObject(visRevIds);
                        viewModel.Browser.ExecuteScriptAsync($"setInRevitStatus({jsonString}, 'updated layout objects in Revit');");
                    }
                    if(removedElements.Count > 0)
                    {
                        using (Transaction tx = new Transaction(doc, "Remove layout objects"))
                        {
                            tx.Start();
                            foreach (var item in removedElements)
                            {
                                //Element e = doc.GetElement(new ElementId(item.RevitId));
                                Element e = doc.GetElement(item.RevitId);
                                if (e != null)
                                {
                                    FamilyInstance fi = e as FamilyInstance;
                                    if(fi != null)
                                    {
                                        ElementId symbolId = fi.Symbol.Id;
                                        ElementId familyId = fi.Symbol.Family.Id;
                                        doc.Delete(fi.Id);
                                        doc.Delete(familyId);
                                    }
                                }
                            }
                            tx.Commit();
                        }
                        viewModel.Browser.ExecuteScriptAsync($"addTextToInfo('Removed: {removedElements.Count} layout objects.');");
                        Dictionary<string, string> visRevIds = new Dictionary<string, string>();
                        foreach (VisElement ve in removedElements) visRevIds.Add(ve.Id, ve.RevitId);
                        string jsonString = JsonConvert.SerializeObject(visRevIds);
                        viewModel.Browser.ExecuteScriptAsync($"removeFromApprovedData({jsonString});");
                    }
                    MaterialWorksetManager.AddMaterialsAndWorksets(doc, viewModel);

                    app.DialogBoxShowing -= OnDialogBoxShowing;
                }
                if (viewModel.VisTableAction == VisTableAction.OpenModel)
                {
                    string projectName = viewModel.ProjectName;
                    ProjectModelGuid projectModelGuid = viewModel.ProjectSettingsManager.ProjectGuids.Where(e => e.ProjectName == projectName).FirstOrDefault();
                    if(projectModelGuid != null)
                    {
                        Guid projectGuid = new Guid("004f4643-59b4-4061-bf00-ec8e737bd85b");
                        Guid modelGuid = projectModelGuid.ModelGuid;
                        ModelPath modelPath = ModelPathUtils.ConvertCloudGUIDsToCloudPath(ModelPathUtils.CloudRegionUS, projectGuid, modelGuid);
                        OpenOptions openOptions = new OpenOptions();
                        WorksetConfiguration openConfig = new WorksetConfiguration(WorksetConfigurationOption.OpenLastViewed);

                        IList<WorksetPreview> worksets;
                        try
                        {
                            worksets = WorksharingUtils.GetUserWorksetInfo(modelPath);
                        }
                        catch (Exception)
                        {
                            TaskDialog.Show("Warning!", "Can't obtain workset info! Project will open with last viewed workset config!");
                            worksets = new List<WorksetPreview>();
                            openConfig = new WorksetConfiguration(WorksetConfigurationOption.OpenLastViewed);
                        }
                        IList<WorksetId> worksetIds = new List<WorksetId>();
                        foreach (WorksetPreview workset in worksets)
                        {
                            
                            if(workset.Name.Contains("FL_TTT")) worksetIds.Add(workset.Id);
                        }
                        openConfig.Open(worksetIds);
                        openOptions.SetOpenWorksetsConfiguration(openConfig);

                        //Document doc = app.Application.OpenDocumentFile(modelPath, openOptions, new OpenFromCloud());
                        //List<Workset> worksets = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).Where(e => e.Name.Contains("FL_TTT")).ToList();
                        //openConfig.Open(worksets.Select(e => e.Id).ToList());
                        //openOptions.SetOpenWorksetsConfiguration(openConfig);

                        UIDocument uidoc = app.OpenAndActivateDocument(modelPath, openOptions, false);

                        viewModel.GetUiDocReference(uidoc);
                    }
                    else
                    {
                        TaskDialog.Show("Information:", "There is no cloud model associated with this project, please contact application admin!");
                    }
                }
                if (viewModel.VisTableAction == VisTableAction.SetMaterialsAndWorksets)
                {
                    if (app.ActiveUIDocument == null) return;
                    Document doc = app.ActiveUIDocument.Document;
                    MaterialWorksetManager.AddMaterialsAndWorksets(doc, viewModel);
                }
                if (viewModel.VisTableAction == VisTableAction.SelectInModel)
                {
                    if (app.ActiveUIDocument == null) return;
                    var selectedIds = new List<ElementId>();
                    foreach (var item in viewModel.SelectedRevitIds)
                    {
                        if(item != null)
                        {
                            string uniqueId = item.ToString();
                            ElementId elementId = app.ActiveUIDocument.Document.GetElement(uniqueId)?.Id;
                            if(elementId != null) selectedIds.Add(elementId);
                        }
                    }
                    if(selectedIds.Count > 0)
                    {
                        app.ActiveUIDocument.Selection.SetElementIds(selectedIds);
                        app.ActiveUIDocument.ShowElements(selectedIds);
                    }
                }
                if(viewModel.VisTableAction == VisTableAction.RemoveFromModel)
                {
                    if (app.ActiveUIDocument == null)
                    {
                        TaskDialog.Show("Warning!", "Document not found!");
                        viewModel.Browser.ExecuteScriptAsync("setTextInfo('Import cancelled. Document not found!');");
                        return;
                    }
                    if (app.ActiveUIDocument.Document.IsFamilyDocument)
                    {
                        TaskDialog.Show("Warning!", "Document is a family document!");
                        viewModel.Browser.ExecuteScriptAsync("setTextInfo('Import cancelled. Document is a family document!');");
                        return;
                    }
                    Document doc = app.ActiveUIDocument.Document;
                    if (!CheckDocumentBinding(viewModel, doc)) return;

                    if (app.ActiveUIDocument == null) return;
                    var selectedIds = new List<ElementId>();
                    foreach (var item in viewModel.SelectedRevitIds)
                    {
                        if (item != null)
                        {
                            string uniqueId = item.ToString();
                            ElementId elementId = app.ActiveUIDocument.Document.GetElement(uniqueId).Id;
                            selectedIds.Add(elementId);
                        }
                    }

                    using (Transaction tx = new Transaction(doc, "Remove layout objects"))
                    {
                        tx.Start();
                        foreach (var item in selectedIds)
                        {
                            Element e = doc.GetElement(item);
                            Parameter parDateCreated = e.LookupParameter("TTT_DateCreated");
                            Parameter parCreatedBy = e.LookupParameter("TTT_CreatedBy");

                            if (e != null)
                            {
                                FamilyInstance fi = e as FamilyInstance;
                                if (fi != null)
                                {
                                    ElementId symbolId = fi.Symbol.Id;
                                    ElementId familyId = fi.Symbol.Family.Id;
                                    doc.Delete(fi.Id);
                                    doc.Delete(familyId);
                                }
                            }

                        }
                        tx.Commit();
                    }
                    string jsonString = JsonConvert.SerializeObject(viewModel.SelectedRevitIds);
                    viewModel.Browser.ExecuteScriptAsync($"removeFromApprovedData2({jsonString});");
                    viewModel.Browser.ExecuteScriptAsync("setTextInfo('Selected items removed!');");
                }
                if(viewModel.VisTableAction == VisTableAction.CreateRevitModel)
                {
                    //open template doc in background
                    Guid fpTemplateGuid = new Guid("67242f8f-6855-483f-81c7-7b7ad1af2a73");
                    Guid projectGuid = new Guid("004f4643-59b4-4061-bf00-ec8e737bd85b");
                    OpenOptions openOptions = new OpenOptions();
                    openOptions.DetachFromCentralOption = DetachFromCentralOption.DoNotDetach;
                    ModelPath modelPath = ModelPathUtils.ConvertCloudGUIDsToCloudPath(ModelPathUtils.CloudRegionUS, projectGuid, fpTemplateGuid);
                    UIDocument templateDocument = app.OpenAndActivateDocument(modelPath, openOptions,false);

                    //save locally

                    string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string parentDirectory = Directory.GetParent(assemblyDirectory).FullName;
                    string tempDirectory = Path.Combine(parentDirectory, "temp");
                    if(!Directory.Exists(tempDirectory)) Directory.CreateDirectory(tempDirectory);
                    string savePath = Path.Combine(tempDirectory, "SavedTemplate.rvt");

                    SaveAsOptions options = new SaveAsOptions();
                    WorksharingSaveAsOptions wsao = new WorksharingSaveAsOptions();
                    wsao.SaveAsCentral = true;
                    options.OverwriteExistingFile = true;
                    options.SetWorksharingOptions(wsao);
                    templateDocument.Document.SaveAs(savePath, options);

                    //save as cloud model
                    Guid accountId = new Guid("d4d9f881-8f37-4242-8a5c-407a7fba9f23");
                    Guid projectId = new Guid("b8be4d49-f3e4-446c-8fc5-9614a087ec95");
                    Guid modelGuid = Guid.NewGuid();
                    string folderUrn = viewModel.ModelCreationData["Urn"];
                    string modelName = viewModel.ModelCreationData["ModelName"];
                    string folderName = viewModel.ModelCreationData["FolderName"];
                    templateDocument.Document.SaveAsCloudModel(accountId, projectId, folderUrn, modelName);
                    viewModel.Browser.ExecuteScriptAsync($"getFolderContent('{folderUrn}','{folderName}',false);");

                    Directory.Delete(tempDirectory, true);
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
                app.DialogBoxShowing -= OnDialogBoxShowing;
            }
        }

        public string GetName()
        {
            return "VisTableAppEventHandler";
        }

        private void OnDialogBoxShowing(object sender, DialogBoxShowingEventArgs e)
        {
            if (e.DialogId == "Dialog_Revit_DocWarnDialog")
            {               
                e.OverrideResult((int)TaskDialogResult.Ok);
            }
        }

        private bool CheckDocumentBinding(VisTableAppModel viewModel, Document doc)
        {
            Guid modelGuid = new Guid();
            if (doc.IsModelInCloud) modelGuid = doc.GetCloudModelPath().GetModelGUID();
            ProjectModelGuid projectModelGuid = viewModel.ProjectSettingsManager.ProjectGuids.Where(e => e.ProjectName == viewModel.ProjectName).FirstOrDefault();
            if (projectModelGuid == null || projectModelGuid.ModelGuid != modelGuid)
            {
                var result = TaskDialog.Show("Warning!", "Opened document is not set as current visTable project destination model! Are you sure you want to continue?", TaskDialogCommonButtons.No | TaskDialogCommonButtons.Yes);
                if (result == TaskDialogResult.No)
                {
                    viewModel.Browser.ExecuteScriptAsync("setTextInfo('Import cancelled. Wrong active model!');");
                    return false;
                }
            }
            return true;
        }
    }

    public class OpenFromCloud : IOpenFromCloudCallback
    {
        public OpenConflictResult OnOpenConflict(OpenConflictScenario scenario)
        {
            return OpenConflictResult.DiscardLocalChangesAndOpenLatestVersion;
        }
    }
}
