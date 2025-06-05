using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CefSharp.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TTTRevitTools.RvtExtEvents;

namespace TTTRevitTools.GridReference
{
    public class GridRefViewModel
    {
        public bool OverwriteParameterValues { get; set; }
        public string ParameterName { get; set; }
        public ObservableCollection<GridRefModel> GridRefModels { get; set; }
        public GridSearchOptions SearchOption { get; set; }
        public ExternalEvent TheEvent { get; set; }
        public GridRefAction TheAction { get; set; }

        string logData = "";

        public GridRefViewModel()
        {
            GridReferenceEvent gridReferenceEvent = new GridReferenceEvent();
            TheEvent = ExternalEvent.Create(gridReferenceEvent);
        }

        public void AddGridsToParameter(Document doc)
        {
            if(string.IsNullOrEmpty(ParameterName))
            {
                TaskDialog.Show("Warning!", "Parameter name is empty!");
                return;
            }

            using (Transaction tx = new Transaction(doc, "Add values to " + ParameterName))
            {
                tx.Start();
                foreach (GridRefModel model in GridRefModels)
                {
                    try
                    {
                        Parameter p = model.FamilyInstance.LookupParameter(ParameterName);
                        if (p != null)
                        {
                            if (!OverwriteParameterValues && !string.IsNullOrEmpty(p.AsString())) continue;
                            p.Set(model.GridReference);
                        }
                        else
                        {
                            model.SystemInfo += "Parameter " + ParameterName + " is null, ";
                        }
                    }
                    catch (Exception)
                    {
                        model.SystemInfo += "Parameter " + ParameterName + " is null or non-text type, ";
                    }

                }
                tx.Commit();
            }
            System.Windows.MessageBox.Show("Grid reference added!", "Info");
        }

        public void InitializeViewModel()
        {
            SearchOption = GridSearchOptions.Combined;
            GridRefModels = new ObservableCollection<GridRefModel>();
            ParameterName = "TTT - Nearest Grid Intersection";
        }

        public void AddSelectedItems(List<FamilyInstance> instances)
        {
            foreach (FamilyInstance fi in instances)
            {
                //if (fi.get_BoundingBox(null) == null) continue;
                if (GridRefModels.Select(e => e.ElementId).Contains(fi.Id.IntegerValue)) continue;
                GridRefModel model = GridRefModel.Initialize(fi);
                GridRefModels.Add(model);
            }
        }

        public void DisplayWindow(IntPtr mainWindowHandle)
        {
            GridReferenceWindow window = new GridReferenceWindow(this, mainWindowHandle);
            window.Show();
        }

        public void FindNearestGrids(Document doc)
        {
            #region Find nearest grids in linked model			
            if (SearchOption != GridSearchOptions.HostModel)
            {
                List<RevitLinkInstance> gridLinkInstances = new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance)).Select(e => e as RevitLinkInstance).Where(e => e.Name.Contains("GRID")).ToList();
                if (gridLinkInstances != null && gridLinkInstances.Count == 1)
                {
                    RevitLinkInstance gridLink = gridLinkInstances.FirstOrDefault();
                    Transform gridLinktransform = gridLink.GetTransform();
                    Document gridLinkDoc = gridLink.GetLinkDocument();
                    if (gridLinkDoc != null)
                    {
                        List<Grid> linkGrids = new FilteredElementCollector(gridLinkDoc).OfCategory(BuiltInCategory.OST_Grids).WhereElementIsNotElementType().Select(e => e as Grid).ToList();

                        List<Grid> horizontalGrids = new List<Grid>();
                        List<Grid> verticalGrids = new List<Grid>();

                        foreach (Grid grid in linkGrids)
                        {
                            double parsingResult = 0;
                            if (double.TryParse(grid.Name, out parsingResult)) continue;
                            if (IsHorizontal(grid) == true)
                            {
                                horizontalGrids.Add(grid);
                            }
                            if (IsHorizontal(grid) == false)
                            {
                                verticalGrids.Add(grid);
                            }
                        }

                        foreach (GridRefModel model in GridRefModels)
                        {
                            var horderedList = horizontalGrids.OrderBy(e => FindDistance(e, model, gridLinktransform)).ToList();
                            var vorderedList = verticalGrids.OrderBy(e => FindDistance(e, model, gridLinktransform)).ToList();
                            if (horderedList.Count == 0 || vorderedList.Count == 0)
                            {
                                logData += string.Format("Id: {0} - nearest grid v or h not found in linked model", model.FamilyInstance.Id.IntegerValue) + Environment.NewLine;
                                continue;
                            }
                            KeyValuePair<Grid, double> hGridDistancePairMin = new KeyValuePair<Grid, double>(horderedList.First(), FindDistance(horderedList.First(), model, gridLinktransform));
                            KeyValuePair<Grid, double> vGridDistancePairMin = new KeyValuePair<Grid, double>(vorderedList.First(), FindDistance(vorderedList.First(), model, gridLinktransform));
                            model.GetNearestGrids(hGridDistancePairMin, vGridDistancePairMin);
                        }
                    }
                    else
                    {
                        logData += "Linked document is null. Try reloading link" + Environment.NewLine;
                    }

                }
                else
                {
                    logData += "Found more than 1 linked grid instance" + Environment.NewLine;
                }
            }
            #endregion
            #region Find nearest grids in host model
            if (SearchOption != GridSearchOptions.LinkedModel)
            {
                List<Grid> hostGrids = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Grids).WhereElementIsNotElementType().Select(e => e as Grid).ToList();
                List<Grid> horizontalGrids = new List<Grid>();
                List<Grid> verticalGrids = new List<Grid>();

                foreach (Grid grid in hostGrids)
                {
                    double parsingResult = 0;
                    if (double.TryParse(grid.Name, out parsingResult)) continue;
                    if (IsHorizontal(grid) == true)
                    {
                        horizontalGrids.Add(grid);
                    }
                    if (IsHorizontal(grid) == false)
                    {
                        verticalGrids.Add(grid);
                    }
                }

                foreach (GridRefModel model in GridRefModels)
                {
                    var horderedList = horizontalGrids.OrderBy(e => FindDistance(e, model)).ToList();
                    var vorderedList = verticalGrids.OrderBy(e => FindDistance(e, model)).ToList();
                    if (horderedList.Count == 0 || vorderedList.Count == 0)
                    {
                        logData += string.Format("Id: {0} - nearest grid v or h not found in host model", model.FamilyInstance.Id.IntegerValue) + Environment.NewLine;
                        continue;
                    }
                    KeyValuePair<Grid, double> hGridDistancePairMin = new KeyValuePair<Grid, double>(horderedList.First(), FindDistance(horderedList.First(), model));
                    KeyValuePair<Grid, double> vGridDistancePairMin = new KeyValuePair<Grid, double>(vorderedList.First(), FindDistance(vorderedList.First(), model));
                    model.GetNearestGrids(hGridDistancePairMin, vGridDistancePairMin);
                }
            }
            #endregion
        }

        private bool? IsHorizontal(Grid grid)
        {
            Line gridLine = grid.Curve as Line;
            if (gridLine == null)
            {
                TaskDialog.Show("Error", "Grid is not a line!" + Environment.NewLine + grid.Id.IntegerValue.ToString());
            }
            XYZ direction = gridLine.Direction;
            XYZ absDirection = new XYZ(Math.Abs(direction.X), Math.Abs(direction.Y), Math.Abs(direction.Z));
            if (absDirection.IsAlmostEqualTo(new XYZ(1, 0, 0)))
            {
                return true;
            }
            if (absDirection.IsAlmostEqualTo(new XYZ(0, 1, 0)))
            {
                return false;
            }
            else
            {
                return null;
            }
        }

        private double FindDistance(Grid grid, GridRefModel gridRefModel)
        {
            XYZ modelPoint = null;
            BoundingBoxXYZ box = gridRefModel.FamilyInstance.get_BoundingBox(null);
            if (gridRefModel.SelectedType == PointType.BboxMiddle && box == null)
            {
                gridRefModel.SystemInfo += "bbox is null, ";
            }
            else
            {
                if(box != null)
                {
                    XYZ middle = (box.Min + box.Max) / 2;
                    modelPoint = middle;
                }

            }
            if (gridRefModel.SelectedType == PointType.LocationPoint || box == null)
            {
                if (box == null) gridRefModel.SelectedType = PointType.LocationPoint;
                var locationPoint = gridRefModel.FamilyInstance.Location as LocationPoint;
                if(locationPoint != null)
                {
                    modelPoint = locationPoint.Point;
                    gridRefModel.SelectedType = PointType.LocationPoint;
                }
                else
                {
                    if(box != null)
                    {
                        XYZ middle = (box.Min + box.Max) / 2;
                        modelPoint = middle;
                        gridRefModel.SelectedType = PointType.BboxMiddle;
                    }
                }
            }

            Line gridLine = grid.Curve as Line;
            XYZ direction = gridLine.Direction;
            XYZ lineOrigin = gridLine.Origin;
            XYZ start = gridLine.Tessellate()[0];
            XYZ end = gridLine.Tessellate()[1];
            Line gridBound = Line.CreateBound(start, end);
            return gridBound.Distance(modelPoint);
        }

        private double FindDistance(Grid grid, GridRefModel gridRefModel, Transform transform)
        {
            XYZ modelPoint = null;
            BoundingBoxXYZ box = gridRefModel.FamilyInstance.get_BoundingBox(null);
            if (gridRefModel.SelectedType == PointType.BboxMiddle && box == null)
            {
                gridRefModel.SystemInfo += "bbox is null, ";
            }
            else
            {
                if (box != null)
                {
                    XYZ middle = (box.Min + box.Max) / 2;
                    modelPoint = middle;
                }

            }
            if (gridRefModel.SelectedType == PointType.LocationPoint || box == null)
            {
                if (box == null) gridRefModel.SelectedType = PointType.LocationPoint;
                var locationPoint = gridRefModel.FamilyInstance.Location as LocationPoint;
                if (locationPoint != null)
                {
                    modelPoint = locationPoint.Point;
                    gridRefModel.SelectedType = PointType.LocationPoint;
                }
                else
                {
                    if (box != null)
                    {
                        XYZ middle = (box.Min + box.Max) / 2;
                        modelPoint = middle;
                        gridRefModel.SelectedType = PointType.BboxMiddle;
                    }
                }
            }
            Line gridLine = grid.Curve as Line;
            XYZ direction = gridLine.Direction;
            XYZ lineOrigin = transform.OfPoint(gridLine.Origin);
            XYZ lineDirection = transform.OfVector(direction);
            XYZ start = transform.OfPoint(gridLine.Tessellate()[0]);
            XYZ end = transform.OfPoint(gridLine.Tessellate()[1]);
            Line transformedGridBound = Line.CreateBound(start, end);
            return transformedGridBound.Distance(modelPoint);
        }
    }
}
