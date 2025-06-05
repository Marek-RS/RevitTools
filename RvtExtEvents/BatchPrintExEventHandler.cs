using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using TTTRevitTools.BatchPrint;
using TTTRevitTools.ProgressDisplay;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace TTTRevitTools.RvtExtEvents
{
    public class BatchPrintExEventHandler : IExternalEventHandler
    {       
        public BatchPrintExEventHandler(List<string> printers, BatchPrintViewModel batchPrintViewModel)
        {
            _printers = printers;
            _batchPrintViewModel = batchPrintViewModel;
        }

        List<string> _printers;
        BatchPrintViewModel _batchPrintViewModel;

        public void Execute(UIApplication app)
        {
            try
            {
                RevitPrinting revitPrinting = new RevitPrinting(app.ActiveUIDocument.Document, _batchPrintViewModel.OverridePrinterAccess);
                //revitPrinting.GetViewSheetSet(_batchPrintViewModel.ViewSheetModels);
                revitPrinting.ViewSheets = _batchPrintViewModel.ViewSheets;
                revitPrinting.GetTitleBlocks();

                ProcessWindow processWindow = new ProcessWindow();
                processWindow.UpdateControls("Batch printer started...");
                processWindow.ProgressBar.Maximum = revitPrinting.ViewSheets.Count;
                processWindow.ProgressBar.Value = 0;
                processWindow.Show();

                if (App.Instance.BatchPrintViewModel.PrintPdfs)
                {
                    string defaultPrinterName = "PDFCreator";
                    if (!_printers.Contains(defaultPrinterName))
                    {
                        //default printer name has been changed, need to pass exisiting name
                        defaultPrinterName = _printers.FirstOrDefault();
                    }

                    if (PrinterWrapper.CheckPDFCreatorInstances())
                    {
                        processWindow.TextBoxInfo.Text += "Application can not close PDFCreator running instances..." + Environment.NewLine;
                        processWindow.BtnCancel.IsEnabled = false;
                        processWindow.BtnClose.IsEnabled = true;
                        DoEvents();
                        return;
                    }
                    //revitPrinting.AddNewSizes();
                    //revitPrinting.ResetPrinter();
                    PrinterWrapper.InitializeJobQueue();

                    foreach (ViewSheet vs in revitPrinting.ViewSheets)
                    {
                        if (processWindow.UserCancelled)
                        {
                            PrinterWrapper.CancelPrinting();
                            break;
                        }
                        revitPrinting.PrintSingleSheetToPdf(vs, defaultPrinterName);
                        processWindow.UpdateControls("Printing: " + vs.Name);
                        DoEvents();
                    }
                }

                App.Instance.BatchPrintViewModel.SetPrintNameDictionary();
                if (processWindow.UserCancelled)
                {
                    PrinterWrapper.CancelPrinting();
                    //close enabled
                    DoEvents();
                }
                else
                {
                    processWindow.Close();

                    CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                    dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    dialog.IsFolderPicker = true;
                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        if (App.Instance.BatchPrintViewModel.PrintPdfs) PrinterWrapper.AwaitJobs(App.Instance.BatchPrintViewModel.PrintNameDictionary, dialog.FileName);
                        if (App.Instance.BatchPrintViewModel.PrintDwgs) revitPrinting.ExportSheetsToDwgs(dialog.FileName);
                    }
                    else
                    {
                        if (App.Instance.BatchPrintViewModel.PrintPdfs) PrinterWrapper.CancelPrinting();
                        Autodesk.Revit.UI.TaskDialog.Show("Error", "No directory selected!");
                    }
                    if (App.Instance.BatchPrintViewModel.PrintPdfs) revitPrinting.ChangePaperSizeToDefault();
                }
            }
            catch (Exception ex)
            {
                Autodesk.Revit.UI.TaskDialog.Show("Error", ex.ToString());
                PrinterWrapper.CancelPrinting();
            }        
        }

        public string GetName()
        {
            return "BatchPrinter";
        }

        public void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
            new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        public object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;
            return null;
        }
    }
}
