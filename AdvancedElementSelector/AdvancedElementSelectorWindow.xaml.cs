using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;

namespace TTTRevitTools.AdvancedElementSelector
{
    /// <summary>
    /// Interaction logic for AdvancedElementSelectorWindow.xaml
    /// </summary>
    public partial class AdvancedElementSelectorWindow : Window
    {
        public SelectorViewModel SelectorViewModel { get; set; }
        public AdvancedElementSelectorWindow(SelectorViewModel selectorViewModel)
        {
            SelectorViewModel = selectorViewModel;
            DataContext = SelectorViewModel;
            SelectorViewModel.MainWindow = this;
            SetOwner();
            InitializeComponent();
        }

        public void CloseChildWindows()
        {
            foreach (Window w in OwnedWindows)
            {
                w.Close();
            }
        }

        public void RefreshContext()
        {
            DataContext = SelectorViewModel;
            AlignCheckedElementIds();
        }

        private void SetOwner()
        {
            WindowHandleSearch handleSearch = WindowHandleSearch.MainWindowHandle;
            handleSearch.SetAsOwner(this);
        }

        bool tempIgnore = false;
        private void InstanceCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (tempIgnore) return;
            CheckBox checkBox = (CheckBox)sender;
            ListBox listBox = FindParent<ListBox>(checkBox);
            if(listBox != null)
            {
                tempIgnore = true;
                foreach (ExpanderModel model in listBox.SelectedItems)
                {
                    model.IsSelected = true;
                }
                tempIgnore = false;
            }

            foreach (ExpanderModel mainExpander in SelectorViewModel.MainExpanders)
            {
                foreach (ExpanderModel subExpander in mainExpander.SubExpanders)
                {
                    foreach (ExpanderModel instance in subExpander.SubExpanders)
                    {
                        if (instance.IsSelected == true)
                        {
                            if (!SelectorViewModel.CheckedElementIds.Contains(instance.ElementId)) SelectorViewModel.CheckedElementIds.Add(instance.ElementId);
                        }
                        else
                        {
                            if (SelectorViewModel.CheckedElementIds.Contains(instance.ElementId)) SelectorViewModel.CheckedElementIds.Remove(instance.ElementId);
                        }
                    }
                }
            }
            SelectorViewModel.TheAction = ElementSelectorAction.SelectItems;
            SelectorViewModel.TheEvent.Raise();
        }

        public void AlignTypeCheckbox(ExpanderModel expanderModel)
        {
            if (expanderModel.SubExpanders.Where(x => x.IsSelected == true).ToList().Count == expanderModel.SubExpanders.Count)
            {
                expanderModel.IsSelected = true;
            }
            else
            {
                if (expanderModel.SubExpanders.Where(x => x.IsSelected == true).ToList().Count == 0)
                {
                    if(expanderModel.SubExpanders.Where(x => x.IsSelected == null).ToList().Count == 0)
                    {
                        expanderModel.IsSelected = false;
                    }
                    else
                    {
                        expanderModel.IsSelected = null;
                    }
                }
                else
                {
                    expanderModel.IsSelected = null;
                }
            }
        }

        private void InstanceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            ListBox listBox = FindParent<ListBox>(checkBox);
            if(listBox != null)
            {
                tempIgnore = true;
                foreach (ExpanderModel model in listBox.SelectedItems)
                {
                    model.IsSelected = false;
                }
                tempIgnore = false;
            }

            foreach (ExpanderModel mainExpander in SelectorViewModel.MainExpanders)
            {
                foreach (ExpanderModel subExpander in mainExpander.SubExpanders)
                {
                    foreach (ExpanderModel instance in subExpander.SubExpanders)
                    {
                        if(instance.IsSelected == true)
                        {
                            if (!SelectorViewModel.CheckedElementIds.Contains(instance.ElementId)) SelectorViewModel.CheckedElementIds.Add(instance.ElementId);
                        }
                        else
                        {
                            if (SelectorViewModel.CheckedElementIds.Contains(instance.ElementId)) SelectorViewModel.CheckedElementIds.Remove(instance.ElementId);
                        }
                    }
                }
            }
            SelectorViewModel.TheAction = ElementSelectorAction.SelectItems;
            SelectorViewModel.TheEvent.Raise();
        }

        private static T FindParent<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(dependencyObject);
            if (parent == null) return null;
            var parentT = parent as T;
            return parentT ?? FindParent<T>(parent);
        }

        public void AlignCheckedElementIds()
        {
            foreach (ExpanderModel mainExpander in SelectorViewModel.MainExpanders)
            {
                foreach (ExpanderModel subExpander in mainExpander.SubExpanders)
                {
                    foreach (ExpanderModel instance in subExpander.SubExpanders)
                    {
                        if (instance.IsSelected == true)
                        {
                            if (!SelectorViewModel.CheckedElementIds.Contains(instance.ElementId)) SelectorViewModel.CheckedElementIds.Add(instance.ElementId);
                        }
                        else
                        {
                            if (SelectorViewModel.CheckedElementIds.Contains(instance.ElementId)) SelectorViewModel.CheckedElementIds.Remove(instance.ElementId);
                        }
                    }
                    AlignTypeCheckbox(subExpander);
                }
               AlignTypeCheckbox(mainExpander);
            }
        }    

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            if(checkBox.IsChecked == false)
            {
                ExpanderModel currentExpander = checkBox.DataContext as ExpanderModel;
                if (currentExpander == null || currentExpander.SubExpanders == null || currentExpander.SubExpanders.Count == 0) return;
                tempIgnore = true;
                foreach (ExpanderModel model in currentExpander.SubExpanders)
                {
                    model.IsSelected = false;
                }
                tempIgnore = false;
            }
            AlignCheckedElementIds();
            SelectorViewModel.TheAction = ElementSelectorAction.SelectItems;
            SelectorViewModel.TheEvent.Raise();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            ExpanderModel currentExpander = checkBox.DataContext as ExpanderModel;
            if (currentExpander.SubExpanders == null || currentExpander.SubExpanders.Count == 0) return;
            tempIgnore = true;
            foreach (ExpanderModel model in currentExpander.SubExpanders)
            {
                model.IsSelected = true;
            }
            tempIgnore = false;
            AlignCheckedElementIds();
            SelectorViewModel.TheAction = ElementSelectorAction.SelectItems;
            SelectorViewModel.TheEvent.Raise();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            SelectorViewModel.TheAction = ElementSelectorAction.Unsubscribe;
            SelectorViewModel.TheEvent.Raise();
        }

        private void Window_Initialized(object sender, System.EventArgs e)
        {
            SelectorViewModel.TheAction = ElementSelectorAction.Subscribe;
            SelectorViewModel.TheEvent.Raise();
        }

        private void ListBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ListBox listBox = (ListBox)sender;
            listBox.Items.Refresh();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem expander = (MenuItem)sender;
            ExpanderModel expanderModel = expander.DataContext as ExpanderModel;
            if (expanderModel == null) return;
            if(expanderModel.GetParameters())
            {
                ParameterFilterWindow parameterFilterWindow = new ParameterFilterWindow(expanderModel, this, SelectorViewModel);
                parameterFilterWindow.Show();
            }
            else
            {
                MessageBox.Show("Info", "Parameters not found!");
            }
        }

        private void CheckBox_Checked_Category(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            ExpanderModel categoryExpander = checkBox.DataContext as ExpanderModel;

            foreach (ExpanderModel eModel in categoryExpander.SubExpanders)
            {
                eModel.IsSelected = true;
                foreach (var instanceExpander in eModel.SubExpanders)
                {
                    instanceExpander.IsSelected = true;
                }
            }
            AlignCheckedElementIds();
            SelectorViewModel.TheAction = ElementSelectorAction.SelectItems;
            SelectorViewModel.TheEvent.Raise();
        }

        private void CheckBox_Unchecked_Category(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            ExpanderModel categoryExpander = checkBox.DataContext as ExpanderModel;
            if(categoryExpander != null)
            {
                foreach (ExpanderModel eModel in categoryExpander.SubExpanders)
                {
                    eModel.IsSelected = false;
                    foreach (var instanceExpander in eModel.SubExpanders)
                    {
                        instanceExpander.IsSelected = false;
                    }
                }
            }

            AlignCheckedElementIds();
            SelectorViewModel.TheAction = ElementSelectorAction.SelectItems;
            SelectorViewModel.TheEvent.Raise();
        }

        private void MainListBox_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            MainScrollViewer.ScrollToVerticalOffset(MainScrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
