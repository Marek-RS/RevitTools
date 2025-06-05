using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TTTRevitTools.BatchPrint
{
    public class RevitPrinting
    {
        public List<ViewSheet> ViewSheets { get; set; }

        PrintManager _printManager;
        Document _doc;
        List<FamilyInstance> _titleBlocks;
        bool _overrideAccess;

        public RevitPrinting(Document doc, bool overrideAccess)
        {
            _doc = doc;
            _printManager = _doc.PrintManager;
            _overrideAccess = overrideAccess;
        }

        public void ResetPrinter()
        {
            _printManager.SelectNewPrintDriver("Microsoft Print to PDF");
            _printManager.Apply();
            _printManager = _doc.PrintManager;
            using(Transaction tx = new Transaction(_doc, "Resetting printers"))
            {
                tx.Start();
                _printManager.PrintSetup.SaveAs("ResetWorkaround");
                tx.Commit();
                tx.Start();
                List<Element> setupList = new FilteredElementCollector(_doc).OfClass(typeof(PrintSetting)).ToList();
                PrintSetting currentPrintSetting = (PrintSetting)setupList.Where(e => e.Name == "ResetWorkaround").FirstOrDefault();
                _printManager.PrintSetup.CurrentPrintSetting = currentPrintSetting;
                _printManager.PrintSetup.Delete();
                tx.Commit();
            }


            //_printManager.PrintSetup.SaveAs("ResetSettings");
        }

        public void GetTitleBlocks()
        {
            _titleBlocks = new FilteredElementCollector(_doc).OfClass(typeof(FamilyInstance))
                            .Where(e => e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_TitleBlocks)
                                .Select(e => e as FamilyInstance).ToList();
        }

        public void ChangePaperSizeToDefault()
        {
            CustomPrintForm.AddCustomPaperSize("PDFCreator", "Delete_Form", 100, 100, _overrideAccess);
            CustomPrintForm.DeleteCustomForm("PDFCreator", "Delete_Form", _overrideAccess);
        }

        //THE PRINTER COULD NOT HANDLE ADDING SIZES ONE BY ONE
        //public void AddNewSizes()
        //{
        //    foreach (var item in _titleBlocks)
        //    {
        //        double height = UnitUtils.ConvertFromInternalUnits(item.get_Parameter(BuiltInParameter.SHEET_HEIGHT).AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
        //        double width = UnitUtils.ConvertFromInternalUnits(item.get_Parameter(BuiltInParameter.SHEET_WIDTH).AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
        //        string paperSizeName = GetPaperSizeName(width, height);
        //        bool sizeExists = false;
        //        foreach (PaperSize size in _printManager.PaperSizes)
        //        {
        //            if (size.Name == paperSizeName)
        //            {
        //                sizeExists = true;
        //                break;
        //            }
        //        }
        //        if (sizeExists) continue;
        //        CustomPrintForm.AddCustomPaperSize("PDFCreator", paperSizeName, (float)Math.Round(width, 0), (float)Math.Round(height, 0));
        //        //Thread.Sleep(100);
        //    }
        //    CustomPrintForm.AddCustomPaperSize("PDFCreator", "Delete_Form", 100, 100);
        //    CustomPrintForm.DeleteCustomForm("PDFCreator", "Delete_Form");
        //}

        private string GetPaperSizeName(double width, double height)
        {
            return "TTT_" + width.ToString("F0") + "x" + height.ToString("F0");
        }



        public void GetViewSheetSet(List<ViewSheetModel> viewSheetModels)
        {
            ViewSheets = new List<ViewSheet>();
            foreach (ViewSheetModel model in viewSheetModels)
            {
                if (model.IsSelected) ViewSheets.Add(model.ViewSheet);
            }
        }

        public void ExportSheetsToDwgs(string folderName)
        {
            List<ElementId> viewIds = ViewSheets.Select(e => e.Id).ToList();
            DWGExportOptions exportOptions = new DWGExportOptions();

            //todo: set colors, solids and layer options
            
            //var mode1 = ExportColorMode.IndexColors;
            //var mode2 = ExportColorMode.TrueColor;
            var mode3 = ExportColorMode.TrueColorPerView;
            exportOptions.Colors = mode3;

            //var solid1 = SolidGeometry.ACIS;
            var solid2 = SolidGeometry.Polymesh;
            exportOptions.ExportOfSolids = solid2;

            exportOptions.MergedViews = true;
            _doc.Export(folderName, "", viewIds, exportOptions);
            string[] pcpFiles = Directory.GetFiles(folderName, "*.pcp", SearchOption.TopDirectoryOnly);
            string[] dwgFiles = Directory.GetFiles(folderName, "*.dwg", SearchOption.TopDirectoryOnly);
            foreach (string f in pcpFiles) File.Delete(f);
            foreach (string f in dwgFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(f);
                string checkFileName = fileName.Replace("-Sheet - ", " - Sheet - ");
                checkFileName = checkFileName.Replace("-Plan - ", " - Sheet - ");
                if (App.Instance.BatchPrintViewModel.PrintNameDictionary.ContainsKey(checkFileName))
                {
                    try
                    {
                        string newName = App.Instance.BatchPrintViewModel.PrintNameDictionary[checkFileName];
                        string incName = newName;
                        int index = 0;
                        while (File.Exists(Path.Combine(folderName, incName) + ".dwg"))
                        {
                            index++;
                            incName = string.Format(newName + "({0})", index);
                        }
                        if (index == 0)
                        {
                            File.Move(Path.Combine(folderName, fileName) + ".dwg", Path.Combine(folderName, newName.Replace("[", "_DWG[")) + ".dwg");
                        }
                        else
                        {
                            File.Move(Path.Combine(folderName, fileName) + ".dwg", Path.Combine(folderName, incName.Replace("[", "_DWG[")) + ".dwg");
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                //file_prefix + "-Sheet - "
            }
        }

        public void PrintSingleSheetToPdf(ViewSheet viewSheet, string printerName)
        {
             List<FamilyInstance> titleBlocks =_titleBlocks.Where(e => e.OwnerViewId.IntegerValue == viewSheet.Id.IntegerValue).ToList();

            FamilyInstance item = null;

            double sum = 0;

            if (titleBlocks.Count > 0)
            {
                foreach (FamilyInstance titleBlock in titleBlocks)
                {
                    double tH = UnitUtils.ConvertFromInternalUnits(titleBlock.get_Parameter(BuiltInParameter.SHEET_HEIGHT).AsDouble(), UnitTypeId.Millimeters);
                    double tW = UnitUtils.ConvertFromInternalUnits(titleBlock.get_Parameter(BuiltInParameter.SHEET_WIDTH).AsDouble(), UnitTypeId.Millimeters);
                    if(tH + tW > sum)
                    {
                        sum = tH + tW;
                        item = titleBlock;
                    }
                }
            }

            if (item == null) return;
#if DEBUG2020 || RELEASE2020
            double height = UnitUtils.ConvertFromInternalUnits(item.get_Parameter(BuiltInParameter.SHEET_HEIGHT).AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
            double width = UnitUtils.ConvertFromInternalUnits(item.get_Parameter(BuiltInParameter.SHEET_WIDTH).AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
#elif DEBUG2023 || RELEASE2023
            double height = UnitUtils.ConvertFromInternalUnits(item.get_Parameter(BuiltInParameter.SHEET_HEIGHT).AsDouble(), UnitTypeId.Millimeters);
            double width = UnitUtils.ConvertFromInternalUnits(item.get_Parameter(BuiltInParameter.SHEET_WIDTH).AsDouble(), UnitTypeId.Millimeters);
#endif
            string paperSizeName = GetPaperSizeName(width, height);
            CustomPrintForm.AddCustomPaperSize(printerName, paperSizeName, (float)Math.Round(width, 0), (float)Math.Round(height, 0), _overrideAccess);
            _printManager.SelectNewPrintDriver(printerName);
            _printManager.Apply();
            _printManager = _doc.PrintManager;
            using (Transaction tx = new Transaction(_doc, "Modifying print settings"))
            {
                _printManager.PrintRange = PrintRange.Select;
                _printManager.Apply();
                ViewSet viewSet = new ViewSet();
                viewSet.Insert(viewSheet);
                ViewSheetSetting viewSheetSetting = _printManager.ViewSheetSetting;
                viewSheetSetting.CurrentViewSheetSet.Views = viewSet;
                List<string> viewSheetSettings = new FilteredElementCollector(_doc).OfClass(typeof(ViewSheetSet)).Select(e => e.Name).ToList();
                string name = "CurrentPrint";
                string nameplus = name.ToString();
                int increment = 1;
                while (viewSheetSettings.Contains(nameplus))
                {
                    nameplus = name + string.Format("({0})", increment);
                    increment++;
                }
                tx.Start();
                bool test = viewSheetSetting.SaveAs(nameplus);             
                tx.Commit();
                //_printManager.SelectNewPrintDriver(printerName);
                //_printManager.Apply();
                //_printManager = _doc.PrintManager;
                _printManager.PrintToFile = true;
                //_printManager.PrintToFileName = @"C:\" + viewSheet.Name + ".pdf";
                _printManager.Apply();

                PrintSetup printSetup = _printManager.PrintSetup;
                printSetup.CurrentPrintSetting.PrintParameters.PaperSize = GetPaperSize(paperSizeName);
                printSetup.CurrentPrintSetting.PrintParameters.ZoomType = ZoomType.Zoom;
                printSetup.CurrentPrintSetting.PrintParameters.Zoom = 100;
                printSetup.CurrentPrintSetting.PrintParameters.PageOrientation = PageOrientationType.Portrait;
                printSetup.CurrentPrintSetting.PrintParameters.PaperPlacement = PaperPlacementType.Center;
                List<string> printSetupNames = new FilteredElementCollector(_doc).OfClass(typeof(PrintSetting)).Select(e => e as PrintSetting).Select(e => e.Name).ToList();
                string setupName = "CurrentPrintSetting";
                string setupNamePlus = setupName.ToString();
                int setupDuplicateInc = 1;
                while (printSetupNames.Contains(setupNamePlus))
                {
                    setupNamePlus = setupName + string.Format("({0})", setupDuplicateInc);
                    setupDuplicateInc++;
                }
                tx.Start();
                printSetup.SaveAs(setupNamePlus); //need to save, otherwise it wont work :(
                List<Element> setupList = new FilteredElementCollector(_doc).OfClass(typeof(PrintSetting)).ToList();
                PrintSetting currentPrintSetting = (PrintSetting)setupList.Where(e => e.Name == setupNamePlus).FirstOrDefault();
                printSetup.CurrentPrintSetting = currentPrintSetting;
                _printManager.Apply();
                tx.Commit();

                _printManager.SubmitPrint();

                tx.Start();
                viewSheetSetting.Delete();
                printSetup.Delete();
                tx.Commit();
            }
        }

        private PaperSize GetPaperSize(string name)
        {
            PaperSize result = null;
            foreach (PaperSize size in _printManager.PaperSizes)
            {
                if(size.Name == name)
                {
                    result = size;
                    break;
                }
            }
            return result;
        }
    }
}
