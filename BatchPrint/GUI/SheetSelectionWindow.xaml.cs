using Autodesk.Revit.UI;
using System.Windows;

namespace TTTRevitTools.BatchPrint.GUI
{
    /// <summary>
    /// Interaction logic for SheetSelectionWindow.xaml
    /// </summary>
    public partial class SheetSelectionWindow : Window
    {
        ExternalEvent _externalEvent;
        public BatchPrintViewModel BatchPrintViewModel { get; set; }
        public SheetSelectionWindow(BatchPrintViewModel batchPrintViewModel, ExternalEvent externalEvent)
        {
            _externalEvent = externalEvent;
            BatchPrintViewModel = batchPrintViewModel;
            this.DataContext = BatchPrintViewModel;
            SetOwnerWindow();
            InitializeComponent();          
        }

        private void BtnClick_PrintSelected(object sender, RoutedEventArgs e)
        {
            _externalEvent.Raise();
            Close();
        }

        private void BtnClick_TestDeleteForm(object sender, RoutedEventArgs e)
        {
            //CustomPrintForm.DeleteCustomForm("PDFCreator", "TTT_1584x594");
        }

        private void SetOwnerWindow()
        {
            var rvtWindow = WindowHandleSearch.MainWindowHandle;
            rvtWindow.SetAsOwner(this);
        }
    }
}
