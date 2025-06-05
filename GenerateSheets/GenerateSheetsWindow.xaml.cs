using Autodesk.Revit.DB;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TTTRevitTools.GenerateSheets
{
    /// <summary>
    /// Interaction logic for GenerateSheetsWindow.xaml
    /// </summary>
    public partial class GenerateSheetsWindow : Window
    {
        public GenerateSheetsViewModel GenerateSheetsViewModel { get; set; }
        public bool WindowResult = false;

        public GenerateSheetsWindow(GenerateSheetsViewModel generateSheetsViewModel)
        {
            GenerateSheetsViewModel = generateSheetsViewModel;
            DataContext = GenerateSheetsViewModel;
            SetOwner();
            InitializeComponent();
            ComBoxTitleBlocks.SelectedIndex = GenerateSheetsViewModel.GetTitleBlockIndex();
            ComBoxViewports.SelectedIndex = GenerateSheetsViewModel.GetPortTypeIndex();
            BtnSaveSettings.IsEnabled = false;
        }

        private void SetOwner()
        {
            WindowHandleSearch search = WindowHandleSearch.MainWindowHandle;
            search.SetAsOwner(this);
        }

        private void BtnClick_CreateViewsAndSheets(object sender, RoutedEventArgs e)
        {
            if(ComBoxTitleBlocks.SelectedItem == null)
            {
                MainTabControl.SelectedIndex = 3;
                MessageBox.Show("Please select TitleBlock family!");
                return;
            }
            if (ComBoxViewports.SelectedItem == null)
            {
                MainTabControl.SelectedIndex = 3;
                MessageBox.Show("Please select ViewPort type!");
                return;
            }
            if(RadBtnUseExistingSizes.IsChecked == true)
            {
                GenerateSheetsViewModel.UseExisitngTitleBlockSizes = true;
            }
            ElementType elementType = ComBoxViewports.SelectedItem as ElementType;
            if (elementType != null) GenerateSheetsViewModel.SelectedViewPort = elementType;
            if (GenerateSheetsViewModel.GetSelectedItems())
            {
                GenerateSheetsViewModel.SetSelectedScale(ComBoxViewScale.SelectedItem.ToString());
                WindowResult = true;
                GenerateSheetsViewModel.GenerateSheetsAction = GenerateSheetsAction.CreateViewsAndSheets;
                GenerateSheetsViewModel.CreateSheets = true;
                GenerateSheetsViewModel.TheEvent.Raise();
                RoomSelectionDataGrid.Items.Refresh();
            }
        }

        private void RoomCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (RoomDataGridItem item in RoomSelectionDataGrid.SelectedItems)
            {
                item.IsSelected = true;
            }
        }

        private void RoomCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (RoomDataGridItem item in RoomSelectionDataGrid.SelectedItems)
            {
                item.IsSelected = false;
            }
        }

        private void ComBoxViewFamilyTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            {
                ComboBox callingBox = (ComboBox)sender;
                ViewFamilyType selectedFamilyType = callingBox.SelectedItem as ViewFamilyType;
                if (selectedFamilyType != null)
                {
                    ViewTypeDataGridItem viewTypeDataGridItem = callingBox.DataContext as ViewTypeDataGridItem;
                    viewTypeDataGridItem.SelectedViewFamilyType = selectedFamilyType;
                }
            }
        }

        private void ComBoxViewTemplates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox callingBox = (ComboBox)sender;
            View selectedViewTemplate = callingBox.SelectedItem as View;
            if(selectedViewTemplate != null)
            {
                ViewTypeDataGridItem viewTypeDataGridItem = callingBox.DataContext as ViewTypeDataGridItem;
                viewTypeDataGridItem.SelectedViewTemplate = selectedViewTemplate;
            }
        }

        private void BtnClick_SaveChanges(object sender, RoutedEventArgs e)
        {
            GenerateSheetsViewModel.SaveNamingRules();
            BtnSaveChanges.IsEnabled = false;
        }

        int _p1SelectionChangedCount = 0;
        private void ComboBox_P1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _p1SelectionChangedCount++;
            if (!IsInitialized) return;
            ComboBox callingBox = (ComboBox)sender;
            SelectableParameter selectedParameter = (SelectableParameter)callingBox.SelectedItem;
            ViewNamingRule currentViewNaminRule = callingBox.DataContext as ViewNamingRule;
            currentViewNaminRule.Parameter1 = selectedParameter;
            BtnSaveChanges.IsEnabled = true;
        }

        int _p2SelectionChangedCount = 0;
        private void ComboBox_P2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {           
            _p2SelectionChangedCount++;
            if (!IsInitialized) return;
            ComboBox callingBox = (ComboBox)sender;
            SelectableParameter selectedParameter = (SelectableParameter)callingBox.SelectedItem;
            ViewNamingRule currentViewNaminRule = callingBox.DataContext as ViewNamingRule;
            currentViewNaminRule.Parameter2 = selectedParameter;
            BtnSaveChanges.IsEnabled = true;
        }

        int _p3SelectionChangedCount = 0;
        private void ComboBox_P3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _p3SelectionChangedCount++;
            if (!IsInitialized) return;
            ComboBox callingBox = (ComboBox)sender;
            SelectableParameter selectedParameter = (SelectableParameter)callingBox.SelectedItem;
            ViewNamingRule currentViewNaminRule = callingBox.DataContext as ViewNamingRule;
            currentViewNaminRule.Parameter3 = selectedParameter;
            BtnSaveChanges.IsEnabled = true;
        }

        int _p4SelectionChangedCount = 0;
        private void ComboBox_P4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _p4SelectionChangedCount++;
            if (!IsInitialized) return;
            ComboBox callingBox = (ComboBox)sender;
            SelectableParameter selectedParameter = (SelectableParameter)callingBox.SelectedItem;
            ViewNamingRule currentViewNaminRule = callingBox.DataContext as ViewNamingRule;
            currentViewNaminRule.Parameter4 = selectedParameter;
            BtnSaveChanges.IsEnabled = true;
        }

        private void NamingDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            BtnSaveChanges.IsEnabled = true;
        }

        private void BtnCick_SaveViewPorts(object sender, RoutedEventArgs e)
        {
            if(incorrectData)
            {
                Autodesk.Revit.UI.TaskDialog.Show("Warning!", "Value must be a number!");
                return;
            }
            GenerateSheetsViewModel.SaveViewPortSettings();
            BtnSaveSettings.IsEnabled = false;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(IsInitialized)
            {
                BtnSaveSettings.IsEnabled = true;
                ComboBox comboBox = (ComboBox)sender;
                switch (comboBox.Name)
                {
                    case "ComBoxTitleBlocks":
                        GenerateSheetsViewModel.ViewSheetSettings.TitleBlockFamilyId = (comboBox.SelectedItem as Family).Id.IntegerValue;
                        break;
                    case "ComBoxViewports":
                        GenerateSheetsViewModel.ViewSheetSettings.ViewPortTypeId = (comboBox.SelectedItem as ElementType).Id.IntegerValue;
                        break;
                    case "ComBoxViewScale":
                        GenerateSheetsViewModel.SetSelectedScale(ComBoxViewScale.SelectedItem.ToString());
                        break;
                    default:                       
                        break;
                }
            }
        }

        

        private void OffsetBox_Changed(object sender, TextChangedEventArgs e)
        {
            if (IsInitialized)
            {
                TextBox txtBox = (TextBox)sender;
                bool test = double.TryParse(txtBox.Text, out double result);
                if(test)
                {
                    txtBox.Foreground = Brushes.Black;
                    incorrectData = false;
                }
                else
                {
                    txtBox.Foreground = Brushes.Red;
                    Autodesk.Revit.UI.TaskDialog.Show("Warning!", "Value must be a number!");
                    incorrectData = true;
                }
                BtnSaveSettings.IsEnabled = true;
            }
        }

        bool incorrectData = false;

        private void ChckBoxViewScale_Changed(object sender, RoutedEventArgs e)
        {
            if(IsInitialized)
            {
                BtnSaveSettings.IsEnabled = true;
                if (ChckBoxViewScale.IsChecked == true)
                {
                    GenerateSheetsViewModel.ViewSheetSettings.OverrideScaleInTemplate = true;
                }
                else
                {
                    GenerateSheetsViewModel.ViewSheetSettings.OverrideScaleInTemplate = false;
                }
            }
        }

        private void RoomSelectionDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RoomDataGridItem roomDataGridItem = RoomSelectionDataGrid.SelectedItem as RoomDataGridItem;
            if (roomDataGridItem == null) return;
            GenerateSheetsViewModel.SelectedRoom = roomDataGridItem;
            GenerateSheetsViewModel.GenerateSheetsAction = GenerateSheetsAction.OpenSheetView;
            GenerateSheetsViewModel.TheEvent.Raise();
        }

        private void BtnClickAddViews(object sender, RoutedEventArgs e)
        {
            AddViewsWindow addViewsWindow = new AddViewsWindow(GenerateSheetsViewModel, this);
            addViewsWindow.ShowDialog();
        }

        private void RadBtn_Checked_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                BtnSaveSettings.IsEnabled = true;
            }
        }

        private void RoomSelectionDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if(RoomSelectionDataGrid.Items.Count > 0)
            {
                RoomSelectionDataGrid.Visibility = System.Windows.Visibility.Visible;
                InfoLabel.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                RoomSelectionDataGrid.Visibility = System.Windows.Visibility.Hidden;
                InfoLabel.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            RoomDataGridItem item = RoomSelectionDataGrid.SelectedItem as RoomDataGridItem;
            if(item != null)
            {
                GenerateSheetsViewModel.SelectedRoom = item;
                GenerateSheetsViewModel.GenerateSheetsAction = GenerateSheetsAction.RemoveViews;
                GenerateSheetsViewModel.TheEvent.Raise();
            }
        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            RoomDataGridItem item = RoomSelectionDataGrid.SelectedItem as RoomDataGridItem;
            if (item != null)
            {
                GenerateSheetsViewModel.SelectedRoom = item;
                GenerateSheetsViewModel.GenerateSheetsAction = GenerateSheetsAction.OpenAllViews;
                GenerateSheetsViewModel.TheEvent.Raise();
            }
        }

        private void MenuItemAddRow_Click(object sender, RoutedEventArgs e)
        {
            GenerateSheetsViewModel.ParameterSubstrings.Add(new ParameterSubstring());
            ParameterSubstringDataGrid.CommitEdit();
            ParameterSubstringDataGrid.CommitEdit();
            ParameterSubstringDataGrid.Items.Refresh();
        }

        private void MenuItemAddRowFindReplace_Click(object sender, RoutedEventArgs e)
        {
            GenerateSheetsViewModel.FindReplaceParameters.Add(new ParameterFindReplace() { OldText = "", NewText = "" });
            ParameterFindReplaceDataGrid.CommitEdit();
            ParameterFindReplaceDataGrid.CommitEdit();
            ParameterFindReplaceDataGrid.Items.Refresh();
        }

        private void BtnSaveModifiers_Click(object sender, RoutedEventArgs e)
        {
            GenerateSheetsViewModel.SaveParameterModifiers();
            MessageBox.Show("Modifiers are saved!");
        }

        private void MenuItemDeleteRowSubstring_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ParameterSubstring item = ParameterSubstringDataGrid.SelectedItem as ParameterSubstring;
                if (item != null) GenerateSheetsViewModel.ParameterSubstrings.Remove(item);
                ParameterSubstringDataGrid.CommitEdit();
                ParameterSubstringDataGrid.CommitEdit();
                ParameterSubstringDataGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while deleting modifier rule: " + ex.ToString());
            }
        }

        private void MenuItemDeleteRowFindReplace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ParameterFindReplace item = ParameterFindReplaceDataGrid.SelectedItem as ParameterFindReplace;
                if (item != null) GenerateSheetsViewModel.FindReplaceParameters.Remove(item);
                ParameterFindReplaceDataGrid.CommitEdit();
                ParameterFindReplaceDataGrid.CommitEdit();
                ParameterFindReplaceDataGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while deleting modifier rule: " + ex.ToString());
            }

        }

        private void BtnClick_SaveParameterValues_ViewSheet_TitleBlock(object sender, RoutedEventArgs e)
        {
            GenerateSheetsViewModel.SaveParameterValues();
            MessageBox.Show("Parameter values are saved!");
        }

        private void BtnClick_ImportSettings(object sender, RoutedEventArgs e)
        {
            SettingsManager.ImportSettings();
        }

        private void BtnClick_ExportSettings(object sender, RoutedEventArgs e)
        {
            SettingsManager.ExportSettings();
        }
    }
}
