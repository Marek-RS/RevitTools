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
    /// Interaction logic for LookupParameterWindow.xaml
    /// </summary>
    public partial class LookupParameterWindow : Window
    {
        public bool WindowResult = false;
        public string SelectedLookup;
        public LookupParameterWindow(List<string> lookups, Window ownerWindow)
        {
            Owner = ownerWindow;
            InitializeComponent();
            ComBoxLookup.ItemsSource = lookups;
        }

        private void BtnClick_OK(object sender, RoutedEventArgs e)
        {
            SelectedLookup = ComBoxLookup.SelectedItem as string;
            if(SelectedLookup != null) WindowResult = true;
            Close();
        }

        private void BtnClick_Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
