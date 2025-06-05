using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Settings
{
    public class SettingsViewModel
    {
        public ObservableCollection<SearchParameter> SearchParameters { get; set; }

        private string _jsonSettingsPath;
        private string _GggBaseDirectory;

        private SettingsViewModel()
        {

        }

        public static SettingsViewModel Initialize()
        {
            SettingsViewModel result = new SettingsViewModel();
            result._GggBaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autodesk\\Navisworks Manage 2023\\TTTGggbase");
            result._jsonSettingsPath = Path.Combine(result._GggBaseDirectory, "settings.json");
            return result;
        }

        public void SerializeAndSave()
        {
            if (!Directory.Exists(_GggBaseDirectory)) Directory.CreateDirectory(_GggBaseDirectory);
            string jsonString = JsonConvert.SerializeObject(SearchParameters);
            File.WriteAllText(_jsonSettingsPath, jsonString);
        }

        public void GetSearchParameters()
        {
            if (!DeserializeAndLoad())
            {
                SearchParameters = new ObservableCollection<SearchParameter>();
                SearchParameter searchParameter = new SearchParameter() { ParameterName = "Mark", CategoryName = "Element", MinLength = 5 };
                if (!SearchParameters.Select(e => e.ParameterName).Contains(searchParameter.ParameterName)) SearchParameters.Add(searchParameter);
            }
            else
            {
                for (int i = SearchParameters.Count - 1; i >= 0; i--)
                {
                    SearchParameter sp = SearchParameters[i];
                    if (string.IsNullOrEmpty(sp.ParameterName) || string.IsNullOrEmpty(sp.CategoryName)) SearchParameters.RemoveAt(i);
                }
            }
        }

        private bool DeserializeAndLoad()
        {
            if (File.Exists(_jsonSettingsPath))
            {
                string jsonString = File.ReadAllText(_jsonSettingsPath);
                SearchParameters = JsonConvert.DeserializeObject<ObservableCollection<SearchParameter>>(jsonString);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
