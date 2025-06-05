using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TTTRevitTools.Settings
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsViewModel SettingsViewModel { get; set; }
        VersionUpdater _versionUpdater;

        public SettingsWindow(VersionUpdater versionUpdater)
        {
            _versionUpdater = versionUpdater;
            SettingsViewModel = versionUpdater.SettingsViewModel;
            DataContext = SettingsViewModel;
            SetOwner();
            InitializeComponent();
        }

        private void SetOwner()
        {
            WindowHandleSearch search = WindowHandleSearch.MainWindowHandle;
            search.SetAsOwner(this);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            _versionUpdater.UpdateTheTools();
            SettingsViewModel.IsUpdateAvailable = false;
            SettingsViewModel.UpdateInfo = "Restart required!";
        }
    }
}
