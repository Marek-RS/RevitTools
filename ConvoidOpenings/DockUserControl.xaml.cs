using Autodesk.Revit.DB;
using DocumentFormat.OpenXml.Vml.Office;
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
using TeslaRevitTools.FindAffectedSheets;

namespace TeslaRevitTools.ConvoidOpenings
{
    /// <summary>
    /// Interaction logic for DockUserControl.xaml
    /// </summary>
    public partial class DockUserControl : UserControl
    {
        public ConvoidViewModel ConvoidViewModel { get; set; }

        public DockUserControl(ConvoidViewModel convoidViewModel)
        {
            ConvoidViewModel = convoidViewModel;
            ConvoidViewModel.GetUpdaterModels();

            InitializeComponent();
            DataContext = ConvoidViewModel;
            if(ConvoidViewModel.UpdaterStatus == "Status: Active") StatusLabel.Foreground = Brushes.Green;
        }

        private void LogBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LogBox.ScrollToEnd();
        }

        private void BtnClick_DeactivateUpdater(object sender, RoutedEventArgs e)
        {
            ConvoidViewModel.Action = ConvoidOpeningsAction.DeactivateUpdater;
            StatusLabel.Foreground = Brushes.Red;
            ConvoidViewModel.TheEvent.Raise();
        }

        private void BtnClick_ActivateUpdater(object sender, RoutedEventArgs e)
        {
            ConvoidViewModel.Action = ConvoidOpeningsAction.ActivateUpdater;
            StatusLabel.Foreground = Brushes.Green;
            ConvoidViewModel.TheEvent.Raise();
        }

        private void BtnClick_AddActiveModel(object sender, RoutedEventArgs e)
        {
            ConvoidViewModel.AddCurentModel();
        }

        private void BtnClick_SaveUpdaterModels(object sender, RoutedEventArgs e)
        {
            ConvoidViewModel.SaveUpdaterModels();
        }

        private void BtnClick_AddDevParameters(object sender, RoutedEventArgs e)
        {
            ConvoidViewModel.Action = ConvoidOpeningsAction.AddDevParameters;
            ConvoidViewModel.TheEvent.Raise();
        }

        private void BtnClick_AddAllOpenings(object sender, RoutedEventArgs e)
        {
            ConvoidViewModel.Action = ConvoidOpeningsAction.GetAllOpenings;
            ConvoidViewModel.TheEvent.Raise();
        }

        private void BtnClick_AddOpening(object sender, RoutedEventArgs e)
        {
            ConvoidViewModel.Action = ConvoidOpeningsAction.GetSelectedOpenings;
            ConvoidViewModel.TheEvent.Raise();          
        }

        private void BtnClick_AddViewOpenings(object sender, RoutedEventArgs e)
        {
            ConvoidViewModel.Action = ConvoidOpeningsAction.GetViewOpenings;
            ConvoidViewModel.TheEvent.Raise();
        }

        private void BtnClick_ClearExportList(object sender, RoutedEventArgs e)
        {
            ConvoidViewModel.ExportCoordinates.Clear();
        }

        private void BtnClick_SaveUpdaterFamilies(object sender, RoutedEventArgs e)
        {
            ConvoidViewModel.SaveUpdaterFamilies();
        }

        private void MenuItem_ShareLinkData(object sender, RoutedEventArgs e)
        {
            //Get the clicked MenuItem
            var menuItem = (MenuItem)sender;

            //Get the ContextMenu to which the menuItem belongs
            var contextMenu = (ContextMenu)menuItem.Parent;

            //Find the placementTarget
            var dataGrid = (DataGrid)contextMenu.PlacementTarget;
            var link = dataGrid.SelectedItem as LinkedModelData;
            if(link != null)
            {
                ConvoidViewModel.RevitLinkManager.SendLinkData(link);
            }
        }

        private void MenuItem_DownloadLinkData(object sender, RoutedEventArgs e)
        {

        }

        bool _isClicked = false;

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isClicked) return;
            CheckBox chbox = sender as CheckBox;
            UpdaterFamily uf = chbox.DataContext as UpdaterFamily;
            ConvoidViewModel.SingleChecked = uf;
            if(uf != null)
            {
                ConvoidViewModel.Action = ConvoidOpeningsAction.AddDevParametersSingle;
                ConvoidViewModel.TheEvent.Raise();
            }
            _isClicked = false;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _isClicked = false;
        }

        private void CheckBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isClicked = true;
        }

        private void BtnClick_UpdateAllOpenings(object sender, RoutedEventArgs e)
        {
            ConvoidViewModel.Action = ConvoidOpeningsAction.UpdateAllOpenings;
            ConvoidViewModel.TheEvent.Raise();
        }

        private void BtnClick_UpdateViewOpenings(object sender, RoutedEventArgs e)
        {
            ConvoidViewModel.Action = ConvoidOpeningsAction.UpdateViewOpenings;
            ConvoidViewModel.TheEvent.Raise();
        }

        private void BtnClick_UpdateSelectedOpenings(object sender, RoutedEventArgs e)
        {
            ConvoidViewModel.Action = ConvoidOpeningsAction.UpdateSelectedOpenings;
            ConvoidViewModel.TheEvent.Raise();
        }

        private void BtnClick_SaveCoordsAs(object sender, RoutedEventArgs e)
        {
            ConvoidViewModel.SaveExportedOpeningsAs();
        }

        private void ExportGrid_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = CoordinatesDataGrid.SelectedItem as ExportCoordinateData;
            if(selectedItem != null) 
            {
                ConvoidViewModel.SelectedId = selectedItem.GetElementId();
                ConvoidViewModel.Action = ConvoidOpeningsAction.SelectInRevit;
                ConvoidViewModel.TheEvent.Raise();
            }
        }

        private void BtnClick_GetDefaultCoords(object sender, RoutedEventArgs e)
        {
            ConvoidViewModel.GetDefaultCoords();
            TransformDataGrid.Items.Refresh();
        }

        private void BtnClick_SetViewTagRefs(object sender, RoutedEventArgs e)
        {
            ConvoidViewModel.Action = ConvoidOpeningsAction.SetViewTagRefs;
            ConvoidViewModel.TheEvent.Raise();
        }

        private void BtnClick_ExportAllOpenings(object sender, RoutedEventArgs e)
        {
            ConvoidViewModel.Action = ConvoidOpeningsAction.ExportAll;
            ConvoidViewModel.TheEvent.Raise();
        }

        private void BtnClick_ExportViewOpenings(object sender, RoutedEventArgs e)
        {
            ConvoidViewModel.Action = ConvoidOpeningsAction.ExportView;
            ConvoidViewModel.TheEvent.Raise();
        }

        private void BtnClick_ExportSelectedOpenings(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemClick_RefreshLinksData(object sender, RoutedEventArgs e)
        {
            ConvoidViewModel.ReferenceLinkedModels.Clear();
            ConvoidViewModel.HostLinkedModels.Clear();
            ConvoidViewModel.RefreshLinksData();
            ReferenceLinkDataGrid.Items.Refresh();
            HostLinkDataGrid.Items.Refresh();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
