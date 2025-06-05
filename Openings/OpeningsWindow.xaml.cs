using System;
using CefSharp;
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
using System.Dynamic;
using Newtonsoft.Json;
using Autodesk.Revit.UI;
using System.Net.Http;
using CefSharp.Web;

namespace TTTRevitTools.Openings
{
    /// <summary>
    /// Interaction logic for OpeningsWindow.xaml
    /// </summary>
    public partial class OpeningsWindow : Window
    {
        public OpeningsViewModel OpeningsViewModel { get; set; }
        public OpeningsWindow(OpeningsViewModel openingsViewModel)
        {
            OpeningsViewModel = openingsViewModel;
            DataContext = OpeningsViewModel;
            InitializeComponent();
            SubscribeEvents();
            ModeComBox.SelectedIndex = 0;
        }
        private void GoToAddressBtnClick(object sender, RoutedEventArgs e)
        {
            Browser.Address = AddressBox.Text;
        }

        private void LogBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            LogBox.ScrollToEnd();
        }

        private void ExtendColumnBtnClick(object sender, RoutedEventArgs e)
        {
            if (ExtendableColumn.Width == new GridLength(8, GridUnitType.Pixel))
            {
                ExtendableColumn.Width = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                ExtendableColumn.Width = new GridLength(8, GridUnitType.Pixel);
            }
        }

        public void SubscribeEvents()
        {
            Browser.JavascriptMessageReceived += Browser_JavascriptMessageReceived;
            Browser.ConsoleMessage += Browser_ConsoleMessage;
            OpeningsViewModel.LogInfo += "Subscribing events..." + Environment.NewLine;
        }

        public void UnsubscribeEvents()
        {
            Browser.JavascriptMessageReceived -= Browser_JavascriptMessageReceived;
            Browser.ConsoleMessage -= Browser_ConsoleMessage;
            OpeningsViewModel.LogInfo += "Unubscribing events..." + Environment.NewLine;
        }

        private void Browser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            string logMessage = e.Message;
            OpeningsViewModel.LogInfo += logMessage + Environment.NewLine;
        }

        private void Browser_JavascriptMessageReceived(object sender, JavascriptMessageReceivedEventArgs e)
        {

            OpeningsViewModel.LogInfo += "CefSharp message received:" + Environment.NewLine;

            IDictionary<string, object> propertyValues = e.Message as ExpandoObject;
            object action = propertyValues["ActionType"];
            if (action != null && action as string != null)
            {
                string actionString = action as string;
                OpeningsViewModel.LogInfo += "Action type: " + actionString + Environment.NewLine;

                if (actionString == "SelectInModel")
                {
                    UnsubscribeEvents();
                    IDictionary<string, object> data = propertyValues["Data"] as ExpandoObject;
                    List<Object> list = data["SelectedElements"] as List<object>;

                    IDictionary<string, object> test = list.First() as IDictionary<string, object>;

                    OpeningsViewModel.WebInfoDictionary = test;
                    OpeningsViewModel.OpeningsAction = OpeningsAction.SelectOpenings;
                    OpeningsViewModel.TheEvent.Raise();
                    OpeningsViewModel.SignalEvent.WaitOne();
                    OpeningsViewModel.SignalEvent.Reset();
                    SubscribeEvents();
                }
            }
        }

        private void BtnClick_FindOpenings(object sender, RoutedEventArgs e)
        {
            OpeningsViewModel.OpeningsAction = OpeningsAction.FindOpenings;
            OpeningsViewModel.TheEvent.Raise();
            OpeningsViewModel.SignalEvent.WaitOne();
            OpeningsViewModel.SignalEvent.Reset();
            OpeningsGrid.Items.Refresh();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OpeningsViewModel.SelectedMode = ModeComBox.SelectedItem as string;
        }

        private void BtnClick_SendSelected(object sender, RoutedEventArgs e)
        {
            OpeningModel model = OpeningsGrid.SelectedItem as OpeningModel;
            if(model != null)
            {
                OpeningsViewModel.SendOpening(model);
            }
        }
    }
}
