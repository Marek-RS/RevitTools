using Autodesk.Revit.DB;
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

namespace TTTRevitTools.GenerateSheets
{
    /// <summary>
    /// Interaction logic for AddViewsWindow.xaml
    /// </summary>
    public partial class AddViewsWindow : Window
    {
        public GenerateSheetsViewModel GenerateSheetsViewModel { get; set; }
        GenerateSheetsWindow _window;
        public AddViewsWindow(GenerateSheetsViewModel generateSheetsViewModel, GenerateSheetsWindow window)
        {
            GenerateSheetsViewModel = generateSheetsViewModel;
            DataContext = GenerateSheetsViewModel;
            Owner = window;
            _window = window;
            InitializeComponent();
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            View view = AddViewsDataGrid.SelectedItem as View;
            if (view == null) return;
            GenerateSheetsViewModel.GetRooms(view);
            AddViewsDataGrid.Items.Filter = null;
            GenerateSheetsViewModel.PlanViews.Remove(view);
            AddViewsDataGrid.Items.Filter = _currentPredicate;
            AddViewsDataGrid.Items.Refresh();
        }

        private void FilterBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (FilterBox.Text == "Filter")
            {
                FilterBox.Clear();
                FilterBox.Foreground = Brushes.Black;
            }
        }

        Predicate<object> _currentPredicate;

        private void FilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsInitialized) return;
            string filter = FilterBox.Text;
            if (filter != "Filter")
            {
                _currentPredicate = new Predicate<object>(item => ((View)item).Name.ToUpper().Contains(filter.ToUpper()));
                AddViewsDataGrid.Items.Filter = _currentPredicate;
            }
            else
            {
                AddViewsDataGrid.Items.Filter = null;
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (FilterBox.IsFocused)
            {
                MainGrid.Focus();
                if (FilterBox.Text == "")
                {
                    FilterBox.Text = "Filter";
                    FilterBox.Foreground = Brushes.Gray;
                }
                else
                {
                    FilterBox.Foreground = Brushes.Black;
                }
            }
        }

        private void AddViewsDataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (FilterBox.IsFocused)
            {
                MainGrid.Focus();
                if (FilterBox.Text == "")
                {
                    FilterBox.Text = "Filter";
                    FilterBox.Foreground = Brushes.Gray;
                }
                else
                {
                    FilterBox.Foreground = Brushes.Black;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            AddViewsDataGrid.Items.Filter = null;
        }
    }
}
