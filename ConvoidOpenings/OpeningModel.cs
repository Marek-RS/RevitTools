using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace TTTRevitTools.ConvoidOpenings
{
    public class OpeningModel
    {
        public string ElementId { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string DateCreated { get; set; }
        public string LastModified { get; set; }
        public string BoundingBox { get; set; }
        public string Coordinates { get; set; }
        public string Type { get; set; }
        public string Direction { get; set; }
        public string Size { get; set; }
        public List<int> HostElements { get; set; }
        public List<int> ReferenceElements { get; set; }
        public string BlTopCoordinates { get; set; }
        public string BlBottomCoordinates { get; set; }        

        public XYZ LocationPoint { get; set; }


        private Element _element { get; set; }

        private Solid _solid;

        private string _relevantLevel;
        private string _discipline;
        private string _nearestGrids;

        private OpeningModel() { }

        public static OpeningModel Initialize(Element element) 
        { 
            OpeningModel model = new OpeningModel();
            model._element = element;
            model.ElementId = element.Id.IntegerValue.ToString();
            //model.GetLocationPoint();
            return model;
        }

        public void GetReferenceElementsData(List<LinkedModelData> linkedModels, List<HostReferenceLookup> hostReferenceLookups)
        {
            FamilyInstance fi = _element as FamilyInstance;
            if (fi != null)
            {
                Parameter referenceElements = null;

                foreach (var lookups in hostReferenceLookups)
                {
                    string pName = lookups.ReferenceLookup;
                    referenceElements = fi.LookupParameter(pName);
                    if (referenceElements != null && !string.IsNullOrEmpty(referenceElements.AsString())) break;
                }


                if (!String.IsNullOrEmpty(referenceElements?.AsString()))
                {
                    ReferenceElements = referenceElements.AsString().Split(new char[] { '/', ',', ';' }).Select(e => Int32.Parse(e)).ToList();
                    string refEl = "351d5915-256b-4b05-ade7-971864bf77f3";
                    SetParameter(refEl, referenceElements.AsString());
                    SetParameter("01c9b9c1-019d-4052-a54a-1a3be6058521", String.Join(",", ReferenceElements)); //TTT - MEP Element Id

                    List<string> mepElementCategory = new List<string>(); //6aefa1d1-642a-4c5e-9b4a-bf0a44187f79
                    List<string> mepElementSourceFile = new List<string>(); //2848d97e-99fe-47da-98b0-ee041eb33a21
                    List<string> mepElementWorkset = new List<string>(); //87e942d2-3c95-435a-8a87-9f6864f86f73
                    List<string> mepElementSystem = new List<string>(); //5a28e083-f6a2-4620-9f20-67b7fe2bb976
                    List<string> mepSystemAbb = new List<string>(); //9cac14a4-de0a-48e8-ac01-2d408234b5b0
                    List<string> mepDiscipline = new List<string>(); //fc977b48-f0b7-47c3-8cb0-11747978d80f
                    List<string> mepType = new List<string>(); //51b64c76-a4d6-457c-9780-6db53e6ff794
                    List<string> mepSize = new List<string>(); //44058fab-07c3-4953-b085-beb79ad3c8e4
                    List<string> mepSummaryInfo = new List<string>(); //6c52d032-8084-4e14-8d66-44487f78e819
                    foreach (int refElement in ReferenceElements)
                    {
                        string singleSummaryInfo = "1 x ";
                        bool hasSizeAndSystem = true;
                        foreach (LinkedModelData lmd in linkedModels.Where(e => e.Status == LinkModelStatus.LoadedInRevit || e.Status == LinkModelStatus.LocalUpToDate || e.Status == LinkModelStatus.LocalUpdateAvailable))
                        {
                            LinkedModelElement element = lmd.ModelElements.Where(e => e.Id == refElement).FirstOrDefault();
                            if (element != null)
                            {
                                mepElementCategory.Add(element.Properties["Category"]);
                                mepElementSourceFile.Add(lmd.Name);
                                mepElementWorkset.Add(element.Properties["Workset"]);
                                if (element.Properties.ContainsKey("System Type"))
                                {
                                    mepElementSystem.Add(element.Properties["System Type"]);
                                }
                                else
                                {
                                    if (element.Properties.ContainsKey("System Classification")) mepElementSystem.Add(element.Properties["System Classification"]);
                                }
                                if (element.Properties.ContainsKey("Size"))
                                {
                                    mepSize.Add(element.Properties["Size"]);
                                    singleSummaryInfo += element.Properties["Size"] + " ";
                                }
                                else
                                {
                                    singleSummaryInfo += "NA ";
                                    hasSizeAndSystem = false;
                                }
                                if (element.Properties.ContainsKey("System Abbreviation"))
                                {
                                    mepSystemAbb.Add(element.Properties["System Abbreviation"]);
                                    singleSummaryInfo += element.Properties["System Abbreviation"];
                                }
                                else
                                {
                                    singleSummaryInfo += "NA";
                                    hasSizeAndSystem = false;
                                }
                                if (element.Properties.ContainsKey("Type")) mepType.Add(element.Properties["Type"]);
                                if(hasSizeAndSystem) mepSummaryInfo.Add(singleSummaryInfo);
                                mepDiscipline.Add(lmd.Discipline);
                            }
                        }
                    }

                    _discipline = String.Join(", ", mepDiscipline.Distinct());
                    SetParameter("6aefa1d1-642a-4c5e-9b4a-bf0a44187f79", String.Join(", ", mepElementCategory));
                    SetParameter("2848d97e-99fe-47da-98b0-ee041eb33a21", String.Join(", ", mepElementSourceFile.Distinct()));
                    SetParameter("87e942d2-3c95-435a-8a87-9f6864f86f73", String.Join(", ", mepElementWorkset.Distinct()));
                    SetParameter("5a28e083-f6a2-4620-9f20-67b7fe2bb976", String.Join(", ", mepElementSystem));
                    SetParameter("9cac14a4-de0a-48e8-ac01-2d408234b5b0", String.Join(", ", mepSystemAbb));
                    SetParameter("fc977b48-f0b7-47c3-8cb0-11747978d80f", _discipline);
                    SetParameter("51b64c76-a4d6-457c-9780-6db53e6ff794", String.Join(", ", mepType));
                    SetParameter("44058fab-07c3-4953-b085-beb79ad3c8e4", String.Join(", ", mepSize));
                    SetParameter("6c52d032-8084-4e14-8d66-44487f78e819", String.Join(", ", mepSummaryInfo));
                }
            }
        }

        public void GetHostElementsData(List<LinkedModelData> linkedModels, List<HostReferenceLookup> hostReferenceLookups)
        {
            FamilyInstance fi = _element as FamilyInstance;
            if (fi != null)
            {
                Parameter hostElements = null;

                foreach (var lookups in hostReferenceLookups)
                {
                    string pName = lookups.HostLookup;
                    hostElements = fi.LookupParameter(pName);
                    if (hostElements != null) break;
                }

                if (!String.IsNullOrEmpty(hostElements?.AsString()))
                {
                    HostElements = hostElements.AsString().Split(new char[] { '/', ',', ';' }).Select(e => Int32.Parse(e)).ToList();
                    string hostEl = "d0e34037-9172-4bb4-a9da-b6bababfc3d8";
                    SetParameter(hostEl, hostElements.AsString());
                    SetParameter("483ee3f4-1fba-46f3-8c9f-07c406e22e0d", String.Join(",", HostElements)); //TTT - Building Element Id
                    List<string> buildingElementCategory = new List<string>(); //e20e771b-da9a-4070-945a-f581b337775c
                    List<string> buildingElementSourceFile = new List<string>(); //a88619d6-7faa-4fa0-9b7e-5c64d26945b5
                    List<string> buildingElementType = new List<string>(); //94d26f96-0ddc-465c-b0f2-8d1aa3c61151
                    foreach (int refElement in HostElements)
                    {
                        foreach (LinkedModelData lmd in linkedModels.Where(e => e.Status == LinkModelStatus.LoadedInRevit || e.Status == LinkModelStatus.LocalUpToDate || e.Status == LinkModelStatus.LocalUpdateAvailable))
                        {
                            LinkedModelElement element = lmd.ModelElements.Where(e => e.Id == refElement).FirstOrDefault();
                            if (element != null)
                            {
                                buildingElementCategory.Add(element.Properties["Category"]);
                                buildingElementSourceFile.Add(lmd.Name);
                                buildingElementType.Add(element.Properties["Type"]);
                            }
                        }
                    }
                    SetParameter("e20e771b-da9a-4070-945a-f581b337775c", String.Join(",", buildingElementCategory.Distinct()));
                    SetParameter("a88619d6-7faa-4fa0-9b7e-5c64d26945b5", String.Join(",", buildingElementSourceFile.Distinct()));
                    SetParameter("94d26f96-0ddc-465c-b0f2-8d1aa3c61151", String.Join(",", buildingElementType.Distinct()));
                }
            }
        }

        public void SetNewOpeningParameters()
        {
            string username = App.UIApp.Application.Username;
            string dateTimeNow = DateTime.Now.ToString("dd-MM-yy hh:mm:ss");
            string bbox = GetSolidBoundingBox();

            string timeCreated = "b598cfeb-b116-4430-af9a-105292b429ad";
            string lastModified = "2c3509dc-07d4-4771-9b39-f733be3ab798";

            string createdBy = "1adef790-4e55-4024-b2ff-239ef7d1dd2a";
            string bboxGuid = "e0f6445b-7495-4b4b-9bf6-7fdef5e08283";

            DateCreated = SetParameter(timeCreated, dateTimeNow); //only for new created
            LastModified = SetParameter(lastModified, dateTimeNow);
            SetParameter("4b71e1ea-bcac-4061-8542-d7df44a48816", dateTimeNow); //TTT Last updated

            CreatedBy = SetParameter(createdBy, username);
            BoundingBox = SetParameter(bboxGuid, bbox);

            FamilyInstance fi = _element as FamilyInstance;
            if(fi != null)
            {
                SetDirectionAndTypeSpecificParameters(fi);
            }
        }

        public void SetModifiedOpeningParameters()
        {
            string username = App.UIApp.Application.Username;
            string dateTimeNow = DateTime.Now.ToString("dd-MM-yy hh:mm:ss");
            string bbox = GetSolidBoundingBox();

            string lastModified = "2c3509dc-07d4-4771-9b39-f733be3ab798";
            string modifiedBy = "595f651a-9436-42b7-96e7-43b498482bda";
            string bboxGuid = "e0f6445b-7495-4b4b-9bf6-7fdef5e08283";
            
            LastModified = SetParameter(lastModified, dateTimeNow);
            SetParameter("4b71e1ea-bcac-4061-8542-d7df44a48816", dateTimeNow); //TTT Last updated
            ModifiedBy = SetParameter(modifiedBy, username);
            BoundingBox = SetParameter(bboxGuid, bbox);

            FamilyInstance fi = _element as FamilyInstance;
            if (fi != null)
            {
                SetDirectionAndTypeSpecificParameters(fi);
            }
        }

        public void SetLevelsData(Dictionary<string, double> levels)
        {
            //65d3b25a-8ce6-40a4-ac3f-3ef743f659d2 nearest level below
            //c6597d59-0383-41d9-a50c-8299f94ff7c7 relevant level
            double opElevation;
            if(_solid != null)
            {
                BoundingBoxXYZ bbox = _solid.GetBoundingBox();
                XYZ min = bbox.Transform.OfPoint(bbox.Min);
                opElevation = min.Z;
            }
            else
            {
                opElevation = LocationPoint.Z;
            }
            string relevantLevel = levels.OrderBy(e => Math.Abs(opElevation - e.Value)).Select(e => e.Key).FirstOrDefault();
            var levelBelow = levels.Where(e => (opElevation - e.Value) >= 0).OrderBy(e => opElevation - e.Value).FirstOrDefault();
            string levelBelowName = levelBelow.Key;
            double distanceTolevelBelow = UnitUtils.ConvertFromInternalUnits(opElevation - levelBelow.Value, UnitTypeId.Millimeters);
            if(levelBelowName != null)
            {
                SetParameter("65d3b25a-8ce6-40a4-ac3f-3ef743f659d2", levelBelowName);
                //c36f602e - 7220 - 4c63 - ae05 - 933cb59334f7 Elevation from nearest level below TODO: to bottom edge
                Parameter p = _element.get_Parameter(new Guid("c36f602e-7220-4c63-ae05-933cb59334f7"));
                p?.Set(distanceTolevelBelow);

            }
            if(relevantLevel != null)
            {
                _relevantLevel = SetParameter("c6597d59-0383-41d9-a50c-8299f94ff7c7", relevantLevel);
                Parameter pElevFromZero = _element.get_Parameter(new Guid("bc098255-17d7-44f7-9305-0a210ec07909"));
                pElevFromZero?.Set(UnitUtils.ConvertFromInternalUnits(opElevation - ConvoidViewModel.DistanceToProjectBase, UnitTypeId.Millimeters));
            }
        }

        public string SetUniqueIdentifier(List<string> uniqueIdentifiers)
        {
            //f0261f82-a41f-4571-ac69-73660a6ede5f
            Parameter p = _element.get_Parameter(new Guid("f0261f82-a41f-4571-ac69-73660a6ede5f"));
            if (p == null) return null;


            string addedIdentifier = "";
            if (String.IsNullOrEmpty(_relevantLevel) || String.IsNullOrEmpty(_nearestGrids) || String.IsNullOrEmpty(_discipline)) return null;
            string identifier = String.Format("{0}.{1}-{2}-", _relevantLevel, _nearestGrids.Replace(" ", ""), _discipline);
            List<string> usedNumbers = uniqueIdentifiers.Where(e => e.Contains(identifier)).Select(e => e.Replace(identifier, "")).ToList();

            if (!String.IsNullOrEmpty(p.AsString()))
            {
                string currentValue = p.AsString();
                if (currentValue.Contains(identifier)) return null;
            }

            int endCode = 1;
           
            while (usedNumbers.Contains(endCode.ToString("D3")))
            {
                endCode ++;
            }

            addedIdentifier = identifier + endCode.ToString("D3");
            SetParameter("f0261f82-a41f-4571-ac69-73660a6ede5f", addedIdentifier);
            return addedIdentifier;
        }

        public void SetGridsData(List<Grid> horizontalGrids, List<Grid> verticalGrids)
        {

            Grid hGrid = horizontalGrids.OrderBy(e => FindDistance(e, LocationPoint)).FirstOrDefault();
            Grid vGrid = verticalGrids.OrderBy(e => FindDistance(e, LocationPoint)).FirstOrDefault();
            if (hGrid == null || vGrid == null) return;

            SetParameter("5cb42d75-99fd-4d98-9c06-79a23ac81646", hGrid.Name);
            SetParameter("379b6e83-48a3-4678-81ac-6b35176ca510", vGrid.Name);

            _nearestGrids = String.Format("{0} - {1}", hGrid.Name, vGrid.Name);
            SetParameter("71e80743-7892-44e3-9b9f-3b0867ff9e23", _nearestGrids);

            double dY = (hGrid.Curve as Line).Origin.Y - LocationPoint.Y;
            double dX = (vGrid.Curve as Line).Origin.X - LocationPoint.X;

            Parameter horDistance = _element.get_Parameter(new Guid("05aa2249-1ccf-4ac4-83ea-16bb99dd7e58"));
            Parameter verDistance = _element.get_Parameter(new Guid("a6509656-731b-4ed8-a988-714dbcd3269d"));

            Parameter horDirection = _element.get_Parameter(new Guid("854127f8-d502-4eaa-afc4-7a20abef394c"));
            Parameter verDirection = _element.get_Parameter(new Guid("deffde7f-568d-4270-9a07-8edb1857e799"));

            Parameter positionEW = _element.get_Parameter(new Guid("ba63fd02-087d-4972-9f79-db867613b214"));
            Parameter positionNS = _element.get_Parameter(new Guid("ea1f66c3-0b7a-41f5-8db8-56f1f45b88db"));


            if (horDistance == null || verDistance == null) return;
            horDistance.Set(Convert.ToInt32(UnitUtils.ConvertFromInternalUnits(Math.Abs(dX), UnitTypeId.Millimeters)));
            verDistance.Set(Convert.ToInt32(UnitUtils.ConvertFromInternalUnits(Math.Abs(dY), UnitTypeId.Millimeters)));

            if (dY > 0)
            {
                verDirection.Set("W");
                positionEW.Set(String.Format("{0} +{1} mm W", hGrid.Name, Convert.ToInt32(UnitUtils.ConvertFromInternalUnits(Math.Abs(dY), UnitTypeId.Millimeters))));
            }
            else
            {
                verDirection.Set("E");
                positionEW.Set(String.Format("{0} +{1} mm E", hGrid.Name, Convert.ToInt32(UnitUtils.ConvertFromInternalUnits(Math.Abs(dY), UnitTypeId.Millimeters))));
            }
            if (dX > 0)
            {
                horDirection.Set("N");
                positionNS.Set(String.Format("{0} +{1} mm N", vGrid.Name, Convert.ToInt32(UnitUtils.ConvertFromInternalUnits(Math.Abs(dX), UnitTypeId.Millimeters))));
            }
            else
            {
                horDirection.Set("S");
                positionNS.Set(String.Format("{0} +{1} mm S", vGrid.Name, Convert.ToInt32(UnitUtils.ConvertFromInternalUnits(Math.Abs(dX), UnitTypeId.Millimeters))));
            }
        }

        private double FindDistance(Grid grid, XYZ locationPoint)
        {
            Line gridLine = grid.Curve as Line;
            XYZ direction = gridLine.Direction;
            XYZ lineOrigin = gridLine.Origin;
            XYZ start = gridLine.Tessellate()[0];
            XYZ end = gridLine.Tessellate()[1];
            Line gridBound = Line.CreateBound(start, end);
            return gridBound.Distance(locationPoint);
        }

        public void GetLocationPoint()
        {
            if(_solid != null)
            {
                XYZ center = _solid.ComputeCentroid();
                LocationPoint = center;
            }
            else
            {
                LocationPoint lp = _element.Location as LocationPoint;
                LocationPoint = lp.Point;

            }
            XYZ point = new XYZ(UnitUtils.ConvertFromInternalUnits(LocationPoint.X, UnitTypeId.Meters),
                        UnitUtils.ConvertFromInternalUnits(LocationPoint.Y, UnitTypeId.Meters),
                            UnitUtils.ConvertFromInternalUnits(LocationPoint.Z, UnitTypeId.Meters));
            string locationPoint = String.Format("{0} {1} {2}", point.X.ToString("F4"), point.Y.ToString("F4"), point.Z.ToString("F4"));
            SetParameter("7c5efcb5-44f3-4cb7-8da0-8e79769d828b", locationPoint); //TTT - Intersection Location
        }

        private void SetDirectionAndTypeSpecificParameters(FamilyInstance fi)
        {
            string opCoordinates = "65175132-b1f1-463e-97f0-e59b552c4684";
            string opDirection = "f4c6686f-f1e6-4606-9867-a0fd13ce7db6";
            string opType = "58518ccf-2ed6-46cd-b03e-56bd2397ddd4";
            string fName = fi.Symbol.Family.Name;
            string direction = string.Format(" ({0},{1},{2})", fi.FacingOrientation.X, fi.FacingOrientation.Y, fi.FacingOrientation.Z);
            string coords = "";

            switch (fName)
            {
                case "CC Horizontal Opening Circular":
                    Direction = SetParameter(opDirection, "Horizontal " + direction);
                    SetParameter("baf87c8b-f4a5-428c-8eab-2b0f34033df3", direction); //TTT direction
                    SetParameter("7ea3a9c8-442e-43ff-8811-634cd93a73a2", "Horizontal"); //TTT orientation
                    Type = SetParameter(opType, "Round");
                    SetRoundSize(fi);
                    coords = GetFormattedString(GetRoundCoordinates(false));
                    Coordinates = SetParameter(opCoordinates, coords);
                    break;
                case "CC Horizontal Opening Rectangle":
                    Direction = SetParameter(opDirection, "Horizontal " + direction);
                    SetParameter("baf87c8b-f4a5-428c-8eab-2b0f34033df3", direction); //TTT direction
                    SetParameter("7ea3a9c8-442e-43ff-8811-634cd93a73a2", "Horizontal"); //TTT orientation
                    Type = SetParameter(opType, "Rectangular");
                    SetRectangularSizeHorizontal(fi);
                    coords = GetFormattedString(GetRectangleCoordinates(false));
                    Coordinates = SetParameter(opCoordinates, coords);
                    break;
                case "CC Vertical Opening Circular":
                    Direction = SetParameter(opDirection, "Vertical");
                    SetParameter("baf87c8b-f4a5-428c-8eab-2b0f34033df3", "(0,0,-1)"); //TTT direction
                    SetParameter("7ea3a9c8-442e-43ff-8811-634cd93a73a2", "Vertical"); //TTT orientation
                    Type = SetParameter(opType, "Round");
                    SetRoundSize(fi);
                    coords = GetFormattedString(GetRoundCoordinates(true));
                    Coordinates = SetParameter(opCoordinates, coords);
                    break;
                case "CC Vertical Opening Rectangle":
                    Direction = SetParameter(opDirection, "Vertical");
                    SetParameter("baf87c8b-f4a5-428c-8eab-2b0f34033df3", "(0,0,-1)"); //TTT direction
                    SetParameter("7ea3a9c8-442e-43ff-8811-634cd93a73a2", "Vertical"); //TTT orientation
                    Type = SetParameter(opType, "Rectangular");
                    SetRectangularSizeVertical(fi);
                    coords = GetFormattedString(GetRectangleCoordinates(true));
                    Coordinates = SetParameter(opCoordinates, coords);
                    break;
                default:
                    if(fName.ToUpper().Contains("VERTICAL"))
                    {
                        Direction = SetParameter(opDirection, "Vertical");
                        SetParameter("baf87c8b-f4a5-428c-8eab-2b0f34033df3", "(0,0,-1)"); //TTT direction
                        SetParameter("7ea3a9c8-442e-43ff-8811-634cd93a73a2", "Vertical"); //TTT orientation
                    }
                    if (fName.ToUpper().Contains("HORIZONTAL"))
                    {
                        Direction = SetParameter(opDirection, "Horizontal " + direction);
                        SetParameter("baf87c8b-f4a5-428c-8eab-2b0f34033df3", direction); //TTT direction
                        SetParameter("7ea3a9c8-442e-43ff-8811-634cd93a73a2", "Horizontal"); //TTT orientation
                    }
                    if (fName.ToUpper().Contains("RECTANGLE"))
                    {
                        Type = SetParameter(opType, "Rectangular");
                        if(fName.ToUpper().Contains("VERTICAL"))
                        {
                            SetRectangularSizeVertical(fi);
                        } else if (fName.ToUpper().Contains("HORIZONTAL"))
                        {
                            SetRectangularSizeHorizontal(fi);
                        }
                        coords = GetFormattedString(GetRectangleCoordinates(true));
                        Coordinates = SetParameter(opCoordinates, coords);
                    }
                    if (fName.ToUpper().Contains("CIRCULAR"))
                    {
                        Type = SetParameter(opType, "Round");
                        SetRoundSize(fi);
                        coords = GetFormattedString(GetRoundCoordinates(true));
                        Coordinates = SetParameter(opCoordinates, coords);
                    }
                    break;
            }
        }

        private List<XYZ> GetRoundCoordinates(bool setBlCoords)
        {
            List<XYZ> coords = new List<XYZ>();
            if (_solid != null)
            {
                foreach (Edge edge in _solid.Edges)
                {
                    Ellipse ellipse = edge.AsCurve() as Ellipse;
                    if (ellipse != null)
                    {
                        if (!coords.Any(e => e.IsAlmostEqualTo(ellipse.Center)))
                        {
                            coords.Add(ellipse.Center);
                        }
                    }
                }
                if(coords.Count == 0)
                {
                    foreach (Edge edge in _solid.Edges)
                    {
                        Arc arc = edge.AsCurve() as Arc;
                        if (arc != null)
                        {
                            if (!coords.Any(e => e.IsAlmostEqualTo(arc.Center)))
                            {
                                coords.Add(arc.Center);
                            }
                        }
                    }
                }
            }
            if(setBlCoords && coords.Count > 0) SetBlRoundCoords(coords);
            return coords;
        }

        private void SetBlRectangularCoords(List<XYZ> coords)
        {
            List<XYZ> orderedCoords = coords.OrderBy(e => e.Z).ToList();
            if (orderedCoords.Count != 8) return;

            //bottom BL coordinates 19151bac-f583-4419-ab54-940a8c997c1d
            //top BL coordinates 301cbf36-67d2-4017-b572-653622865592

            string bottom_bl = "";
            string top_bl = "";

            SingleCoords basisX = ConvoidViewModel.CoordsTransform[0];
            SingleCoords basisY = ConvoidViewModel.CoordsTransform[1];
            SingleCoords basisZ = ConvoidViewModel.CoordsTransform[2];
            SingleCoords vector = ConvoidViewModel.CoordsTransform[3];

            for (int i = 0; i < orderedCoords.Count; i++)
            {
                XYZ point = orderedCoords[i];
                if(i < 4)
                {
                    double x_bottom = -UnitUtils.ConvertFromInternalUnits(point.X, UnitTypeId.Meters); // + 5373.974;
                    double y_bottom = UnitUtils.ConvertFromInternalUnits(point.Y, UnitTypeId.Meters); // + 7564.528;
                    double z_bottom = UnitUtils.ConvertFromInternalUnits(point.Z, UnitTypeId.Meters);

                    double x_bottom_transformed = (basisX.X * x_bottom + basisX.Y * y_bottom + basisX.Z * z_bottom) + vector.X;
                    double y_bottom_transformed = (basisY.X * x_bottom + basisY.Y * y_bottom + basisY.Z * z_bottom) + vector.Y;
                    double z_bottom_transformed = (basisZ.X * x_bottom + basisZ.Y * y_bottom + basisZ.Z * z_bottom) + vector.Z;

                    bottom_bl += String.Format("{0} {1} {2}", x_bottom_transformed.ToString("F4"), y_bottom_transformed.ToString("F4"), z_bottom_transformed.ToString("F4"));
                    bottom_bl += ";" + Environment.NewLine;
                }
                else
                {
                    double x_top = UnitUtils.ConvertFromInternalUnits(point.X, UnitTypeId.Meters); // + 5373.974;
                    double y_top = UnitUtils.ConvertFromInternalUnits(point.Y, UnitTypeId.Meters); // + 7564.528;
                    double z_top = UnitUtils.ConvertFromInternalUnits(point.Z, UnitTypeId.Meters);

                    double x_top_transformed = (basisX.X * x_top + basisX.Y * y_top + basisX.Z * z_top) + vector.X;
                    double y_top_transformed = (basisY.X * x_top + basisY.Y * y_top + basisY.Z * z_top) + vector.Y;
                    double z_top_transformed = (basisZ.X * x_top + basisZ.Y * y_top + basisZ.Z * z_top) + vector.Z;

                    top_bl += String.Format("{0} {1} {2}", x_top_transformed.ToString("F4"), y_top_transformed.ToString("F4"), z_top_transformed.ToString("F4"));
                    top_bl += ";" + Environment.NewLine;
                }
            }
            BlTopCoordinates = top_bl;
            BlBottomCoordinates = bottom_bl;
            SetParameter("301cbf36-67d2-4017-b572-653622865592", top_bl);
            SetParameter("19151bac-f583-4419-ab54-940a8c997c1d", bottom_bl);
        }

        private void SetBlRoundCoords(List<XYZ> blRoundCoords)
        {
            XYZ top = blRoundCoords.OrderBy(e => e.Z).Last();
            XYZ bottom = blRoundCoords.OrderBy(e => e.Z).First();
            //bottom BL coordinates 19151bac-f583-4419-ab54-940a8c997c1d
            //top BL coordinates 301cbf36-67d2-4017-b572-653622865592

            SingleCoords basisX = ConvoidViewModel.CoordsTransform[0];
            SingleCoords basisY = ConvoidViewModel.CoordsTransform[1];
            SingleCoords basisZ = ConvoidViewModel.CoordsTransform[2];
            SingleCoords vector = ConvoidViewModel.CoordsTransform[3];

            double x_top = UnitUtils.ConvertFromInternalUnits(top.X, UnitTypeId.Meters); // + ConvoidViewModel.CoordsTransform[3].X; // + 5373.974;
            double y_top = UnitUtils.ConvertFromInternalUnits(top.Y, UnitTypeId.Meters); // + ConvoidViewModel.CoordsTransform[3].Y; // + 7564.528;
            double z_top = UnitUtils.ConvertFromInternalUnits(top.Z, UnitTypeId.Meters); // + ConvoidViewModel.CoordsTransform[3].Z;

            double x_top_transformed = (basisX.X * x_top + basisX.Y * y_top + basisX.Z * z_top) + vector.X;
            double y_top_transformed = (basisY.X * x_top + basisY.Y * y_top + basisY.Z * z_top) + vector.Y;
            double z_top_transformed = (basisZ.X * x_top + basisZ.Y * y_top + basisZ.Z * z_top) + vector.Z;

            double x_bottom = UnitUtils.ConvertFromInternalUnits(bottom.X, UnitTypeId.Meters);
            double y_bottom = UnitUtils.ConvertFromInternalUnits(bottom.Y, UnitTypeId.Meters);
            double z_bottom = UnitUtils.ConvertFromInternalUnits(bottom.Z, UnitTypeId.Meters);

            double x_bottom_transformed = (basisX.X * x_bottom + basisX.Y * y_bottom + basisX.Z * z_bottom) + vector.X;
            double y_bottom_transformed = (basisY.X * x_bottom + basisY.Y * y_bottom + basisY.Z * z_bottom) + vector.Y;
            double z_bottom_transformed = (basisZ.X * x_bottom + basisZ.Y * y_bottom + basisZ.Z * z_bottom) + vector.Z;

            string bottom_bl = String.Format("{0} {1} {2}", x_bottom_transformed.ToString("F4"), y_bottom_transformed.ToString("F4"), z_bottom_transformed.ToString("F4"));
            string top_bl = String.Format("{0} {1} {2}", x_top_transformed.ToString("F4"), y_top_transformed.ToString("F4"), z_top_transformed.ToString("F4"));

            BlTopCoordinates = top_bl;
            BlBottomCoordinates = bottom_bl;

            SetParameter("301cbf36-67d2-4017-b572-653622865592", top_bl);
            SetParameter("19151bac-f583-4419-ab54-940a8c997c1d", bottom_bl);
        }

        private List<XYZ> GetRectangleCoordinates(bool setBlCoords)
        {
            List<XYZ> coords = new List<XYZ>();
            if (_solid != null)
            {               
                foreach (Edge edge in _solid.Edges)
                {
                    Line line = edge.AsCurve() as Line;
                    if(line != null)
                    {
                        foreach (XYZ xyz in line.Tessellate())
                        {
                            if (!coords.Any(e => e.IsAlmostEqualTo(xyz)))
                            {
                                coords.Add(xyz);
                            }
                        }                    
                    }
                }
            }
            if(setBlCoords) SetBlRectangularCoords(coords);
            return coords;
        }

        private string GetFormattedString(List<XYZ> coordinates)
        {
            // Convert each XYZ point to a formatted string and join them with commas
            IEnumerable<string> formattedPoints = coordinates.Select(point =>
                $"{point.X.ToString("F4")} {point.Y.ToString("F4")} {point.Z.ToString("F4")}");

            // Join the formatted points with commas to create the final string
            string result = string.Join(Environment.NewLine, formattedPoints);

            return result;
        }

        //different for vertical and horizontal WHD, WLD
        private void SetRectangularSizeVertical(FamilyInstance fi)
        {
            Parameter w = fi.LookupParameter("Width");
            Parameter l = fi.LookupParameter("Length");
            Parameter d = fi.LookupParameter("Depth");

            if (w != null && l != null && d != null)
            {
                string size = string.Format("{0}x{1}x{2} (WxLxD)",
                                UnitUtils.ConvertFromInternalUnits(w.AsDouble(), UnitTypeId.Millimeters).ToString("F0"),
                                        UnitUtils.ConvertFromInternalUnits(l.AsDouble(), UnitTypeId.Millimeters).ToString("F0"),
                                            UnitUtils.ConvertFromInternalUnits(d.AsDouble(), UnitTypeId.Millimeters).ToString("F0"));
                string guid = "b711da92-40bd-498d-8fff-99e7e3aa3f6a";
                Size = SetParameter(guid, size);
                SetParameter("6943c8a0-7c60-4716-ba22-ee426e3a9b90", Size); //TTT - Opening Size
                string area = (UnitUtils.ConvertFromInternalUnits(w.AsDouble(), UnitTypeId.Meters) * UnitUtils.ConvertFromInternalUnits(l.AsDouble(), UnitTypeId.Meters)).ToString("F2");
                SetParameter("9299bdd5-f9ad-4a7b-9ede-20b7c30735db", area);
            }
        }

        private void SetRectangularSizeHorizontal(FamilyInstance fi)
        {
            Parameter w = fi.LookupParameter("Width");
            Parameter h = fi.LookupParameter("Height");
            Parameter d = fi.LookupParameter("Depth");

            if(w != null && h != null && d != null)
            {
                string size = string.Format("{0}x{1}x{2} (WxHxD)", 
                                UnitUtils.ConvertFromInternalUnits(w.AsDouble(), UnitTypeId.Millimeters).ToString("F0"), 
                                        UnitUtils.ConvertFromInternalUnits(h.AsDouble(), UnitTypeId.Millimeters).ToString("F0"), 
                                            UnitUtils.ConvertFromInternalUnits(d.AsDouble(), UnitTypeId.Millimeters).ToString("F0"));
                string guid = "b711da92-40bd-498d-8fff-99e7e3aa3f6a";
                Size = SetParameter(guid, size);
                SetParameter("6943c8a0-7c60-4716-ba22-ee426e3a9b90", Size); //TTT - Opening Size
                                                                            //TTT opening area 9299bdd5-f9ad-4a7b-9ede-20b7c30735db
                string area = (UnitUtils.ConvertFromInternalUnits(w.AsDouble(), UnitTypeId.Meters) * UnitUtils.ConvertFromInternalUnits(h.AsDouble(), UnitTypeId.Meters)).ToString("F2");
                SetParameter("9299bdd5-f9ad-4a7b-9ede-20b7c30735db", area);
            }
        }

        private void SetRoundSize(FamilyInstance fi)
        {
            Parameter diameter = fi.LookupParameter("Diameter");
            Parameter depth = fi.LookupParameter("Depth");

            if (diameter != null && depth != null)
            {
                string size = string.Format("{0}x{1} (DxL)", UnitUtils.ConvertFromInternalUnits(diameter.AsDouble(), UnitTypeId.Millimeters).ToString("F0"), UnitUtils.ConvertFromInternalUnits(depth.AsDouble(), UnitTypeId.Millimeters).ToString("F0"));
                string guid = "b711da92-40bd-498d-8fff-99e7e3aa3f6a";
                Size = SetParameter(guid, size);
                SetParameter("6943c8a0-7c60-4716-ba22-ee426e3a9b90", Size); //TTT - Opening Size
                //TTT opening area 9299bdd5-f9ad-4a7b-9ede-20b7c30735db
                string area = (Math.PI * UnitUtils.ConvertFromInternalUnits(diameter.AsDouble(), UnitTypeId.Meters) / 4).ToString("F2");
                SetParameter("9299bdd5-f9ad-4a7b-9ede-20b7c30735db", area);
            }
        }

        private string SetParameter(string guid, string value)
        {
            string result = "N/A";
            Parameter p = _element.get_Parameter(new Guid (guid));
            if (p != null)
            {
                p.Set(value);
                result = value;
            }
            return result;
        }

        private string GetSolidBoundingBox()
        {
            string bbox = "";
            Options opt = new Options();
            opt.IncludeNonVisibleObjects = false;
            GeometryElement geoElem = _element.get_Geometry(opt);

            foreach (GeometryObject geoObj in geoElem)
            {
                GeometryInstance geoInst = geoObj as GeometryInstance;
                if (null != geoInst)
                {
                    GeometryElement instGeoElem = geoInst.GetInstanceGeometry();
                    if (instGeoElem != null)
                    {
                        foreach (GeometryObject o in instGeoElem)
                        {
                            Solid solid = o as Solid;
                            if (solid != null)
                            {
                                if (solid.Volume > 0)
                                {
                                    
                                    if(_solid == null)
                                    {
                                        _solid = solid;
                                        BoundingBoxXYZ xyz = solid.GetBoundingBox();
                                        XYZ min = xyz.Transform.OfPoint(xyz.Min);
                                        XYZ max = xyz.Transform.OfPoint(xyz.Max);
                                        bbox = string.Format("min({0} {1} {2}) max({3} {4} {5})", min.X.ToString("F4"), min.Y.ToString("F4"), min.Z.ToString("F4"), max.X.ToString("F4"), max.Y.ToString("F4"), max.Z.ToString("F4"));
                                    }
                                    else if (solid.Volume > _solid.Volume)
                                    {
                                        _solid = solid;
                                        BoundingBoxXYZ xyz = solid.GetBoundingBox();
                                        XYZ min = xyz.Transform.OfPoint(xyz.Min);
                                        XYZ max = xyz.Transform.OfPoint(xyz.Max);
                                        bbox = string.Format("min({0} {1} {2}) max({3} {4} {5})", min.X.ToString("F4"), min.Y.ToString("F4"), min.Z.ToString("F4"), max.X.ToString("F4"), max.Y.ToString("F4"), max.Z.ToString("F4"));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            return bbox;
        }
    }
}
