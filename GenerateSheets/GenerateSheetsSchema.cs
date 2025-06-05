using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.GenerateSheets
{
    public class GenerateSheetsSchema
    {
        Schema _schema;
        Guid _schemaGuid = new Guid("F9B72D78-4EA2-45D4-97FB-44A2C170587A");

        private GenerateSheetsSchema()
        {

        }

        public static GenerateSheetsSchema Initialize()
        {
            GenerateSheetsSchema result = new GenerateSheetsSchema();
            result.GetSchema();
            return result;
        }

        public void WriteToSchema(string uniqueId, Element element)
        {
            SetEntityField(element, "sheet_view_unique_id", uniqueId);
        }

        public string ReadFromSchema(Element element)
        {
            Entity entity = element.GetEntity(_schema);
            if (entity.IsValid())
            {
                return entity.Get<string>(_schema.GetField("sheet_view_unique_id"));
            }
            else
            {
                return "";
            }
        }

        private void GetSchema()
        {
            _schema = Schema.Lookup(_schemaGuid);
            {
                if (_schema == null)
                {
                    _schema = CreateSchema();
                }
            }
        }

        private bool SetEntityField(Element element, string fieldName, string value)
        {
            bool result = false;
            Entity entity = element.GetEntity(_schema);
            Field field = _schema.GetField(fieldName);

            if (entity.IsValid())
            {
                entity.Set(field, value);
                element.SetEntity(entity);
                result = true;
            }
            else
            {
                SetSchemaEntity(element, field, value);
            }
            return result;
        }

        private bool SetEntityField(Element element, string fieldName, int value)
        {
            bool result = false;
            Entity entity = element.GetEntity(_schema);
            Field field = _schema.GetField(fieldName);

            if (entity.IsValid())
            {
                entity.Set(field, value);
                element.SetEntity(entity);
                result = true;
            }
            else
            {
                SetSchemaEntity(element, field, value);
            }
            return result;
        }

        private void SetSchemaEntity(Element element, Field field, string value)
        {
            Entity entity = new Entity(_schema);
            entity.Set<string>(field, value);
            element.SetEntity(entity);
        }

        private void SetSchemaEntity(Element element, Field field, int value)
        {
            Entity entity = new Entity(_schema);
            entity.Set<int>(field, value);
            element.SetEntity(entity);
        }

        private Schema CreateSchema()
        {
            SchemaBuilder schemaBuilder = new SchemaBuilder(_schemaGuid);
            schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
            schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
            schemaBuilder.SetVendorId("TTT");
            schemaBuilder.SetSchemaName("GenerateSheetsTTT");
            FieldBuilder sheetViewId = schemaBuilder.AddSimpleField("sheet_view_unique_id", typeof(string));

            sheetViewId.SetDocumentation("SheetView Unique Id");
            return schemaBuilder.Finish();
        }
    }
}
