using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;
using System.Collections.Generic;
using TTTRevitTools.BatchPrint;
using TTTRevitTools.BatchPrint.GUI;
using TTTRevitTools.RvtExtEvents;
using System;

namespace TTTRevitTools.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class BatchPrintCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                bool canUsePrinter;
                string ourVersion = "5.1.1";
                string version = PrinterWrapper.GetVersion(out canUsePrinter, ourVersion);
                if (!canUsePrinter)
                {
                    string info = string.Format("You need to install PDF Creator version: {0} or higher.", ourVersion);
                    info += "Your current PDF Creator version is: " + version;
                    TaskDialog.Show("Bad Printer", info);
                    return Result.Cancelled;
                }

                List<string> pdfCreatorPrinters = PrinterWrapper.CheckPrinters();
                if (pdfCreatorPrinters.Count == 0)
                {
                    MessageBox.Show("There is no PDFCreator printer found on your PC. Please install PDFCreator free printer");
                    return Result.Cancelled;
                }

                BatchPrintViewModel viewModel = App.Instance.BatchPrintViewModel;
                viewModel.ActiveDocument = commandData.Application.ActiveUIDocument.Document;
                SelectionMonitorExEventHandler selectionMonitorExEventHandler = new SelectionMonitorExEventHandler();
                BatchPrintExEventHandler batchPrintExEventHandler = new BatchPrintExEventHandler(pdfCreatorPrinters, viewModel);

                viewModel.SelectionChangedSubscriber = ExternalEvent.Create(selectionMonitorExEventHandler);
                viewModel.PrintingEvent = ExternalEvent.Create(batchPrintExEventHandler);
                PrintSelectionWindow window = new PrintSelectionWindow(viewModel);
                window.DisplayWindow();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
                return Result.Cancelled;
            }

            //List<ViewSheet> viewSheets = new FilteredElementCollector(commandData.Application.ActiveUIDocument.Document).OfClass(typeof(ViewSheet)).Select(e => e as ViewSheet).ToList();
            //BatchPrintViewModel batchPrintViewModel = BatchPrintViewModel.Initialize(viewSheets);

            //IExternalEventHandler batchPrintEvHandler = new BatchPrintExEventHandler(pdfCreatorPrinters, batchPrintViewModel);
            //ExternalEvent exEvent = ExternalEvent.Create(batchPrintEvHandler);
            //SheetSelectionWindow window = new SheetSelectionWindow(batchPrintViewModel, exEvent);          
            //window.Show();
            return Result.Succeeded;        
        }
    }
}
