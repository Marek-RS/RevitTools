using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.Settings
{
    public class VersionUpdater
    {
        public SettingsViewModel SettingsViewModel { get; set; }
        public bool InternetConnection { get; set; }
        string _downloadedBuiltNumber;
        string _currentBuildNumber;
        string _revitVersionYear;
        string _downloadDirectory;

        private string _appDataDirPath;
        public string AppDataDirPath
        {
            get
            {
                if (_appDataDirPath != null)
                {
                    return _appDataDirPath;
                }
                else
                {
                    _appDataDirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    return _appDataDirPath;
                }
            }
        }

        public async void CheckForUpdates(SettingsViewModel settingsViewModel, string revitVersionYear)
        {
            SettingsViewModel = settingsViewModel;
            _revitVersionYear = revitVersionYear;
            await CheckForInternetConnection();
            if (!InternetConnection)
            {
                //No internet connection
                SettingsViewModel.UpdateInfo = "No internet connection!";
                SettingsViewModel.IsUpdateAvailable = false;
                return;
            }
            string importDirectory = Path.Combine(AppDataDirPath, "Autodesk", "Revit", "Addins", _revitVersionYear, "TTTRevitTools");
            _currentBuildNumber = SettingsViewModel.Version;
            await DownloadFiles(importDirectory, _revitVersionYear, false);
            CompareVersions(importDirectory, _revitVersionYear, SettingsViewModel);
            //DisplayWindow(SettingsViewModel);
        }

        public void DisplayWindow()
        {
            SettingsWindow window = new SettingsWindow(this);
            window.ShowDialog();
        }

        public void UpdateTheTools()
        {
            if(SettingsViewModel.IsUpdateAvailable)
            {
                string newVersionDirectory = Path.Combine(Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName, _downloadedBuiltNumber);
                string manifestPath = Path.Combine(AppDataDirPath, "Autodesk", "Revit", "Addins", _revitVersionYear, "TTTRevitTools.addin");

                Directory.CreateDirectory(newVersionDirectory);
                foreach (string filePath in Directory.GetFiles(_downloadDirectory))
                {
                    string fileName = Path.GetFileName(filePath);
                    string newPath = Path.Combine(newVersionDirectory, fileName);
                    File.Move(filePath, newPath);
                }
                CreateManifestFile(manifestPath);
            }
        }

        private void CreateManifestFile(string manifestFilePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "TTTRevitTools.Resources.TTTRevitTools.txt";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                string newManifestSource = result.Replace("XXXXXXXXXX", _downloadedBuiltNumber);
                File.WriteAllText(manifestFilePath, newManifestSource);
            }
        }

        private async Task CheckForInternetConnection()
        {
            try
            {
                string url = "https://www.google.com";

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                request.Timeout = 4000;
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    InternetConnection = true;
                }
            }
            catch
            {
                InternetConnection = false;
            }
            await Task.Delay(1000);
        }

        private async Task DownloadFiles(string importDirectory, string version, bool includeJson)
        {
            _downloadDirectory = Path.Combine(importDirectory, "Downloads");

            string dll1DownloadPath = string.Format("http://www.yourapp.pl/downloads/TTT/TTTRevitTools/TTTRevitTools{0}.dll", version);
            string dll1SavePath = Path.Combine(_downloadDirectory, "TTTRevitTools.dll");

            string dll2DownloadPath = string.Format("http://www.yourapp.pl/downloads/TTT/TTTRevitTools/CustomRevitTools{0}.dll", version);
            string dll2SavePath = Path.Combine(_downloadDirectory, "CustomRevitTools.dll");

            string jsonDownloadPath = "http://www.yourapp.pl/downloads/TTT/TTTRevitTools/cleaner_settings.json";
            string jsonSavePath = Path.Combine(importDirectory, "cleaner_settings.json");

            Uri uri1 = new Uri(dll1DownloadPath);
            Uri uri2 = new Uri(dll2DownloadPath);
            Uri uri3 = new Uri(jsonDownloadPath);
            using (var client = new WebClient())
            {
                await client.DownloadFileTaskAsync(uri1, dll1SavePath);
                await client.DownloadFileTaskAsync(uri2, dll2SavePath);
                if (includeJson) await client.DownloadFileTaskAsync(uri3, jsonSavePath);
            }
            CheckDownloadedBuildNumber(dll1SavePath);
            await Task.Delay(100);
        }

        private void CheckDownloadedBuildNumber(string path)
        {
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(path);
            _downloadedBuiltNumber = info.FileVersion;
        }

        private void CompareVersions(string pluginDirectory, string revitVersionYear, SettingsViewModel settingsViewModel)
        {
            var existingVersion = new Version(_currentBuildNumber);
            var newVersion = new Version(_downloadedBuiltNumber);
            var result = existingVersion.CompareTo(newVersion);
            if (result < 0)
            {
                //new version available
                settingsViewModel.IsUpdateAvailable = true;
                settingsViewModel.UpdateInfo = "Update is available!";
            }
            else
            {
                //exisitng plugin is up to date or newer
                settingsViewModel.IsUpdateAvailable = false;
                settingsViewModel.UpdateInfo = "Your current version is up to date!";
                string downloadDirectory = Path.Combine(pluginDirectory, "Downloads");
                foreach (string file in Directory.GetFiles(downloadDirectory))
                {
                    File.Delete(file);
                }
            }
        }
    }
}
