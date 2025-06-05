using Autodesk.Revit.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.ExStorageSchema
{
	public class ElementUpdater : IUpdater
	{
		public bool IsActive { get; set; }

		const string guid = "0A656593-3A3D-410C-A697-7A358E22347C";
		UpdaterId _updater_id;

		public ElementUpdater(Document doc, AddInId addInId)
		{
			_updater_id = new UpdaterId(addInId, new Guid(guid));
			RegisterUpdater(doc);
			RegisterTriggers();
			IsActive = true;
			_doc = doc;
		}

		public void DisableUpdater()
		{
			UpdaterRegistry.RemoveAllTriggers(_updater_id);
			UpdaterRegistry.DisableUpdater(_updater_id);
			UpdaterRegistry.RemoveDocumentTriggers(_updater_id, _doc);
			UpdaterRegistry.UnregisterUpdater(_updater_id, _doc);
			IsActive = false;
		}
		Document _doc;

		public void RegisterUpdater(Document doc)
		{
			if (UpdaterRegistry.IsUpdaterRegistered(_updater_id, doc))
			{
				UpdaterRegistry.RemoveAllTriggers(_updater_id);
				UpdaterRegistry.UnregisterUpdater(_updater_id, doc);
			}
			UpdaterRegistry.RegisterUpdater(this, doc);
		}

		public void RegisterTriggers()
		{
			if (null != _updater_id && UpdaterRegistry.IsUpdaterRegistered(_updater_id))
			{
				UpdaterRegistry.RemoveAllTriggers(_updater_id);
				UpdaterRegistry.AddTrigger(_updater_id, new ElementCategoryFilter(BuiltInCategory.OST_Walls), Element.GetChangeTypeElementAddition());
				UpdaterRegistry.AddTrigger(_updater_id, new ElementCategoryFilter(BuiltInCategory.OST_Walls), Element.GetChangeTypeAny());
				UpdaterRegistry.AddTrigger(_updater_id, new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves), Element.GetChangeTypeElementAddition());
				UpdaterRegistry.AddTrigger(_updater_id, new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves), Element.GetChangeTypeAny());

			}
		}

		public void Execute(UpdaterData data)
		{
			if (!IsActive) return;
			Document doc = data.GetDocument();
			ICollection<ElementId> newIds = data.GetAddedElementIds();
			ICollection<ElementId> modifiedIds = data.GetModifiedElementIds();
			TTTSchema schema = TTTSchema.Initialize();
			foreach (ElementId id in newIds)
			{
				Element element = doc.GetElement(id);
				ParameterSet parameters = element.Parameters;
				List<Parameter> orderedParameters = new List<Parameter>();
				foreach (Parameter parameter in parameters) orderedParameters.Add(parameter);
				orderedParameters = orderedParameters.OrderBy(e => e.Definition.Name).ToList();

				Dictionary<string, string> dict = new Dictionary<string, string>();
				foreach (Parameter p in orderedParameters)
				{
					string content = "";
					switch (p.StorageType)
					{
						case StorageType.None:
							content = "StorageType.None";
							break;
						case StorageType.Integer:
							content = p.AsInteger().ToString();
							break;
						case StorageType.Double:
							content = p.AsDouble().ToString();
							break;
						case StorageType.String:
							content = p.AsString();
							break;
						case StorageType.ElementId:
							content = p.AsElementId().IntegerValue.ToString();
							break;
						default:
							break;
					}
					
					if (dict.ContainsKey(p.Definition.Name)) continue;
					dict.Add(p.Definition.Name, content);
				}
				string data0 = JsonConvert.SerializeObject(dict);

				SchemaDataModel model = new SchemaDataModel() { DataField0 = data0, DataField1 = "", DataField2 = "", DataField3 = "", WriteIndex = 1 };
				schema.WriteToSchema(model, element);
			}

			foreach (ElementId id in modifiedIds)
			{
				Element element = doc.GetElement(id);
				ParameterSet parameters = element.Parameters;
				List<Parameter> orderedParameters = new List<Parameter>();
				foreach (Parameter parameter in parameters) orderedParameters.Add(parameter);
				orderedParameters = orderedParameters.OrderBy(e => e.Definition.Name).ToList();
				Dictionary<string, string> dict = new Dictionary<string, string>();
				foreach (Parameter p in orderedParameters)
				{
					string content = "";
					switch (p.StorageType)
					{
						case StorageType.None:
							content = "StorageType.None";
							break;
						case StorageType.Integer:
							content = p.AsInteger().ToString();
							break;
						case StorageType.Double:
							content = p.AsDouble().ToString();
							break;
						case StorageType.String:
							content = p.AsString();
							break;
						case StorageType.ElementId:
							content = p.AsElementId().IntegerValue.ToString();
							break;
						default:
							break;
					}
					if (dict.ContainsKey(p.Definition.Name)) continue;
					dict.Add(p.Definition.Name, content);
				}
				string newData = JsonConvert.SerializeObject(dict);
				SchemaDataModel readModel = schema.ReadFromSchema(element);
				if (readModel == null)
                {
					SchemaDataModel model = new SchemaDataModel() {DataField0 = "empty", DataField1 = newData, DataField2 = "", DataField3 = "", WriteIndex = 2 };
					schema.WriteToSchema(model, element);
					return;
				}
				if(readModel.WriteIndex == 1)
                {
					SchemaDataModel model = new SchemaDataModel() {DataField0 = readModel.DataField0,  DataField1 = newData, DataField2 = readModel.DataField2, DataField3 = readModel.DataField3, WriteIndex = 2 };
					schema.WriteToSchema(model, element);
				}
				if (readModel.WriteIndex == 2)
				{
					SchemaDataModel model = new SchemaDataModel() { DataField0 = readModel.DataField0, DataField1 = readModel.DataField1, DataField2 = newData, DataField3 = readModel.DataField3, WriteIndex = 3 };
					schema.WriteToSchema(model, element);
				}
				if (readModel.WriteIndex == 3)
				{
					SchemaDataModel model = new SchemaDataModel() { DataField0 = readModel.DataField0, DataField1 = readModel.DataField1, DataField2 = readModel.DataField2, DataField3 = newData, WriteIndex = 1 };
					schema.WriteToSchema(model, element);
				}
			}
		}

		public string GetAdditionalInformation()
		{
			return "NA";
		}

		public ChangePriority GetChangePriority()
		{
			return ChangePriority.MEPSystems;
		}

		public UpdaterId GetUpdaterId()
		{
			return _updater_id;
		}

		public string GetUpdaterName()
		{
			return "WallUpdater";
		}
	}
}
