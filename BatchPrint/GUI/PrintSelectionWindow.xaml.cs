using System;
using System.Windows;

namespace TTTRevitTools.BatchPrint.GUI
{
    /// <summary>
    /// Interaction logic for PrintSelectionWindow.xaml
    /// </summary>
    public partial class PrintSelectionWindow : Window
    {
        public BatchPrintViewModel BatchPrintViewModel { get; set; }
        public PrintSelectionWindow(BatchPrintViewModel batchPrintViewModel)
        {
            BatchPrintViewModel = batchPrintViewModel;
            BatchPrintViewModel.TheWindow = this;
            this.DataContext = BatchPrintViewModel;
            SetOwner();
            InitializeComponent();
        }

        private void SetOwner()
        {
            WindowHandleSearch windowHandleSearch = WindowHandleSearch.MainWindowHandle;
            windowHandleSearch.SetAsOwner(this);
        }

        public void DisplayWindow()
        {
            if (BatchPrintViewModel.ViewSheets.Count > 0) BtnPrint.IsEnabled = true;
            BatchPrintViewModel.SelectionChangedSubscriber.Raise();
            Show();
        }

        private void BtnClick_PrintSelected(object sender, RoutedEventArgs e)
        {
            BatchPrintViewModel.PrintingEvent.Raise();
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            BatchPrintViewModel.SelectionChangedSubscriber.Raise();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;
            if (DwgExportBox.IsChecked == false && PdfExportBox.IsChecked == false) BtnPrint.IsEnabled = false;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;
            if (DwgExportBox.IsChecked == true || PdfExportBox.IsChecked == true) BtnPrint.IsEnabled = true;
        }
    }
}
