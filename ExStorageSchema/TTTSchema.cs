using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;

namespace TTTRevitTools.ExStorageSchema
{
    public class TTTSchema
    {
        Schema _schema;
        Guid _schemaGuid = new Guid("4BB4F7C4-4109-4ECC-A7B7-0DBD10DEFAD6");

        private TTTSchema()
        {

        }

        public static TTTSchema Initialize()
        {
            TTTSchema result = new TTTSchema();
            result.GetSchema();
            return result;
        }

        public void WriteToSchema(SchemaDataModel schemaDataModel, Element element)
        {
            SetEntityField(element, "data_field_0", schemaDataModel.DataField0);
            SetEntityField(element, "data_field_1", schemaDataModel.DataField1);
            SetEntityField(element, "data_field_2", schemaDataModel.DataField2);
            SetEntityField(element, "data_field_3", schemaDataModel.DataField3);
            SetEntityField(element, "index_field", schemaDataModel.WriteIndex);
        }

        public SchemaDataModel ReadFromSchema(Element element)
        {
            SchemaDataModel result = new SchemaDataModel();
            Entity entity = element.GetEntity(_schema);
            if(entity.IsValid())
            {
                string data0 = entity.Get<string>(_schema.GetField("data_field_0"));
                string data1 = entity.Get<string>(_schema.GetField("data_field_1"));
                string data2 = entity.Get<string>(_schema.GetField("data_field_2"));
                string data3 = entity.Get<string>(_schema.GetField("data_field_3"));
                int index = entity.Get<int>(_schema.GetField("index_field"));
                result.DataField0 = data0;
                result.DataField1 = data1;
                result.DataField2 = data2;
                result.DataField3 = data3;
                result.WriteIndex = index;
            }
            else
            {
                return null;
            }
            return result;
        }

        private void GetSchema()
        {
            _schema = Schema.Lookup(_schemaGuid);
            {
                if(_schema == null)
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
            schemaBuilder.SetSchemaName("TTT_Optimiser");
            FieldBuilder data0 = schemaBuilder.AddSimpleField("data_field_0", typeof(string));
            FieldBuilder data1 = schemaBuilder.AddSimpleField("data_field_1", typeof(string));
            FieldBuilder data2 = schemaBuilder.AddSimpleField("data_field_2", typeof(string));
            FieldBuilder data3 = schemaBuilder.AddSimpleField("data_field_3", typeof(string));
            FieldBuilder index = schemaBuilder.AddSimpleField("index_field", typeof(int));
            data0.SetDocumentation("initial info on creation");
            data1.SetDocumentation("json string field 1");
            data2.SetDocumentation("json string field 2");
            data3.SetDocumentation("json string field 3");
            index.SetDocumentation("index to write json string");
            return schemaBuilder.Finish();
        }
    }
}
