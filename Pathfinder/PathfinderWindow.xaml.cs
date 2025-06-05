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
using System.Windows.Shapes;

namespace TTTRevitTools.Pathfinder
{
    /// <summary>
    /// Interaction logic for PathfinderWindow.xaml
    /// </summary>
    public partial class PathfinderWindow : Window
    {
        public PathfinderViewModel PathfinderViewModel { get; set; }
        ExternalEvent _theEvent;

        public PathfinderWindow(PathfinderViewModel pathfinderViewModel, ExternalEvent theEvent)
        {
            _theEvent = theEvent;
            PathfinderViewModel = pathfinderViewModel;
            DataContext = PathfinderViewModel;
            SetOwner();
            InitializeComponent();
        }

        private void SetOwner()
        {
            WindowHandleSearch search = WindowHandleSearch.MainWindowHandle;
            search.SetAsOwner(this);
        }

        private void MainDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PathfinderViewModel.SelectedItem = MainDataGrid.SelectedItem as PathfinderDatagridItem;
            if(PathfinderViewModel.SelectedItem != null)
            {
                _theEvent.Raise();
            }
        }
    }
}
