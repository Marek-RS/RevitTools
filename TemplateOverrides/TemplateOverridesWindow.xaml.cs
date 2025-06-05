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

namespace TTTRevitTools.TemplateOverrides
{
    /// <summary>
    /// Interaction logic for TemplateOverridesWindow.xaml
    /// </summary>
    public partial class TemplateOverridesWindow : Window
    {
        public TemplateOverridesViewModel TemplateOverridesViewModel { get; set; }
        public TemplateOverridesWindow(TemplateOverridesViewModel templateOverridesViewModel)
        {
            TemplateOverridesViewModel = templateOverridesViewModel;
            this.DataContext = TemplateOverridesViewModel;
            SetOwner();
            InitializeComponent();
            DestinationTemplatesDataGrid.Items.Refresh();
            SourceTemplatesDatagrid.Items.Refresh();
        }

        private void SetOwner()
        {
            WindowHandleSearch search = WindowHandleSearch.MainWindowHandle;
            search.SetAsOwner(this);
        }

        private void SourceTemplatesDatagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsInitialized) return;
            if(SourceTemplatesDatagrid.SelectedItem as TemplateViewModel != null)
            {
                TemplateFiltersDataGrid.ItemsSource = TemplateOverridesViewModel.SelectedFilters;
                TemplateFiltersDataGrid.ItemsSource = (SourceTemplatesDatagrid.SelectedItem as TemplateViewModel).Filters;
                TemplateFiltersDataGrid.Items.Filter = null;
                TxtBoxFilterFilters.Text = "Filter";
                TxtBoxFilterFilters.Foreground = Brushes.Gray;
                TemplateFiltersDataGrid.Items.Refresh();
            }
        }

        private void BtnClick_Apply(object sender, RoutedEventArgs e)
        {
            List<FilterViewModel> selectedFilters = new List<FilterViewModel>();
            foreach (FilterViewModel filterViewModel in TemplateFiltersDataGrid.Items)
            {
                if (filterViewModel.IsSelected) selectedFilters.Add(filterViewModel);
            }
            
            foreach (TemplateViewModel templateViewModel in DestinationTemplatesDataGrid.Items)
            {
                if (templateViewModel.IsSelected)
                {
                    templateViewModel.SetFilters(selectedFilters, TemplateOverridesViewModel._doc);
                }
            }
            Close();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            if (checkBox == null) return;
            string name = checkBox.Name;
            if(name == "CheckBoxFilters")
            {
                if(TemplateFiltersDataGrid.SelectedItems.Count > 1)
                {
                    foreach (FilterViewModel item in TemplateFiltersDataGrid.SelectedItems)
                    {
                        item.IsSelected = true;
                    }
                }
            }
            if (name == "CheckBoxTemplates")
            {
                if (DestinationTemplatesDataGrid.SelectedItems.Count > 1)
                {
                    foreach (TemplateViewModel item in DestinationTemplatesDataGrid.SelectedItems)
                    {
                        item.IsSelected = true;
                    }
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            if (checkBox == null) return;
            string name = checkBox.Name;
            if (name == "CheckBoxFilters")
            {
                if (TemplateFiltersDataGrid.SelectedItems.Count > 1)
                {
                    foreach (FilterViewModel item in TemplateFiltersDataGrid.SelectedItems)
                    {
                        item.IsSelected = false;
                    }
                }
            }
            if (name == "CheckBoxTemplates")
            {
                if (DestinationTemplatesDataGrid.SelectedItems.Count > 1)
                {
                    foreach (TemplateViewModel item in DestinationTemplatesDataGrid.SelectedItems)
                    {
                        item.IsSelected = false;
                    }
                }
            }
        }

        private void TxtBoxFilterSource_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(SetFilterTextBox(TxtBoxFilterSource))
            {
                Predicate<object> filter = new Predicate<object>(item => (item as TemplateViewModel).View.Name.ToUpper().Contains(TxtBoxFilterSource.Text.ToUpper()));
                SourceTemplatesDatagrid.Items.Filter = filter;
            }
        }

        private bool SetFilterTextBox(TextBox textBox)
        {
            bool result = true;
            if (textBox.IsFocused) return result;
            if (textBox.Text == "Filter")
            {
                textBox.Foreground = Brushes.Gray;
                result = false;
            }
            if (textBox.Text == "")
            {
                textBox.Foreground = Brushes.Gray;
                textBox.Text = "Filter";
                result = false;
            }
            return result;
        }

        private void TxtBoxFilter_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox == null) return;
            SetFilterTextBox(textBox);
        }

        private void TxtBoxFilter_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox == null) return;
            if (textBox.Text == "Filter")
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void TxtBoxFilterDestination_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SetFilterTextBox(TxtBoxFilterDestination))
            {
                Predicate<object> filter = new Predicate<object>(item => (item as TemplateViewModel).View.Name.ToUpper().Contains(TxtBoxFilterDestination.Text.ToUpper()));
                DestinationTemplatesDataGrid.Items.Filter = filter;
            }
        }

        private void TxtBoxFilterFilters_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SetFilterTextBox(TxtBoxFilterFilters) && TxtBoxFilterFilters.Text != "Filter")
            {
                Predicate<object> filter = new Predicate<object>(item => (item as FilterViewModel).ParameterFilterElement.Name.ToUpper().Contains(TxtBoxFilterFilters.Text.ToUpper()));
                TemplateFiltersDataGrid.Items.Filter = filter;
            }
        }
    }
}
