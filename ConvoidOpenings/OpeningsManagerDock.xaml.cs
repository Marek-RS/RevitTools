using Autodesk.Revit.UI;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TTTRevitTools.ConvoidOpenings
{
    /// <summary>
    /// Interaction logic for OpeningsManagerDock.xaml
    /// </summary>
    public partial class OpeningsManagerDock : Page, IDockablePaneProvider
    {
        public OpeningsManagerDock(ConvoidViewModel viewModel)
        {
            InitializeComponent();
            SetMainControl(viewModel);
        }

        private void SetMainControl(ConvoidViewModel viewModel)
        {
            viewModel.LogInfo = "Initializing dock panel main control..." + Environment.NewLine;
            DockUserControl dockUserControl = new DockUserControl(viewModel);
            MainContentControl.Content = dockUserControl;
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;
            data.InitialState = new DockablePaneState();
            data.InitialState.DockPosition = DockPosition.Right;
        }
    }
}
