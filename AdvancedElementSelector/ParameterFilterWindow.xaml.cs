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

namespace TTTRevitTools.AdvancedElementSelector
{
    /// <summary>
    /// Interaction logic for ParameterFilterWindow.xaml
    /// </summary>
    public partial class ParameterFilterWindow : Window
    {
        public ExpanderModel ExpanderModel { get; set; }
        SelectorViewModel _selectorViewModel;
        public ParameterFilterWindow(ExpanderModel expanderModel, Window owner, SelectorViewModel selectorViewModel)
        {
            _selectorViewModel = selectorViewModel;
            ExpanderModel = expanderModel;
            DataContext = ExpanderModel;
            Owner = owner;
            InitializeComponent();
            Title = "Parameter Filters: " + ExpanderModel.ExpanderName;
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            ParameterModel pModel = MainDataGrid.SelectedItem as ParameterModel;
            if(pModel != null)
            {
                TextBox txtBox = e.EditingElement as TextBox;
                string input = txtBox.Text;
                if(pModel.CheckValue(input))
                {
                    if(!string.IsNullOrEmpty(pModel.SelectedOperator))
                    {
                        _selectorViewModel.CurrentExpander = ExpanderModel;
                        _selectorViewModel.TheAction = ElementSelectorAction.FilterSelection;
                        _selectorViewModel.TheEvent.Raise();
                    }
                }
                else
                {
                    txtBox.Text = "";
                    e.Cancel = false;
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if(comboBox != null)
            {
                ParameterModel parameterModel = comboBox.DataContext as ParameterModel;
                parameterModel.SelectedOperator = comboBox.SelectedItem as string;
                if(!string.IsNullOrEmpty(parameterModel.Value))
                {
                    _selectorViewModel.CurrentExpander = ExpanderModel;
                    _selectorViewModel.TheAction = ElementSelectorAction.FilterSelection;
                    _selectorViewModel.TheEvent.Raise();
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _selectorViewModel.CurrentExpander = null;
        }
    }
}
