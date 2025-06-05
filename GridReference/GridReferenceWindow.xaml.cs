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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TTTRevitTools.GridReference
{
    /// <summary>
    /// Interaction logic for GridReferenceWindow.xaml
    /// </summary>
    public partial class GridReferenceWindow : Window
    {
        public GridRefViewModel GridRefViewModel { get; set; }

        public GridReferenceWindow(GridRefViewModel gridRefViewModel, IntPtr handle)
        {
            SetAsOwner(this, handle);
            GridRefViewModel = gridRefViewModel;
            DataContext = GridRefViewModel;
            InitializeComponent();
        }

        private void SetAsOwner(Window childWindow, IntPtr handle)
        {
            var helper = new WindowInteropHelper(childWindow) { Owner = handle };
        }

        private void BtnClick_AddSelection(object sender, RoutedEventArgs e)
        {
            GridRefViewModel.TheAction = GridRefAction.AddSelection;
            GridRefViewModel.TheEvent.Raise();
        }

        private void BtnClick_FindGrids(object sender, RoutedEventArgs e)
        {
            GridRefViewModel.TheAction = GridRefAction.FindGrids;
            GridRefViewModel.TheEvent.Raise();
        }

        private void BtnClick_ClearList(object sender, RoutedEventArgs e)
        {
            GridRefViewModel.GridRefModels.Clear();
        }

        private void BtnClick_AddToParameter(object sender, RoutedEventArgs e)
        {
            if (GridRefViewModel.ParameterName == "Comments")
            {
                TaskDialog.Show("Info", "Comments is not allowed!");
                return;
            }
            var result = MessageBox.Show("Would you like to overwrite current parameter values?" + Environment.NewLine + "Parameter name: " + GridRefViewModel.ParameterName, "Info", MessageBoxButton.YesNo);
            GridRefViewModel.OverwriteParameterValues = false;
            if (result == MessageBoxResult.Yes) GridRefViewModel.OverwriteParameterValues = true;

            GridRefViewModel.TheAction = GridRefAction.AddParameter;
            GridRefViewModel.TheEvent.Raise();
        }

        private void MenuItemClick_SetBbox(object sender, RoutedEventArgs e)
        {
            foreach (GridRefModel item in SelectionGrid.SelectedItems)
            {
                item.SelectedType = PointType.BboxMiddle;
            }
        }

        private void MenuItemClick_SetLocationPoint(object sender, RoutedEventArgs e)
        {
            foreach (GridRefModel item in SelectionGrid.SelectedItems)
            {
                item.SelectedType = PointType.LocationPoint;
            }
        }
    }
}
