using System.Windows.Threading;
using CefSharp;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Windows;
using TTTRevitTools;
using System.ComponentModel;

namespace NavisworksGggbaseTTT
{
    /// <summary>
    /// Interaction logic for TTTDockPanel.xaml
    /// </summary>
    public partial class GggbaseWindow : Window
    {
        public GggbaseViewModel GggbaseViewModel { get; set; }

        public GggbaseWindow(GggbaseViewModel GggbaseViewModel)
        {
            GggbaseViewModel = GggbaseViewModel;
            DataContext = GggbaseViewModel;
            //SetOwner();
            InitializeComponent();
            GggbaseViewModel.Browser = Browser;
            Loaded += GggbaseWindow_Loaded;
        }

        private void GggbaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.DoWork += Worker_DoWork;
            GggbaseViewModel.Worker = worker;
            worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            GggbaseViewModel.GggbaseAction = TTTRevitTools.Gggbase.GggbaseAction.Collect;
            GggbaseViewModel.TheEvent.Raise();
            GggbaseViewModel.SignalEvent.WaitOne();
            GggbaseViewModel.SignalEvent.Reset();
            //GggbaseViewModel.CollectModelItems();
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            LoadingProgressBar.Value = (double)e.ProgressPercentage;
            ProgressState.Text = e.UserState.ToString();
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SubscribeEvents();
        }

        public void SubscribeEvents()
        {
            Browser.JavascriptMessageReceived += Browser_JavascriptMessageReceived;
            Browser.ConsoleMessage += Browser_ConsoleMessage;
            GggbaseViewModel.LogInfo += "Subscribing events..." + Environment.NewLine;
        }

        public void UnsubscribeEvents()
        {
            Browser.JavascriptMessageReceived -= Browser_JavascriptMessageReceived;
            Browser.ConsoleMessage -= Browser_ConsoleMessage;
            GggbaseViewModel.LogInfo += "Unubscribing events..." + Environment.NewLine;
        }

        private void SetOwner()
        {
            WindowHandleSearch windowHandleSearch = WindowHandleSearch.MainWindowHandle;
            windowHandleSearch.SetAsOwner(this);
        }

        private void Browser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            string logMessage = e.Message;
            //GggbaseViewModel.LogInfo += "Console message received:" + Environment.NewLine;
            GggbaseViewModel.LogInfo += logMessage + Environment.NewLine;
        }

        private void Browser_JavascriptMessageReceived(object sender, JavascriptMessageReceivedEventArgs e)
        {

            GggbaseViewModel.LogInfo += "CefSharp message received:" + Environment.NewLine;

            IDictionary<string, object> propertyValues = e.Message as ExpandoObject;
            object action = propertyValues["ActionType"];
            if (action != null && action as string != null)
            {
                string actionString = action as string;
                GggbaseViewModel.LogInfo += "Action type: " + actionString + Environment.NewLine;

                if (actionString == "SelectInNavis")
                {
                    UnsubscribeEvents();
                    IDictionary<string, object> data = propertyValues["Data"] as ExpandoObject;
                    GggbaseViewModel.MarksSelection = data["SelectedElements"] as List<object>;
                    GggbaseViewModel.GggbaseAction = TTTRevitTools.Gggbase.GggbaseAction.Select;

                    GggbaseViewModel.TheEvent.Raise();
                    GggbaseViewModel.SignalEvent.WaitOne();
                    GggbaseViewModel.SignalEvent.Reset();
                    SubscribeEvents();
                    //GggbaseViewModel.SelectByMarkInCustomParameters();
                }
                if (actionString == "ImportSelected")
                {
                    GggbaseViewModel.SendSelectedElements();
                }
                if (actionString == "ImportComplete")
                {
                    MessageBox.Show("Import complete", "Import Info");
                }
            }
        }

        private void GoToAddressBtnClick(object sender, RoutedEventArgs e)
        {
            Browser.Address = AddressBox.Text;
        }

        private void LogBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            LogBox.ScrollToEnd();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            GggbaseViewModel.GggbaseWindow = null;
            Browser.Dispose();
        }

        private void ExtendColumnBtnClick(object sender, RoutedEventArgs e)
        {
            if(ExtendableColumn.Width == new GridLength(8, GridUnitType.Pixel))
            {
                ExtendableColumn.Width = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                ExtendableColumn.Width = new GridLength(8, GridUnitType.Pixel);
            }
        }

        private void SelectionChangedChckBox_Checked(object sender, RoutedEventArgs e)
        {
            GggbaseViewModel.SubscribeSelectionChanged();
        }

        private void SelectionChangedChckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            GggbaseViewModel.UnsubscribeSelectionChanged();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GggbaseViewModel.UnsubscribeSelectionChanged();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
