using Autodesk.Revit.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.ConvoidOpenings
{
    internal class OpeningUpdater : IUpdater
    {
        private static Guid _guid = new Guid("093F33E5-A5AD-4B42-B628-7F46F11CABF8");
        private  UpdaterId _updaterId = new UpdaterId(App.UIContApp.ActiveAddInId, _guid);
        private Document _doc;
        private ConvoidViewModel _convoidViewModel;
        private string[] _familyNames;

        private OpeningUpdater() { }

        public static OpeningUpdater Initialize(Document doc, ConvoidViewModel convoidViewModel)
        {
            OpeningUpdater updater = new OpeningUpdater();
            updater._doc = doc;
            updater._convoidViewModel = convoidViewModel;
            convoidViewModel.UpdaterStatus = "Status: Active";
            return updater;
        }

        public void DeactivateUpdater(string deactivationInfo)
        {
            if (UpdaterRegistry.IsUpdaterRegistered(_updaterId, _doc))
            {            
                UpdaterRegistry.RemoveAllTriggers(_updaterId);
                UpdaterRegistry.UnregisterUpdater(_updaterId, _doc);
                _convoidViewModel.UpdaterStatus = "Status: Inactive";
                _convoidViewModel.LogInfo += deactivationInfo + Environment.NewLine;
            }
        }

        public void ActivateUpdater(string activationType)
        {
            RegisterUpdater();
            RegisterTriggers();
            _convoidViewModel.UpdaterStatus = "Status: Active";
            _convoidViewModel.LogInfo += activationType + Environment.NewLine;
        }

        public void RegisterUpdater()
        {
            if (UpdaterRegistry.IsUpdaterRegistered(_updaterId, _doc))
            {
                UpdaterRegistry.RemoveDocumentTriggers(_updaterId, _doc);
                UpdaterRegistry.UnregisterUpdater(_updaterId, _doc);
            }
            UpdaterRegistry.RegisterUpdater(this, _doc);
        }

        public void RegisterTriggers()
        {
            if(_updaterId != null && UpdaterRegistry.IsUpdaterRegistered(_updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(_updaterId);
                UpdaterRegistry.AddTrigger(_updaterId, _doc, new ElementCategoryFilter(BuiltInCategory.OST_GenericModel), Element.GetChangeTypeElementAddition());
                UpdaterRegistry.AddTrigger(_updaterId, _doc, new ElementCategoryFilter(BuiltInCategory.OST_GenericModel), Element.GetChangeTypeGeometry());
                UpdaterRegistry.AddTrigger(_updaterId, _doc, new ElementCategoryFilter(BuiltInCategory.OST_GenericModel), Element.GetChangeTypeElementDeletion());
            }
        }

        public void Execute(UpdaterData data)
        {
            _familyNames = _convoidViewModel.UpdaterFamilies.Where(e => e.IsChecked).Select(e => e.Name).ToArray();

            ICollection<ElementId> newIds = data.GetAddedElementIds();
            ICollection<ElementId> modifiedIds = data.GetModifiedElementIds();
            ICollection<ElementId> deletedIds = data.GetDeletedElementIds();
            List<OpeningModel> newOpenings = new List<OpeningModel>();
            List<OpeningModel> modifiedOpenings = new List<OpeningModel>();
            List<OpeningModel> deletedOpenings = new List<OpeningModel>();



            foreach (ElementId id in newIds)
            {
                FamilyInstance fi = _doc.GetElement(id) as FamilyInstance;
                if (fi != null)
                {
                    if (!_familyNames.Contains(fi.Symbol.Family.Name)) continue;
                    OpeningModel op = OpeningModel.Initialize(fi);
                    op.SetNewOpeningParameters();
                    op.GetLocationPoint();
                    op.GetHostElementsData(_convoidViewModel.HostLinkedModels, _convoidViewModel.HostReferenceLookups);
                    op.GetReferenceElementsData(_convoidViewModel.ReferenceLinkedModels, _convoidViewModel.HostReferenceLookups);
                    op.SetGridsData(_convoidViewModel.HorizontalGrids, _convoidViewModel.VerticalGrids);
                    op.SetLevelsData(_convoidViewModel.ModelLevels);
                    string addedId = op.SetUniqueIdentifier(_convoidViewModel.UniqueIdentifiers);
                    if (addedId != null) _convoidViewModel.UniqueIdentifiers.Add(addedId);
                    newOpenings.Add(op);
                }
            }
            foreach (ElementId id in modifiedIds)
            {
                FamilyInstance fi = _doc.GetElement(id) as FamilyInstance;
                if (fi != null)
                {
                    if (!_familyNames.Contains(fi.Symbol.Family.Name)) continue;
                    OpeningModel op = OpeningModel.Initialize(fi);
                    op.SetModifiedOpeningParameters();
                    op.GetLocationPoint();
                    op.GetHostElementsData(_convoidViewModel.HostLinkedModels, _convoidViewModel.HostReferenceLookups);
                    op.GetReferenceElementsData(_convoidViewModel.ReferenceLinkedModels, _convoidViewModel.HostReferenceLookups);
                    op.SetGridsData(_convoidViewModel.HorizontalGrids, _convoidViewModel.VerticalGrids);
                    op.SetLevelsData(_convoidViewModel.ModelLevels);
                    string addedId = op.SetUniqueIdentifier(_convoidViewModel.UniqueIdentifiers);
                    if (addedId != null) _convoidViewModel.UniqueIdentifiers.Add(addedId);
                    modifiedOpenings.Add(op);
                }
            }

            foreach (ElementId id in deletedIds)
            {
                
            }

            if(newOpenings.Count > 0)
            {
                //string path_new = @"\\BER02P1EGEAP001\www\convoid\data\new_openings_" + DateTime.Now.ToString("dd-MM-yy_hh-mm-ss") + ".json";
                _convoidViewModel.LogInfo += String.Format("Created {0} new openings..." + Environment.NewLine, newOpenings.Count);
                //string jsonString = JsonConvert.SerializeObject(newOpenings, Formatting.Indented);
                //StreamWriter sw_new = new StreamWriter(path_new);
                //sw_new.Write(jsonString);
                //sw_new.FlushAsync();
            }

            if (modifiedOpenings.Count > 0)
            {
                //string path_mod = @"\\BER02P1EGEAP001\www\convoid\data\mod_openings_" + DateTime.Now.ToString("dd-MM-yy_hh-mm-ss") + ".json";
                _convoidViewModel.LogInfo += String.Format("Modified {0} openings..." + Environment.NewLine, modifiedOpenings.Count);
                //string jsonString = JsonConvert.SerializeObject(modifiedOpenings, Formatting.Indented);
                //StreamWriter sw_mod = new StreamWriter(path_mod);
                //sw_mod.Write(jsonString);
                //sw_mod.FlushAsync();
            }

            if (deletedIds.Count > 0)
            {
                _convoidViewModel.LogInfo += String.Format("Deleted {0} openings..." + Environment.NewLine, deletedIds.Count);
            }
        }

        public string GetAdditionalInformation()
        {
            return "NA";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.DoorsOpeningsWindows;
        }

        public UpdaterId GetUpdaterId()
        {
            return _updaterId;
        }

        public string GetUpdaterName()
        {
            return "ConvoidOpeningUpdater";
        }
    }
}
