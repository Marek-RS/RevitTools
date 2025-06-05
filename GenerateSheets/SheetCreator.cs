using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TTTRevitTools.GenerateSheets
{
    public class SheetCreator
    {
        public XYZ BottomLineInsertionPoint { get; set; }
        public XYZ TopLineInsertionPoint { get; set; }
        public ViewSheet ViewSheet { get; set; }
        public void CreateSheetView(Document doc, string sheetName, string sheetNumber)
        {
            ViewSheet = ViewSheet.Create(doc, ElementId.InvalidElementId);
            ViewSheet.Name = sheetName;
            ViewSheet.SheetNumber = sheetNumber;
        }

        public void WriteSheetIdToSchema(GenerateSheetsSchema sheetsSchema, RoomDataGridItem roomDataGridItem)
        {
            sheetsSchema.WriteToSchema(ViewSheet.UniqueId, roomDataGridItem.Room);
            roomDataGridItem.SheetViewStatus = "Sheet view created! <double click to open>";
            roomDataGridItem.SheetView = ViewSheet;
        }

        public void SetSharedParameters(List<ViewSheetParameter> viewSharedParameters)
        {
            foreach (ViewSheetParameter vsp in viewSharedParameters)
            {
                if(vsp.Value != "")
                {
                    try
                    {
                        Parameter p = ViewSheet.get_Parameter(vsp.Parameter.Definition);
                        switch (vsp.StorageType)
                        {
                            case StorageType.None:
                                TaskDialog.Show("Error", "Storage type is uknown!" + Environment.NewLine + "Parameter: " + vsp.Name + ", Value: " + vsp.Value);
                                break;
                            case StorageType.Integer:
                                bool result1 = Int32.TryParse(vsp.Value, out int intToWrite);
                                if(result1)
                                {
                                    p.Set(intToWrite);
                                }
                                else
                                {
                                    TaskDialog.Show("Error", "Can't convert value to Integer!" + Environment.NewLine + "Parameter: " + vsp.Name + ", Value: " + vsp.Value);
                                }
                                break;
                            case StorageType.Double:
                                bool result2 = double.TryParse(vsp.Value, out double doubleToWrite);
                                if (result2)
                                {
                                    p.Set(doubleToWrite);
                                }
                                else
                                {
                                    TaskDialog.Show("Error", "Can't convert value to Integer!" + Environment.NewLine + "Parameter: " + vsp.Name + ", Value: " + vsp.Value);
                                }
                                break;
                            case StorageType.String:
                                p.Set(vsp.Value);
                                break;
                            case StorageType.ElementId:
                                TaskDialog.Show("Error", "Can't handle this storage type!" + Environment.NewLine + "Parameter: " + vsp.Name + ", Value: " + vsp.Value);
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("Error!", "Can't assign value: " + vsp.Value + " to parameter: " + vsp.Name + Environment.NewLine + ex.ToString());
                    }
                }
            }
        }

        public void CreateAndPlaceTitleBlock(Document doc, ViewSheetSettings viewSheetSettings, List<ViewSheetParameter> titleBlockParameters, Transaction tx)
        {
            List<ElementId> viewPorts = ViewSheet.GetAllViewports().ToList();
            XYZ min = new XYZ(0, 0, 0);
            XYZ max = new XYZ(0, 0, 0);
            foreach (ElementId viewPortId in viewPorts)
            {
                Viewport viewport = doc.GetElement(viewPortId) as Viewport;
                Outline outline = viewport.GetBoxOutline();
                if (outline.MinimumPoint.X < min.X)
                {
                    min = new XYZ(outline.MinimumPoint.X, min.Y, min.Z);
                }
                if (outline.MinimumPoint.Y < min.Y)
                {
                    min = new XYZ(min.X, outline.MinimumPoint.Y, min.Z);
                }

                if (outline.MaximumPoint.X > max.X)
                {
                    max = new XYZ(outline.MaximumPoint.X, max.Y, max.Z);
                }
                if (outline.MaximumPoint.Y > max.Y)
                {
                    max = new XYZ(max.X, outline.MaximumPoint.Y, max.Z);
                }
            }
#if DEBUG2020 || RELEASE2020
            double totalHeight = max.Y - min.Y + UnitUtils.ConvertToInternalUnits(viewSheetSettings.TopOffset + viewSheetSettings.BottomOffset, DisplayUnitType.DUT_MILLIMETERS);
            double totalWidth = max.X - min.X + UnitUtils.ConvertToInternalUnits(viewSheetSettings.LeftOffset + viewSheetSettings.RightOffset, DisplayUnitType.DUT_MILLIMETERS);
#elif DEBUG2023 || RELEASE2023
            double totalHeight = max.Y - min.Y + UnitUtils.ConvertToInternalUnits(viewSheetSettings.TopOffset + viewSheetSettings.BottomOffset, UnitTypeId.Millimeters);
            double totalWidth = max.X - min.X + UnitUtils.ConvertToInternalUnits(viewSheetSettings.LeftOffset + viewSheetSettings.RightOffset, UnitTypeId.Millimeters);
#endif

            List<double> standardPrintHeights = new List<double>() { 420, 594, 841, 1189 };
#if DEBUG2020 || RELEASE2020
            var list = standardPrintHeights.Where(e => e > UnitUtils.ConvertFromInternalUnits(totalHeight, DisplayUnitType.DUT_MILLIMETERS)).ToList();
#elif DEBUG2023 || RELEASE2023
            var list = standardPrintHeights.Where(e => e > UnitUtils.ConvertFromInternalUnits(totalHeight, UnitTypeId.Millimeters)).ToList();
#endif
            double minPrintHeight = 0;
            if (list.Count != 0)
            {
                minPrintHeight = list.Min();
            }
            if (minPrintHeight == 0) minPrintHeight = standardPrintHeights.Last();

            FamilySymbol fs = null;
            Family titleBlockFamily = new FilteredElementCollector(doc).OfClass(typeof(Family)).Select(e => e as Family).Where(e => e.Id.IntegerValue == viewSheetSettings.TitleBlockFamilyId).FirstOrDefault();
            if(titleBlockFamily == null)
            {
                TaskDialog.Show("Error", "Title block family not found!");
                return;
            }
#if DEBUG2020 || RELEASE2020
            string newTypeName = "TTT_XX_" + minPrintHeight.ToString("F0") + "x" + UnitUtils.ConvertFromInternalUnits(totalWidth, DisplayUnitType.DUT_MILLIMETERS).ToString("F0");
#elif DEBUG2023 || RELEASE2023
            string newTypeName = "TTT_XX_" + minPrintHeight.ToString("F0") + "x" + UnitUtils.ConvertFromInternalUnits(totalWidth, UnitTypeId.Millimeters).ToString("F0");
#endif

            ElementId symbolId = titleBlockFamily.GetFamilySymbolIds().FirstOrDefault();
            List<ElementId> symbolIds = titleBlockFamily.GetFamilySymbolIds().ToList();
            bool symbolExists = false;
            foreach (ElementId id in symbolIds)
            {
                FamilySymbol familySymbol = doc.GetElement(id) as FamilySymbol;
                if(familySymbol.Name.ToUpper() == newTypeName.ToUpper())
                {
                    fs = familySymbol;
                    symbolExists = true;
                }
            }
            if(!symbolExists)
            {
                ElementType elType = doc.GetElement(symbolId) as ElementType;
                ElementType newElementType =  elType.Duplicate(newTypeName);
                FamilySymbol familySymbol = newElementType as FamilySymbol;
                List<string> potentialHNames = new List<string>() { "HÖHE", "HEIGHT" };
                string[] potentialWNames = { "LENGTH", "WIDTH", "LÄNGE", "BREITE" };

                ParameterSet parameters = familySymbol.Parameters;
                bool widthFound = false;
                bool heightFound = false;
                foreach (Parameter p in parameters)
                {
                    if(potentialHNames.Contains(p.Definition.Name.ToUpper()))
                    {
                        heightFound = true;
#if DEBUG2020 || RELEASE2020
                        p.Set(UnitUtils.ConvertToInternalUnits(minPrintHeight, DisplayUnitType.DUT_MILLIMETERS));
#elif DEBUG2023 || RELEASE2023
                        p.Set(UnitUtils.ConvertToInternalUnits(minPrintHeight, UnitTypeId.Millimeters));
#endif
                    }
                    if (potentialWNames.Contains(p.Definition.Name.ToUpper()))
                    {
                        widthFound = true;
                        p.Set(totalWidth);
                    }
                }
                if(heightFound && widthFound)
                {
                    fs = familySymbol;
                }
                else
                {
                    doc.Delete(familySymbol.Id);
                    fs = doc.GetElement(symbolId) as FamilySymbol;
                }
            }


            XYZ insertionPoint = new XYZ(0, 0, 0);
            FamilyInstance fi = doc.Create.NewFamilyInstance(insertionPoint, fs, ViewSheet);
            tx.Commit();
            tx.Start();
            BoundingBoxXYZ box = fi.get_BoundingBox(ViewSheet);
#if DEBUG2020 || RELEASE2020
            double dx = box.Min.X - (min.X - UnitUtils.ConvertToInternalUnits(viewSheetSettings.LeftOffset, DisplayUnitType.DUT_MILLIMETERS));
            double dy = box.Min.Y - (min.Y - UnitUtils.ConvertToInternalUnits(viewSheetSettings.BottomOffset, DisplayUnitType.DUT_MILLIMETERS));
#elif DEBUG2023 || RELEASE2023
            double dx = box.Min.X - (min.X - UnitUtils.ConvertToInternalUnits(viewSheetSettings.LeftOffset, UnitTypeId.Millimeters));
            double dy = box.Min.Y - (min.Y - UnitUtils.ConvertToInternalUnits(viewSheetSettings.BottomOffset, UnitTypeId.Millimeters));
#endif

            ElementTransformUtils.MoveElement(doc, fi.Id, new XYZ(-dx, -dy, 0));
            SetTitleBlockParameters(titleBlockParameters, fi);
        }

        public void FindAndPlaceTitleBlock(Document doc, ViewSheetSettings viewSheetSettings, List<ViewSheetParameter> titleBlockParameters, Transaction tx)
        {
            List<ElementId> viewPorts = ViewSheet.GetAllViewports().ToList();
            XYZ min = new XYZ(0, 0, 0); 
            XYZ max = new XYZ(0, 0, 0);
            foreach (ElementId viewPortId in viewPorts)
            {
                Viewport viewport = doc.GetElement(viewPortId) as Viewport;
                Outline outline = viewport.GetBoxOutline();
                if(outline.MinimumPoint.X < min.X)
                {
                    min = new XYZ(outline.MinimumPoint.X, min.Y, min.Z);
                }
                if (outline.MinimumPoint.Y < min.Y)
                {
                    min = new XYZ(min.X, outline.MinimumPoint.Y, min.Z);
                }

                if (outline.MaximumPoint.X > max.X)
                {
                    max = new XYZ(outline.MaximumPoint.X, max.Y, max.Z);
                }
                if (outline.MaximumPoint.Y > max.Y)
                {
                    max = new XYZ(max.X, outline.MaximumPoint.Y, max.Z);
                }
            }
#if DEBUG2020 || RELEASE2020
            double totalHeight = max.Y - min.Y + UnitUtils.ConvertToInternalUnits(viewSheetSettings.TopOffset + viewSheetSettings.BottomOffset, DisplayUnitType.DUT_MILLIMETERS);
            double totalWidth = max.X - min.X + UnitUtils.ConvertToInternalUnits(viewSheetSettings.LeftOffset + viewSheetSettings.RightOffset, DisplayUnitType.DUT_MILLIMETERS);
#elif DEBUG2023 || RELEASE2023
            double totalHeight = max.Y - min.Y + UnitUtils.ConvertToInternalUnits(viewSheetSettings.TopOffset + viewSheetSettings.BottomOffset, UnitTypeId.Millimeters);
            double totalWidth = max.X - min.X + UnitUtils.ConvertToInternalUnits(viewSheetSettings.LeftOffset + viewSheetSettings.RightOffset, UnitTypeId.Millimeters);
#endif

            List<FamilySymbol> titleBlockTypes = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Select(e => e as FamilySymbol).Where(e => e.Family.Id.IntegerValue == viewSheetSettings.TitleBlockFamilyId).ToList();
            titleBlockTypes = titleBlockTypes.OrderBy(e => GetHeight(e)).OrderBy(e => GetLength(e)).ToList();

            FamilySymbol fs = null;

            List<FamilySymbol> filtered = titleBlockTypes.Where(e => GetHeight(e) > totalHeight).Where(e => GetLength(e) > totalWidth).ToList();
            fs = filtered.FirstOrDefault();

            if (fs == null) fs = titleBlockTypes.Last();

            XYZ insertionPoint = new XYZ(0, 0, 0);
            FamilyInstance fi = doc.Create.NewFamilyInstance(insertionPoint, fs, ViewSheet);
            tx.Commit();
            tx.Start();
            BoundingBoxXYZ box = fi.get_BoundingBox(ViewSheet);
#if DEBUG2020 || RELEASE2020
            double dx = box.Min.X - (min.X - UnitUtils.ConvertToInternalUnits(viewSheetSettings.LeftOffset, DisplayUnitType.DUT_MILLIMETERS));
            double dy = box.Min.Y - (min.Y - UnitUtils.ConvertToInternalUnits(viewSheetSettings.BottomOffset, DisplayUnitType.DUT_MILLIMETERS));
#elif DEBUG2023 || RELEASE2023
            double dx = box.Min.X - (min.X - UnitUtils.ConvertToInternalUnits(viewSheetSettings.LeftOffset, UnitTypeId.Millimeters));
            double dy = box.Min.Y - (min.Y - UnitUtils.ConvertToInternalUnits(viewSheetSettings.BottomOffset, UnitTypeId.Millimeters));
#endif

            ElementTransformUtils.MoveElement(doc, fi.Id, new XYZ(-dx, -dy, 0));
            SetTitleBlockParameters(titleBlockParameters, fi);
        }

        private void SetTitleBlockParameters(List<ViewSheetParameter> titleBlockParameters, FamilyInstance fi)
        {
            foreach (ViewSheetParameter vsp in titleBlockParameters)
            {
                if (vsp.Value != "")
                {
                    try
                    {
                        Parameter p = fi.get_Parameter(vsp.Parameter.Definition);
                        switch (vsp.StorageType)
                        {
                            case StorageType.None:
                                TaskDialog.Show("Error", "Storage type is uknown!" + Environment.NewLine + "Parameter: " + vsp.Name + ", Value: " + vsp.Value);
                                break;
                            case StorageType.Integer:
                                bool result1 = Int32.TryParse(vsp.Value, out int intToWrite);
                                if (result1)
                                {
                                    p.Set(intToWrite);
                                }
                                else
                                {
                                    TaskDialog.Show("Error", "Can't convert value to Integer!" + Environment.NewLine + "Parameter: " + vsp.Name + ", Value: " + vsp.Value);
                                }
                                break;
                            case StorageType.Double:
                                bool result2 = double.TryParse(vsp.Value, out double doubleToWrite);
                                if (result2)
                                {
                                    p.Set(doubleToWrite);
                                }
                                else
                                {
                                    TaskDialog.Show("Error", "Can't convert value to Integer!" + Environment.NewLine + "Parameter: " + vsp.Name + ", Value: " + vsp.Value);
                                }
                                break;
                            case StorageType.String:
                                p.Set(vsp.Value);
                                break;
                            case StorageType.ElementId:
                                TaskDialog.Show("Error", "Can't handle this storage type!" + Environment.NewLine + "Parameter: " + vsp.Name + ", Value: " + vsp.Value);
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("Error!", "Can't assign value: " + vsp.Value + " to parameter: " + vsp.Name + Environment.NewLine + ex.ToString());
                    }
                }
            }
        }
        private double GetHeight(FamilySymbol familySymbol)
        {
            double result = 0;
            List<string> potentialNames = new List<string>(){ "HÖHE", "HEIGHT" };
            ParameterSet parameters = familySymbol.Parameters;
            foreach (Parameter p in parameters)
            {
                if(potentialNames.Any(e => p.Definition.Name.ToUpper().Contains(e)))
                {
                    result = p.AsDouble();
                    break;
                }
            }
            return result;
        }

        private double GetLength(FamilySymbol familySymbol)
        {
            double result = 0;
            string[] potentialNames = { "LENGTH", "WIDTH", "LÄNGE", "BREITE" };
            ParameterSet parameters = familySymbol.Parameters;
            foreach (Parameter p in parameters)
            {
                if (potentialNames.Any(e => p.Definition.Name.ToUpper().Contains(e)))
                {
                    result = p.AsDouble();
                    break;
                }
            }
            return result;
        }

        public void InsertViewsOnSheet(RoomDataGridItem roomDataGridItem, Document doc, ViewSheetSettings viewSheetSettings, ElementType selectedViewPort)
        {
            List<View> views = roomDataGridItem.Views;
            BottomLineInsertionPoint = new XYZ(0, 0, 0);
            double maxViewHeight = 0;
            //create bottom line
            List<ElementId> usedViews = new List<ElementId>();
            View bottomView1 = GetTheView(views, viewSheetSettings.BottomViewLine1);
            if (bottomView1 != null && !usedViews.Contains(bottomView1.Id))
            {
                usedViews.Add(bottomView1.Id);
                Viewport viewPort = Viewport.Create(doc, ViewSheet.Id, bottomView1.Id, BottomLineInsertionPoint);
                if (selectedViewPort != null) viewPort.ChangeTypeId(selectedViewPort.Id);

                BoundingBoxUV outline = bottomView1.Outline;
#if DEBUG2020 || RELEASE2020
                double dx = outline.Max.U - outline.Min.U + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, DisplayUnitType.DUT_MILLIMETERS);
                double dy = outline.Max.V - outline.Min.V + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, DisplayUnitType.DUT_MILLIMETERS);
#elif DEBUG2023 || RELEASE2023
                double dx = outline.Max.U - outline.Min.U + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, UnitTypeId.Millimeters);
                double dy = outline.Max.V - outline.Min.V + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, UnitTypeId.Millimeters);
#endif


                ElementTransformUtils.MoveElement(doc, viewPort.Id, new XYZ(dx / 2, dy / 2, 0));
                BottomLineInsertionPoint = new XYZ(BottomLineInsertionPoint.X + dx, BottomLineInsertionPoint.Y, BottomLineInsertionPoint.Z);
                double currentViewHeight = outline.Max.V - outline.Min.V;
                if (currentViewHeight > maxViewHeight) maxViewHeight = currentViewHeight;
            }

            View bottomView2 = GetTheView(views, viewSheetSettings.BottomViewLine2);
            if (bottomView2 != null && !usedViews.Contains(bottomView2.Id))
            {
                usedViews.Add(bottomView2.Id);
                Viewport viewPort = Viewport.Create(doc, ViewSheet.Id, bottomView2.Id, BottomLineInsertionPoint);
                if (selectedViewPort != null) viewPort.ChangeTypeId(selectedViewPort.Id);

                BoundingBoxUV outline = bottomView2.Outline;
#if DEBUG2020 || RELEASE2020
                double dx = outline.Max.U - outline.Min.U + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, DisplayUnitType.DUT_MILLIMETERS);
                double dy = outline.Max.V - outline.Min.V + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, DisplayUnitType.DUT_MILLIMETERS);
#elif DEBUG2023 || RELEASE2023
                double dx = outline.Max.U - outline.Min.U + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, UnitTypeId.Millimeters);
                double dy = outline.Max.V - outline.Min.V + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, UnitTypeId.Millimeters);
#endif

                ElementTransformUtils.MoveElement(doc, viewPort.Id, new XYZ(dx / 2, dy / 2, 0));
                BottomLineInsertionPoint = new XYZ(BottomLineInsertionPoint.X + dx, BottomLineInsertionPoint.Y, BottomLineInsertionPoint.Z);
                double currentViewHeight = outline.Max.V - outline.Min.V;
                if (currentViewHeight > maxViewHeight) maxViewHeight = currentViewHeight;
            }

            View bottomView3 = GetTheView(views, viewSheetSettings.BottomViewLine3);
            if (bottomView3 != null && !usedViews.Contains(bottomView3.Id))
            {
                usedViews.Add(bottomView3.Id);
                Viewport viewPort = Viewport.Create(doc, ViewSheet.Id, bottomView3.Id, BottomLineInsertionPoint);
                if (selectedViewPort != null) viewPort.ChangeTypeId(selectedViewPort.Id);

                BoundingBoxUV outline = bottomView3.Outline;
#if DEBUG2020 || RELEASE2020
                double dx = outline.Max.U - outline.Min.U + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, DisplayUnitType.DUT_MILLIMETERS);
                double dy = outline.Max.V - outline.Min.V + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, DisplayUnitType.DUT_MILLIMETERS);
#elif DEBUG2023 || RELEASE2023
                double dx = outline.Max.U - outline.Min.U + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, UnitTypeId.Millimeters);
                double dy = outline.Max.V - outline.Min.V + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, UnitTypeId.Millimeters);
#endif

                ElementTransformUtils.MoveElement(doc, viewPort.Id, new XYZ(dx / 2, dy / 2, 0));
                BottomLineInsertionPoint = new XYZ(BottomLineInsertionPoint.X + dx, BottomLineInsertionPoint.Y, BottomLineInsertionPoint.Z);
                double currentViewHeight = outline.Max.V - outline.Min.V;
                if (currentViewHeight > maxViewHeight) maxViewHeight = currentViewHeight;
            }

            View bottomView4 = GetTheView(views, viewSheetSettings.BottomViewLine4);
            if (bottomView4 != null && !usedViews.Contains(bottomView4.Id))
            {
                usedViews.Add(bottomView4.Id);
                Viewport viewPort = Viewport.Create(doc, ViewSheet.Id, bottomView4.Id, BottomLineInsertionPoint);
                if (selectedViewPort != null) viewPort.ChangeTypeId(selectedViewPort.Id);

                BoundingBoxUV outline = bottomView4.Outline;
#if DEBUG2020 || RELEASE2020
                double dx = outline.Max.U - outline.Min.U + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, DisplayUnitType.DUT_MILLIMETERS);
                double dy = outline.Max.V - outline.Min.V + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, DisplayUnitType.DUT_MILLIMETERS);
#elif DEBUG2023 || RELEASE2023
                double dx = outline.Max.U - outline.Min.U + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, UnitTypeId.Millimeters);
                double dy = outline.Max.V - outline.Min.V + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, UnitTypeId.Millimeters);
#endif

                ElementTransformUtils.MoveElement(doc, viewPort.Id, new XYZ(dx / 2, dy / 2, 0));
                BottomLineInsertionPoint = new XYZ(BottomLineInsertionPoint.X + dx, BottomLineInsertionPoint.Y, BottomLineInsertionPoint.Z);
                double currentViewHeight = outline.Max.V - outline.Min.V;
                if (currentViewHeight > maxViewHeight) maxViewHeight = currentViewHeight;
            }

            //create top line
#if DEBUG2020 || RELEASE2020
            TopLineInsertionPoint = new XYZ(0, maxViewHeight + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, DisplayUnitType.DUT_MILLIMETERS), 0);
#elif DEBUG2023 || RELEASE2023
            TopLineInsertionPoint = new XYZ(0, maxViewHeight + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, UnitTypeId.Millimeters), 0);

#endif

            View topView1 = GetTheView(views, viewSheetSettings.TopViewLine1);
            if (topView1 != null && !usedViews.Contains(topView1.Id))
            {
                usedViews.Add(topView1.Id);
                Viewport viewPort = Viewport.Create(doc, ViewSheet.Id, topView1.Id, TopLineInsertionPoint);
                if (selectedViewPort != null) viewPort.ChangeTypeId(selectedViewPort.Id);

                BoundingBoxUV outline = topView1.Outline;
#if DEBUG2020 || RELEASE2020
                double dx = outline.Max.U - outline.Min.U + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, DisplayUnitType.DUT_MILLIMETERS);
                double dy = outline.Max.V - outline.Min.V + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, DisplayUnitType.DUT_MILLIMETERS);
#elif DEBUG2023 || RELEASE2023
                double dx = outline.Max.U - outline.Min.U + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, UnitTypeId.Millimeters);
                double dy = outline.Max.V - outline.Min.V + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, UnitTypeId.Millimeters);
#endif

                ElementTransformUtils.MoveElement(doc, viewPort.Id, new XYZ(dx / 2, dy / 2, 0));
                TopLineInsertionPoint = new XYZ(TopLineInsertionPoint.X + dx, TopLineInsertionPoint.Y, TopLineInsertionPoint.Z);
            }

            View topView2 = GetTheView(views, viewSheetSettings.TopViewLine2);
            if (topView2 != null && !usedViews.Contains(topView2.Id))
            {
                usedViews.Add(topView2.Id);

                Viewport viewPort = Viewport.Create(doc, ViewSheet.Id, topView2.Id, TopLineInsertionPoint);
                if (selectedViewPort != null) viewPort.ChangeTypeId(selectedViewPort.Id);

                BoundingBoxUV outline = topView2.Outline;
#if DEBUG2020 || RELEASE2020
                double dx = outline.Max.U - outline.Min.U + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, DisplayUnitType.DUT_MILLIMETERS);
                double dy = outline.Max.V - outline.Min.V + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, DisplayUnitType.DUT_MILLIMETERS);
#elif DEBUG2023 || RELEASE2023
                double dx = outline.Max.U - outline.Min.U + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, UnitTypeId.Millimeters);
                double dy = outline.Max.V - outline.Min.V + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, UnitTypeId.Millimeters);
#endif

                ElementTransformUtils.MoveElement(doc, viewPort.Id, new XYZ(dx / 2, dy / 2, 0));
                TopLineInsertionPoint = new XYZ(TopLineInsertionPoint.X + dx, TopLineInsertionPoint.Y, TopLineInsertionPoint.Z);
            }

            View topView3 = GetTheView(views, viewSheetSettings.TopViewLine3);
            if (topView3 != null && !usedViews.Contains(topView3.Id))
            {
                usedViews.Add(topView3.Id);
                Viewport viewPort = Viewport.Create(doc, ViewSheet.Id, topView3.Id, TopLineInsertionPoint);
                if (selectedViewPort != null) viewPort.ChangeTypeId(selectedViewPort.Id);
                BoundingBoxUV outline = topView3.Outline;
#if DEBUG2020 || RELEASE2020
                double dx = outline.Max.U - outline.Min.U + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, DisplayUnitType.DUT_MILLIMETERS);
                double dy = outline.Max.V - outline.Min.V + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, DisplayUnitType.DUT_MILLIMETERS);
#elif DEBUG2023 || RELEASE2023
                double dx = outline.Max.U - outline.Min.U + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, UnitTypeId.Millimeters);
                double dy = outline.Max.V - outline.Min.V + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, UnitTypeId.Millimeters);
#endif

                ElementTransformUtils.MoveElement(doc, viewPort.Id, new XYZ(dx / 2, dy / 2, 0));
                TopLineInsertionPoint = new XYZ(TopLineInsertionPoint.X + dx, TopLineInsertionPoint.Y, TopLineInsertionPoint.Z);
            }

            View topView4 = GetTheView(views, viewSheetSettings.TopViewLine4);
            if (topView4 != null && !usedViews.Contains(topView4.Id))
            {
                usedViews.Add(topView4.Id);
                Viewport viewPort = Viewport.Create(doc, ViewSheet.Id, topView4.Id, TopLineInsertionPoint);
                if (selectedViewPort != null) viewPort.ChangeTypeId(selectedViewPort.Id);
                BoundingBoxUV outline = topView4.Outline;
#if DEBUG2020 || RELEASE2020
                double dx = outline.Max.U - outline.Min.U + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, DisplayUnitType.DUT_MILLIMETERS);
                double dy = outline.Max.V - outline.Min.V + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, DisplayUnitType.DUT_MILLIMETERS);
#elif DEBUG2023 || RELEASE2023
                double dx = outline.Max.U - outline.Min.U + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, UnitTypeId.Millimeters);
                double dy = outline.Max.V - outline.Min.V + UnitUtils.ConvertToInternalUnits(viewSheetSettings.ViewportDistance, UnitTypeId.Millimeters);
#endif

                ElementTransformUtils.MoveElement(doc, viewPort.Id, new XYZ(dx / 2, dy / 2, 0));
                TopLineInsertionPoint = new XYZ(TopLineInsertionPoint.X + dx, TopLineInsertionPoint.Y, TopLineInsertionPoint.Z);
            }
            roomDataGridItem.Views.Clear();
        }

        private View GetTheView(List<View> views, ViewPortName viewPortName)
        {
            View result = null;
            List<View> elevations = views.Where(e => e.ViewType == ViewType.Elevation).ToList();
            switch (viewPortName)
            {
                case ViewPortName.None:
                    break;
                case ViewPortName.FloorPlan:
                    result = views.Where(e => e.ViewType == ViewType.FloorPlan).FirstOrDefault();
                    break;
                case ViewPortName.CeilingPlan:
                    result = views.Where(e => e.ViewType == ViewType.CeilingPlan).FirstOrDefault();
                    break;
                case ViewPortName.ThreeD:
                    result = views.Where(e => e.ViewType == ViewType.ThreeD).FirstOrDefault();
                    break;
                case ViewPortName.ElevationNorth:
                    result = elevations.Where(e => e.ViewDirection.IsAlmostEqualTo(new XYZ(0, -1, 0))).FirstOrDefault();
                    break;
                case ViewPortName.ElevationSouth:
                    result = elevations.Where(e => e.ViewDirection.IsAlmostEqualTo(new XYZ(0, 1, 0))).FirstOrDefault();
                    break;
                case ViewPortName.ElevationWest:
                    result = elevations.Where(e => e.ViewDirection.IsAlmostEqualTo(new XYZ(1, 0, 0))).FirstOrDefault();
                    break;
                case ViewPortName.ElevationEast:
                    result = elevations.Where(e => e.ViewDirection.IsAlmostEqualTo(new XYZ(-1, 0, 0))).FirstOrDefault();
                    break;
                default:
                    break;
            }
            return result;
        }

        class FamilyLoadOptions : IFamilyLoadOptions
        {
            public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
            {
                overwriteParameterValues = false;
                return true;
            }

            public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
            {
                source = FamilySource.Project;
                overwriteParameterValues = false;
                return true;
            }
        }
    }
}
