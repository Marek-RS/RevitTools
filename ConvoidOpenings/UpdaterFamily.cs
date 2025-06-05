using Autodesk.Revit.DB;
using CefSharp.DevTools.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.ConvoidOpenings
{
    public class UpdaterFamily : INotifyPropertyChanged
    {
        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { 
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }
        public string Name { get; set; }

        private Family _family;
        private List<Definition> _missingDefinitions = new List<Definition>();

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //public for deserialization, not for initialization use
        public UpdaterFamily()
        {

        }

        public static UpdaterFamily Initialize(Family family)
        {
            UpdaterFamily updaterFamily = new UpdaterFamily();
            updaterFamily._family = family;
            updaterFamily.Name = family.Name;
            updaterFamily.IsChecked = false;
            return updaterFamily;
        }

        public Family GetFamily()
        {
            return _family;
        }

        public void SetFamily(Family family)
        {
            _family = family;
            _missingDefinitions.Clear();
        }

        public void CheckDefinitions(List<Definition> definitions, Document doc)
        {
            _missingDefinitions = new List<Definition>();
            if (!_family.IsValidObject) return;

            Document famDoc = doc.EditFamily(_family);
            FamilyManager fmgr = famDoc.FamilyManager;
            foreach (Definition f in definitions)
            {
                ExternalDefinition external = f as ExternalDefinition;
                Guid pGuid = external.GUID;
                FamilyParameter p = fmgr.get_Parameter(pGuid);
                if (p == null) _missingDefinitions.Add(f);
            }
            famDoc.Close(false);
        }

        public Family AddDefinitions(Document doc)
        {
            Family family = null;
            Document famDoc = doc.EditFamily(_family);
            using(Transaction tx = new Transaction(famDoc, "Add family parameters")) 
            {
                tx.Start();
                foreach (Definition definition in _missingDefinitions)
                {
                    FamilyParameter fp = famDoc.FamilyManager.AddParameter(definition as ExternalDefinition, BuiltInParameterGroup.INVALID, true);
                    if (fp.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
                    {
                        famDoc.FamilyManager.Set(fp, 0);
                    }
                }
                family = famDoc.LoadFamily(doc, new VisTableImport.FamilyLoadOptions());
                tx.Commit();
            }
            famDoc.Close(false);
            return family;
        }

        public bool HasMissingDefinitions()
        {
            return _missingDefinitions.Count > 0;
        }
    }
}
