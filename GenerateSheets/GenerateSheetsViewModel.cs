using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.ComponentModel;

namespace TTTRevitTools.GenerateSheets
{
    public class GenerateSheetsViewModel : INotifyPropertyChanged
    {
        public List<ParameterSubstring> ParameterSubstrings { get; set; }
        public List<ParameterFindReplace> FindReplaceParameters { get; set; }        

        private List<View> _planViews;
        public List<View> PlanViews
        {
            get => _planViews;
            set
            {
                if (_planViews != value)
                {
                    _planViews = value;
                    OnPropertyChanged(nameof(PlanViews));
                }
            }
        }
        public GenerateSheetsAction GenerateSheetsAction { get; set; }
        public RoomDataGridItem SelectedRoom { get; set; }
        public ExternalEvent TheEvent { get; set; }
        public bool UseExisitngTitleBlockSizes { get; set; } = false;
        public List<ElementType> ViewPorts { get; set; }
        public ElementType SelectedViewPort { get; set; }
        public int SelectedScale { get; set; }
        public List<string> ViewScales { get; set; }
        public bool OverrideScaleInTemplate { get; set; } = false;

        public ViewSheetSettings ViewSheetSettings { get; set; }
        public List<Family> TitleBlocks { get; set; }

        private List<RoomDataGridItem> _rooms;
        public List<RoomDataGridItem> Rooms
        {
            get => _rooms;
            set
            {
                if (_rooms != value)
                {
                    _rooms = value;
                    OnPropertyChanged(nameof(Rooms));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<ViewTypeDataGridItem> ViewTypes { get; set; }

        public List<RoomDataGridItem> SelectedRooms { get; set; }
        public List<ViewTypeDataGridItem> SelectedViewTypes { get; set; }
        public List<ViewNamingRule> ViewNamingRules { get; set; }
        public List<SelectableParameter> SelectableParameters { get; set; }

        public List<ViewSheetParameter> ViewSheetParameters { get; set; }
        public List<ViewSheetParameter> TitleBlockParameters { get; set; }

        public bool CreateSheets = false;

        Document _doc;

        public GenerateSheetsViewModel(Document doc)
        {
            _doc = doc;
            GetViewSheetSharedParameters();
            //GetTitleBlockSharedParameters();
            CreateViewScalesList();
        }

        public GenerateSheetsViewModel()
        {
            
        }

        public void GetRoomParameters()
        {
            List<string> result = new List<string>();
            Room room = new FilteredElementCollector(_doc).OfClass(typeof(SpatialElement)).Select(e => e as Room).Where(e => e != null).FirstOrDefault();
            if(room == null)
            {
                TaskDialog.Show("Info", "There is no room found in this project!");
                return;
            }
            ParameterSet parameterSet = room.Parameters;
            foreach (Parameter p in parameterSet)
            {
                if(p.StorageType == StorageType.String)
                {
                    result.Add(p.Definition.Name);
                }
            }
            result.Sort();
            foreach (string pName in result)
            {
                SelectableParameter selectableParameter = new SelectableParameter() { Name = pName };
                //selectableParameter.GetIndex(SelectableParameters);
                SelectableParameters.Add(selectableParameter);
            }
            foreach (SelectableParameter parameter in SelectableParameters) parameter.GetIndex(SelectableParameters);
        }

        public void SetViewModel(Document doc)
        {
            _doc = doc;
            GetViewSheetSharedParameters();
            //GetTitleBlockSharedParameters();
            CreateViewScalesList();
            Rooms = new List<RoomDataGridItem>();
            PlanViews = new FilteredElementCollector(_doc).OfClass(typeof(View)).Select(e => e as View).Where(e => e.ViewType == ViewType.FloorPlan || e.ViewType == ViewType.CeilingPlan).ToList();
            PlanViews = PlanViews.OrderBy(e => e.Name).ToList();
            SelectableParameters = new List<SelectableParameter>();
            SelectableParameter viewOrientation = new SelectableParameter() { Name = "ViewOrientation", IsViewOrientation = true};
            SelectableParameter empty = new SelectableParameter() { Name = "Empty"};
            SelectableParameters.Add(empty);
            SelectableParameters.Add(viewOrientation);
            GetRoomParameters();
            ParameterSubstrings = new List<ParameterSubstring>();
            FindReplaceParameters = new List<ParameterFindReplace>();
            ReadParameterModifiers();
        }

        private List<ViewSheetParameterSerialized> GetSerializedParameterValues()
        {
            List<ViewSheetParameterSerialized> result = new List<ViewSheetParameterSerialized>();
            string assemblyDirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string TTTRevitToolsDirectory = Directory.GetParent(assemblyDirPath).FullName;
            string jsonFilePath = Path.Combine(TTTRevitToolsDirectory, "titleblock_sheet_parameters.json");
            if(File.Exists(jsonFilePath))
            {
                try
                {
                    result = JsonConvert.DeserializeObject<List<ViewSheetParameterSerialized>>(File.ReadAllText(jsonFilePath));
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", "Can't deserialize parameter values" + ex.ToString());
                }
            }
            return result;
        }

        private void GetViewSheetSharedParameters()
        {
            ViewSheetParameters = new List<ViewSheetParameter>();
            ViewSheet viewSheet = new FilteredElementCollector(_doc).OfClass(typeof(View)).Select(e => e as View).Where(e => e.ViewType == ViewType.DrawingSheet).Select(e => e as ViewSheet).ToList().FirstOrDefault();
            if (viewSheet == null) return;
            List<Parameter> parameters = viewSheet.GetOrderedParameters().ToList();

            foreach (Parameter p in parameters)
            {
                if (p.IsShared)
                {
                    ViewSheetParameter sp = new ViewSheetParameter();
                    sp.Parameter = p;
                    sp.Name = p.Definition.Name;
                    sp.Value = "";
                    sp.StorageType = p.StorageType;
                    sp.IsShared = p.IsShared;
                    ViewSheetParameters.Add(sp);
                }
                else
                {
                    if(p.StorageType == StorageType.String || p.StorageType == StorageType.Double || p.StorageType == StorageType.Integer)
                    {
                        if ((p.Definition as InternalDefinition).BuiltInParameter == BuiltInParameter.SHEET_NAME || (p.Definition as InternalDefinition).BuiltInParameter == BuiltInParameter.SHEET_NUMBER) continue;
                        ViewSheetParameter sp = new ViewSheetParameter();
                        sp.Parameter = p;
                        sp.Name = p.Definition.Name;
                        sp.Value = "";
                        sp.StorageType = p.StorageType;
                        sp.IsShared = p.IsShared;
                        ViewSheetParameters.Add(sp);
                    }
                }
            }
            var checkList = GetSerializedParameterValues();
            foreach (ViewSheetParameter parameter in ViewSheetParameters)
            {
                var test = checkList.Where(e => e.Name == parameter.Name);
                if (test.ToList().Count == 1)
                {
                    parameter.Value = test.FirstOrDefault().Value;
                }
            }
        }

        public void GetTitleBlockSharedParameters()
        {
            TitleBlockParameters = new List<ViewSheetParameter>();
            FamilyInstance titleBlock = new FilteredElementCollector(_doc).OfClass(typeof(FamilyInstance)).Select(e => e as FamilyInstance).Where(e => e.Symbol.Family.Id.IntegerValue == ViewSheetSettings.TitleBlockFamilyId).FirstOrDefault();
            if (titleBlock == null) return;
            List<Parameter> parameters = titleBlock.GetOrderedParameters().ToList();

            foreach (Parameter p in parameters)
            {
                if (p.IsShared)
                {
                    ViewSheetParameter sp = new ViewSheetParameter();
                    sp.Parameter = p;
                    sp.Name = p.Definition.Name;
                    sp.Value = "";
                    sp.StorageType = p.StorageType;
                    sp.IsShared = p.IsShared;
                    TitleBlockParameters.Add(sp);
                }
                else
                {
                    if (p.StorageType == StorageType.String || p.StorageType == StorageType.Double || p.StorageType == StorageType.Integer)
                    {
                        if (p.StorageType == StorageType.Integer && p.CanBeAssociatedWithGlobalParameters() == false) continue;
                        if ((p.Definition as InternalDefinition).BuiltInParameter == BuiltInParameter.SHEET_NAME || (p.Definition as InternalDefinition).BuiltInParameter == BuiltInParameter.SHEET_NUMBER) continue;
                        ViewSheetParameter sp = new ViewSheetParameter();
                        sp.Parameter = p;
                        sp.Name = p.Definition.Name;
                        sp.Value = "";
                        sp.StorageType = p.StorageType;
                        sp.IsShared = p.IsShared;
                        TitleBlockParameters.Add(sp);
                    }
                }
            }

            var checkList = GetSerializedParameterValues();
            foreach (ViewSheetParameter parameter in TitleBlockParameters)
            {
                var test = checkList.Where(e => e.Name == parameter.Name);
                if (test.ToList().Count == 1)
                {
                    parameter.Value = test.FirstOrDefault().Value;
                }
            }
        }

        public void GetViewports()
        {
            ViewPorts = new FilteredElementCollector(_doc).WhereElementIsElementType().Select(e => e as ElementType).Where(e => e.FamilyName == "Ansichtsfenster" || e.FamilyName == "Viewport").ToList();
        }

        public void CreateSomeSheets()
        {
            if(CreateSheets)
            {
                GenerateSheetsSchema sheetsSchema = GenerateSheetsSchema.Initialize();
                using (Transaction tx = new Transaction(_doc, "CreateSheetViews"))
                {
                    tx.Start();
                    foreach (RoomDataGridItem room in SelectedRooms)
                    {
                        SheetCreator sheetCreator = new SheetCreator();
                        string name = BuildViewName(room.Room, "Sheet Name");
                        string number = BuildViewName(room.Room, "Sheet Number");
                        sheetCreator.CreateSheetView(_doc, name, number);
                        sheetCreator.SetSharedParameters(ViewSheetParameters);
                        sheetCreator.InsertViewsOnSheet(room, _doc, ViewSheetSettings, SelectedViewPort);
                        if(UseExisitngTitleBlockSizes)
                        {
                            sheetCreator.FindAndPlaceTitleBlock(_doc, ViewSheetSettings, TitleBlockParameters, tx);
                        }
                        else
                        {
                            sheetCreator.CreateAndPlaceTitleBlock(_doc, ViewSheetSettings, TitleBlockParameters, tx);
                        }
                        sheetCreator.WriteSheetIdToSchema(sheetsSchema, room);
                        room.IsSelected = false;
                    }
                    tx.Commit();
                }
            }
        }

        public void CreateSomeViews()
        {
            using (Transaction tx = new Transaction(_doc, "Create Views"))
            {
                tx.Start();
                foreach (RoomDataGridItem room in SelectedRooms)
                {
                    
                    foreach (ViewTypeDataGridItem viewType in SelectedViewTypes)
                    {
                        switch (viewType.ViewFamily)
                        {
                            case ViewFamily.ThreeDimensional:
                                CreateThreeD(room, viewType);
                                break;
                            case ViewFamily.FloorPlan:
                                CreateViewPlan(room, viewType);
                                break;
                            case ViewFamily.CeilingPlan:
                                CreateViewPlan(room, viewType);
                                break;
                            case ViewFamily.Elevation:
                                ElevationMarker em = CreateElevationMarker(room.Room, viewType);
                                for (int i = 0; i < 4; i++)
                                {
                                    ViewSection vs = em.CreateElevation(_doc, room.DisplayView.Id, i);
                                    SetElevationCropBox(room, vs, viewType);
                                }
                                break;
                            case ViewFamily.Section:
                                CreateSection(room.Room, viewType);
                                break;
                            default:
                                break;
                        }
                    }

                }
                tx.Commit();
            }
        }

        private void CreateSection(Room room, ViewTypeDataGridItem viewType)
        {
            //section direction enum N,S,W,E
            //section bottom, top, left, right coordinates (calculated from room)
            //section depth (calculated from room)
            //section family type (selected by user)

            BoundingBoxXYZ roomBoxOff = GetRoomBoundingBox(room, ViewSheetSettings);
            double width = roomBoxOff.Max.X - roomBoxOff.Min.X;
            double depth = roomBoxOff.Max.Y - roomBoxOff.Min.Y;
            double height = roomBoxOff.Max.Z - roomBoxOff.Min.Z;

            double offset = UnitUtils.ConvertToInternalUnits(100, UnitTypeId.Millimeters);

            Transform tranformDown = Transform.Identity;
            tranformDown.Origin = XYZ.Zero;
            tranformDown.BasisX = XYZ.BasisX;
            tranformDown.BasisY = XYZ.BasisZ;
            tranformDown.BasisZ = XYZ.BasisX.CrossProduct(XYZ.BasisZ);
            BoundingBoxXYZ sectionBoxDown = new BoundingBoxXYZ();
            sectionBoxDown.Transform = tranformDown;
            sectionBoxDown.Min = new XYZ (roomBoxOff.Min.X, roomBoxOff.Min.Z, depth/2 + offset);
            sectionBoxDown.Max = new XYZ (roomBoxOff.Max.X, roomBoxOff.Max.Z, depth);
            ViewSection viewSectionDown = ViewSection.CreateSection(_doc, viewType.SelectedViewFamilyType.Id, sectionBoxDown);
            viewSectionDown.Name = "Test_Section_Down";

            Transform tranformUp = Transform.Identity;
            tranformUp.Origin = XYZ.Zero;
            tranformUp.BasisX = XYZ.BasisX;
            tranformUp.BasisY = -XYZ.BasisZ;
            tranformUp.BasisZ = XYZ.BasisX.CrossProduct(-XYZ.BasisZ);

            BoundingBoxXYZ sectionBoxUp = new BoundingBoxXYZ();
            sectionBoxUp.Transform = tranformUp;
            sectionBoxUp.Min = new XYZ(roomBoxOff.Min.X, -roomBoxOff.Max.Z, -depth/2 + offset);
            sectionBoxUp.Max = new XYZ(roomBoxOff.Max.X, -roomBoxOff.Min.Z, 0);
            ViewSection viewSectionUp = ViewSection.CreateSection(_doc, viewType.SelectedViewFamilyType.Id, sectionBoxUp);
            viewSectionUp.Name = "Test_Section_Up";

        }

        private ElevationMarker CreateElevationMarker(Room room, ViewTypeDataGridItem viewType)
        {
            BoundingBoxXYZ roomBox = room.get_BoundingBox(null);
            //XYZ midPoint = new XYZ((roomBox.Max.X + roomBox.Min.X) / 2, (roomBox.Max.Y + roomBox.Min.Y) / 2, (roomBox.Max.Z + roomBox.Min.Z) / 2);
            XYZ midPoint = new XYZ((roomBox.Max.X + roomBox.Min.X) / 2, (roomBox.Max.Y + roomBox.Min.Y) / 2, room.Level.Elevation);
            return ElevationMarker.CreateElevationMarker(_doc, viewType.SelectedViewFamilyType.Id, midPoint, 100);
        }

        private void SetElevationCropBox(RoomDataGridItem roomItem, ViewSection viewSection, ViewTypeDataGridItem viewType)
        {
            BoundingBoxXYZ roomBoxOff = GetRoomBoundingBox(roomItem.Room, ViewSheetSettings);
            BoundingBoxXYZ bb = viewSection.CropBox;

            //case back
            if (viewSection.ViewDirection.IsAlmostEqualTo(new XYZ(0, 1, 0)))
            {
                XYZ max = bb.Max;
                max = new XYZ(-roomBoxOff.Min.X, roomBoxOff.Max.Z, max.Z);
                XYZ min = bb.Min;
                min = new XYZ(-roomBoxOff.Max.X, roomBoxOff.Min.Z, min.Z);
                bb.Max = max;
                bb.Min = min;
                viewSection.CropBox = bb;
                viewSection.Name = BuildViewName(roomItem.Room, viewType, "South");
            }

            //case front
            if (viewSection.ViewDirection.IsAlmostEqualTo(new XYZ(0, -1, 0)))
            {
                XYZ max = bb.Max;
                max = new XYZ(roomBoxOff.Max.X, roomBoxOff.Max.Z, max.Z);
                XYZ min = bb.Min;
                min = new XYZ(roomBoxOff.Min.X, roomBoxOff.Min.Z, min.Z);
                bb.Max = max;
                bb.Min = min;
                viewSection.CropBox = bb;
                viewSection.Name = BuildViewName(roomItem.Room, viewType, "North");
            }

            //case left
            if (viewSection.ViewDirection.IsAlmostEqualTo(new XYZ(1, 0, 0)))
            {
                XYZ max = bb.Max;
                max = new XYZ(roomBoxOff.Max.Y, roomBoxOff.Max.Z, max.Z);
                XYZ min = bb.Min;
                min = new XYZ(roomBoxOff.Min.Y, roomBoxOff.Min.Z, min.Z);
                bb.Max = max;
                bb.Min = min;
                viewSection.CropBox = bb;
                viewSection.Name = BuildViewName(roomItem.Room, viewType, "West");
            }

            //case right
            if (viewSection.ViewDirection.IsAlmostEqualTo(new XYZ(-1, 0, 0)))
            {
                XYZ max = bb.Max;
                max = new XYZ(-roomBoxOff.Min.Y, roomBoxOff.Max.Z, max.Z);
                XYZ min = bb.Min;
                min = new XYZ(-roomBoxOff.Max.Y, roomBoxOff.Min.Z, min.Z);
                bb.Max = max;
                bb.Min = min;
                viewSection.CropBox = bb;
                viewSection.Name = BuildViewName(roomItem.Room, viewType, "East");
            }

            SetViewTemplateAndScale(viewSection, viewType.SelectedViewTemplate);
            Parameter p = viewSection.get_Parameter(BuiltInParameter.VIEWER_ANNOTATION_CROP_ACTIVE);
            if (p != null) p.Set(1);
            roomItem.Views.Add(viewSection);
        }

        private void CreateThreeD(RoomDataGridItem roomItem, ViewTypeDataGridItem viewType)
        {
            View3D newThreeDView = View3D.CreateIsometric(_doc, viewType.SelectedViewFamilyType.Id);
            newThreeDView.Name = "New3DView_" + roomItem.Room.Number;
            newThreeDView.SetSectionBox(GetRoomBoundingBox(roomItem.Room, ViewSheetSettings));
            newThreeDView.IsSectionBoxActive = true;
            newThreeDView.CropBoxActive = true;
            AdjustViewCropToSectionBox(newThreeDView);
            newThreeDView.CropBoxVisible = false;
            Parameter p = newThreeDView.get_Parameter(BuiltInParameter.VIEWER_ANNOTATION_CROP_ACTIVE);
            if (p != null) p.Set(1);
            SetViewTemplateAndScale(newThreeDView, viewType.SelectedViewTemplate);
            newThreeDView.Name = BuildViewName(roomItem.Room, viewType, "3D");
            roomItem.Views.Add(newThreeDView);
        }

        private void AdjustViewCropToSectionBox(View3D view)
        {
            if (!view.IsSectionBoxActive)
            {
                return;
            }
            if (!view.CropBoxActive)
            {
                view.CropBoxActive = true;
            }
            BoundingBoxXYZ CropBox = view.CropBox;
            BoundingBoxXYZ SectionBox = view.GetSectionBox();
            Transform T = CropBox.Transform;
            var Corners = BBCorners(SectionBox, T);
            double MinX = Corners.Min(j => j.X);
            double MinY = Corners.Min(j => j.Y);
            double MinZ = Corners.Min(j => j.Z);
            double MaxX = Corners.Max(j => j.X);
            double MaxY = Corners.Max(j => j.Y);
            double MaxZ = Corners.Max(j => j.Z);
            CropBox.Min = new XYZ(MinX, MinY, MinZ);
            CropBox.Max = new XYZ(MaxX, MaxY, MaxZ);
            view.CropBox = CropBox;
        }

        private XYZ[] BBCorners(BoundingBoxXYZ SectionBox, Transform T)
        {
            XYZ sbmn = SectionBox.Min;
            XYZ sbmx = SectionBox.Max;
            XYZ Btm_LL = sbmn; // Lower Left
            var Btm_LR = new XYZ(sbmx.X, sbmn.Y, sbmn.Z); // Lower Right
            var Btm_UL = new XYZ(sbmn.X, sbmx.Y, sbmn.Z); // Upper Left
            var Btm_UR = new XYZ(sbmx.X, sbmx.Y, sbmn.Z); // Upper Right
            XYZ Top_UR = sbmx; // Upper Right
            var Top_UL = new XYZ(sbmn.X, sbmx.Y, sbmx.Z); // Upper Left
            var Top_LR = new XYZ(sbmx.X, sbmn.Y, sbmx.Z); // Lower Right
            var Top_LL = new XYZ(sbmn.X, sbmn.Y, sbmx.Z); // Lower Left
            var Out = new XYZ[8] {
            Btm_LL, Btm_LR, Btm_UL, Btm_UR,
            Top_UR, Top_UL, Top_LR, Top_LL };
            for (int i = 0, loopTo = Out.Length - 1; i <= loopTo; i++)
            {
                // Transform bounding box coords to model coords
                Out[i] = SectionBox.Transform.OfPoint(Out[i]);
                // Transform bounding box coords to view coords
                Out[i] = T.Inverse.OfPoint(Out[i]);
            }
            return Out;
        }

        private void CreateViewScalesList()
        {
            ViewScales = new List<string>()
            { 
                "1 : 1",
                "1 : 2",
                "1 : 5",
                "1 : 10",
                "1 : 20",
                "1 : 25",
                "1 : 50",
                "1 : 100",
                "1 : 200"
            };

        }

        public void SetSelectedScale(string selection)
        {
            switch (selection)
            {
                case "1 : 1":
                    SelectedScale = 1;
                    break;
                case "1 : 2":
                    SelectedScale = 2;
                    break;
                case "1 : 5":
                    SelectedScale = 5;
                    break;
                case "1 : 10":
                    SelectedScale = 10;
                    break;
                case "1 : 20":
                    SelectedScale = 20;
                    break;
                case "1 : 25":
                    SelectedScale = 25;
                    break;
                case "1 : 50":
                    SelectedScale = 50;
                    break;
                case "1 : 100":
                    SelectedScale = 100;
                    break;
                case "1 : 200":
                    SelectedScale = 200;
                    break;
                default:
                    break;
            }

        }

        private void SetViewTemplateAndScale(View newView, View viewTemplate)
        {
            if(viewTemplate == null)
            {
                newView.Scale = SelectedScale;
            }
            else
            {
                List<int> ids = viewTemplate.GetNonControlledTemplateParameterIds().Select(e => e.IntegerValue).ToList();
                int scaleIdInt = (int)BuiltInParameter.VIEW_SCALE;
                if(ids.Contains(scaleIdInt))
                {
                    newView.Scale = SelectedScale;
                }
                else
                {
                    viewTemplate.Scale = SelectedScale;
                }
                newView.ViewTemplateId = viewTemplate.Id;
            }
        }

        private void CreateViewPlan(RoomDataGridItem roomItem, ViewTypeDataGridItem viewType)
        {
            ElementId roomAssociatedId = roomItem.Room.LevelId;
            ViewPlan newViewPlan = ViewPlan.Create(_doc, viewType.SelectedViewFamilyType.Id, roomAssociatedId);
            newViewPlan.Name = "NewViewPlan_" + roomItem.Room.Number;
            newViewPlan.CropBox = GetRoomBoundingBox(roomItem.Room, ViewSheetSettings);
            newViewPlan.CropBoxActive = true;
            Parameter p = newViewPlan.get_Parameter(BuiltInParameter.VIEWER_ANNOTATION_CROP_ACTIVE);
            if(p != null) p.Set(1);
            SetViewTemplateAndScale(newViewPlan, viewType.SelectedViewTemplate);
            string orientationTag = "";
            if (viewType.ViewType == ViewType.FloorPlan) orientationTag = "Down";
            if (viewType.ViewType == ViewType.CeilingPlan) orientationTag = "Up";
            newViewPlan.Name = BuildViewName(roomItem.Room, viewType, orientationTag);
            roomItem.Views.Add(newViewPlan);
        }

        private string LoopParameterModifiers(string modifiedValue, ParameterOption parameterOption)
        {
            if (!ViewSheetSettings.UseParameterModifiers) return modifiedValue;
            foreach (ParameterSubstring modifier in ParameterSubstrings)
            {
                int lastIndex = modifiedValue.Length - 1;

                if (modifier.ParameterOption == parameterOption)
                {
                    int startIndex;
                    if(modifier.StartIndex > lastIndex)
                    {
                        continue;
                    }
                    else
                    {
                        startIndex = modifier.StartIndex;
                    }

                    int endIndex = modifier.EndIndex;
                    if (modifier.EndIndex > lastIndex)
                    {
                        endIndex = lastIndex;
                    }
                    else
                    {
                        endIndex = modifier.EndIndex;
                    }

                    if (startIndex > lastIndex) continue;

                    int substringLength = endIndex - startIndex + 1;

                    modifiedValue = modifiedValue.Substring(startIndex, substringLength);
                    
                }
            }
            foreach (ParameterFindReplace modifier in FindReplaceParameters)
            {
                if (modifier.ParameterOption == parameterOption)
                {
                    if (string.IsNullOrEmpty(modifier.OldText)) continue;
                    string oldString = modifier.OldText;
                    string newString = modifier.NewText;
                    if(modifiedValue.Contains(oldString))
                    {
                        modifiedValue = modifiedValue.Replace(oldString, newString);
                        break;
                    }
                }
            }
            return modifiedValue;
        }
        private void CheckParameter(SelectableParameter selectableParameter, string viewOrientationTag, List<Parameter> roomParameters, ParameterOption parameterOption, ref string result)
        {
            if (selectableParameter.Name != "Empty")
            {
                if (selectableParameter.IsViewOrientation)
                {
                    result += LoopParameterModifiers(viewOrientationTag, parameterOption);
                }
                else
                {
                    Parameter p = roomParameters.Where(e => e.StorageType == StorageType.String && e.Definition.Name == selectableParameter.Name).FirstOrDefault();
                    if (p != null)
                    {
                        if (p.StorageType == StorageType.String)
                        {
                            result += LoopParameterModifiers(p.AsString(), parameterOption);
                        }
                    }
                }
            }
        }

        //build name for sheet name and number
        private string BuildViewName(Room room, string ruleName)
        {
            string result = "";
            string viewOrientationTag = "";

            ViewNamingRule rule = ViewNamingRules.Where(e => e.RuleName == ruleName).FirstOrDefault();
            List<Parameter> roomParameters = new List<Parameter>();
            foreach (Parameter p in room.Parameters) roomParameters.Add(p);
            if (rule != null)
            {
                string roomName = room.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
                result += rule.Prefix;                
                CheckParameter(rule.Parameter1, viewOrientationTag, roomParameters, ParameterOption.Parameter1, ref result);
                result += rule.Break1;
                CheckParameter(rule.Parameter2, viewOrientationTag, roomParameters, ParameterOption.Parameter2, ref result);
                result += rule.Break2;
                CheckParameter(rule.Parameter3, viewOrientationTag, roomParameters, ParameterOption.Parameter3, ref result);
                result += rule.Break3;
                CheckParameter(rule.Parameter4, viewOrientationTag, roomParameters, ParameterOption.Parameter4, ref result);
                result += rule.Suffix;
            }

            if(ruleName == "Sheet Number")
            {
                List<string> viewNames = new FilteredElementCollector(_doc).OfClass(typeof(ViewSheet))
                                .Select(e => e as ViewSheet).Where(e => e.ViewType == ViewType.DrawingSheet && !e.IsTemplate).Select(e => e.SheetNumber).ToList();
                int counter = 1;
                string viewName = result;
                while (viewNames.Contains(result))
                {
                    result = viewName + string.Format("({0})", counter);
                    counter++;
                }
            }

            return result;
        }

        //build view name for plan views and elevations
        private string BuildViewName(Room room, ViewTypeDataGridItem viewType, string viewOrientationTag)
        {
            string result = "";
            ViewType vtype = viewType.ViewType;

            ViewNamingRule rule = ViewNamingRules.Where(e => e.ViewType == vtype).FirstOrDefault();
            List<Parameter> roomParameters = new List<Parameter>();
            foreach (Parameter p in room.Parameters) roomParameters.Add(p);
            if (rule != null)
            {
                string roomName = room.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
                result += rule.Prefix;
                CheckParameter(rule.Parameter1, viewOrientationTag, roomParameters, ParameterOption.Parameter1, ref result);
                result += rule.Break1;
                CheckParameter(rule.Parameter2, viewOrientationTag, roomParameters, ParameterOption.Parameter2, ref result);
                result += rule.Break2;
                CheckParameter(rule.Parameter3, viewOrientationTag, roomParameters, ParameterOption.Parameter3, ref result);
                result += rule.Break3;
                CheckParameter(rule.Parameter4, viewOrientationTag, roomParameters, ParameterOption.Parameter4, ref result);
                result += rule.Suffix;
            }
            List<string> viewNames = new FilteredElementCollector(_doc).OfClass(typeof(View))
                                            .Select(e => e as View).Where(e => e.ViewType == vtype && !e.IsTemplate).Select(e => e.Name).ToList();
            int counter = 1;
            string viewName = result;
            while (viewNames.Contains(result))
            {
                result = viewName + string.Format("({0})", counter);
                counter++;
            }
            return result;
        }

        public bool GetSelectedItems()
        {
            bool result = true;
            SelectedRooms = Rooms.Where(e => e.IsSelected).ToList();
            SelectedViewTypes = ViewTypes.Where(e => e.IsSelected).ToList();

            if (SelectedRooms.Count == 0)
            {
                TaskDialog.Show("Error", "There is no room selected!");
                result = false;
            }
            if(SelectedViewTypes.Count == 0)
            {
                TaskDialog.Show("Error", "There is no output viewtype selected!");
                result = false;
            }
            return result;
        }

        public void GetViewTypes()
        {
            ViewTypes = new List<ViewTypeDataGridItem>();
            ViewTypeDataGridItem floorPlan = new ViewTypeDataGridItem() { IsSelected = true, ViewType = ViewType.FloorPlan, ViewFamily = ViewFamily.FloorPlan };
            floorPlan.GetViewFamilyTypes(_doc);
            floorPlan.GetViewTemplates(_doc);
            ViewTypeDataGridItem ceilingPlan = new ViewTypeDataGridItem() { IsSelected = true, ViewType = ViewType.CeilingPlan, ViewFamily = ViewFamily.CeilingPlan };
            ceilingPlan.GetViewFamilyTypes(_doc);
            ceilingPlan.GetViewTemplates(_doc);
            ViewTypeDataGridItem elevationView = new ViewTypeDataGridItem() { IsSelected = true, ViewType = ViewType.Elevation, ViewFamily = ViewFamily.Elevation };
            elevationView.GetViewFamilyTypes(_doc);
            elevationView.GetViewTemplates(_doc);
            ViewTypeDataGridItem sectionView = new ViewTypeDataGridItem() { IsSelected = true, ViewType = ViewType.Section, ViewFamily = ViewFamily.Section };
            sectionView.GetViewFamilyTypes(_doc);
            sectionView.GetViewTemplates(_doc);



            ViewTypeDataGridItem threeD = new ViewTypeDataGridItem() { IsSelected = true, ViewType = ViewType.ThreeD, ViewFamily = ViewFamily.ThreeDimensional };
            threeD.GetViewFamilyTypes(_doc);
            threeD.GetViewTemplates(_doc);
            ViewTypes.Add(floorPlan);
            ViewTypes.Add(ceilingPlan);
            ViewTypes.Add(elevationView);
            ViewTypes.Add(threeD);
            ViewTypes.Add(sectionView);
        }

        public void GetRooms(View selectedView)
        {
            GenerateSheetsSchema sheetsSchema = GenerateSheetsSchema.Initialize();
            List<Room> rooms = new FilteredElementCollector(_doc, selectedView.Id).OfClass(typeof(SpatialElement)).Select(e => e as Room).ToList();
            foreach (Room room in rooms)
            {
                if (room == null || room.Id == ElementId.InvalidElementId) continue;
                if (Rooms.Select(e => e.Room.Id.IntegerValue).ToList().Contains(room.Id.IntegerValue)) continue;
                RoomDataGridItem roomDataGridItem = new RoomDataGridItem() { IsSelected = false, Room = room, Level = room.Level, SheetViewStatus = "", ViewName = selectedView.Name};
                roomDataGridItem.GetName();
                string extractedId = sheetsSchema.ReadFromSchema(room);
                roomDataGridItem.SetSheetView(extractedId, _doc);
                roomDataGridItem.DisplayView = selectedView;
                Rooms.Add(roomDataGridItem);
            }
            Rooms = Rooms.OrderBy(x => x.Room.Number).OrderBy(y => y.ViewName).ToList();
            //window.RoomSelectionDataGrid.Items.Refresh();
        }

        public void DisplayWindow()
        {
            GenerateSheetsWindow window = new GenerateSheetsWindow(this);
            window.Show();
            //return window.WindowResult;
        }

        private BoundingBoxXYZ GetRoomBoundingBox(Room room, ViewSheetSettings viewSheetSettings)
        {
            BoundingBoxXYZ result = new BoundingBoxXYZ();
            BoundingBoxXYZ roomBox = room.get_BoundingBox(null);
            XYZ boxMin = roomBox.Min;
            XYZ boxMax = roomBox.Max;

            //Find and add boundary segments to room box
            if(viewSheetSettings.AddBoundarySegments)
            {
                IList<IList<BoundarySegment>> boundaries = room.GetBoundarySegments(new Options());
                if (boundaries != null)
                {
                    foreach (IList<BoundarySegment> segments in boundaries)
                    {
                        if (segments == null) continue;
                        foreach (BoundarySegment segment in segments)
                        {
                            if (segment == null || segment.ElementId == ElementId.InvalidElementId) continue;

                            Line line = segment.GetCurve() as Line;
                            if (line == null) continue;
                            Element element = _doc.GetElement(segment.ElementId);
                            BoundingBoxXYZ boundaryBox = element.get_BoundingBox(null);

                            //Vertical offset
                            if (Math.Abs(line.Direction.Y) == 1)
                            {
                                if (boundaryBox.Min.X < boxMin.X)
                                {
                                    boxMin = new XYZ(boundaryBox.Min.X, boxMin.Y, boxMin.Z);
                                }
                                if (boundaryBox.Max.X > boxMax.X)
                                {
                                    boxMax = new XYZ(boundaryBox.Max.X, boxMax.Y, boxMax.Z);
                                }
                            }

                            //Horizontal offset
                            if (Math.Abs(line.Direction.X) == 1)
                            {
                                if (boundaryBox.Min.Y < boxMin.Y)
                                {
                                    boxMin = new XYZ(boxMin.X, boundaryBox.Min.Y, boxMin.Z);
                                }
                                if (boundaryBox.Max.Y > boxMax.Y)
                                {
                                    boxMax = new XYZ(boxMax.X, boundaryBox.Max.Y, boxMax.Z);
                                }
                            }

                            //TopBottom offset (seems top and bottom boundaries are not found)
                            if (Math.Abs(line.Direction.Z) == 1)
                            {
                                if (boundaryBox.Min.Z < boxMin.Z)
                                {
                                    boxMin = new XYZ(boxMin.X, boxMin.Y, boundaryBox.Min.Z);
                                }
                                if (boundaryBox.Max.Z > boxMax.Z)
                                {
                                    boxMax = new XYZ(boxMax.X, boxMax.Y, boundaryBox.Max.Z);
                                }
                            }
                        }
                    }
                }
            }
#if DEBUG2020 || RELEASE2020
            double dH = UnitUtils.ConvertToInternalUnits(viewSheetSettings.RoomBoxHorOffset, DisplayUnitType.DUT_MILLIMETERS);
            double dV = UnitUtils.ConvertToInternalUnits(viewSheetSettings.RoomBoxVerOffset, DisplayUnitType.DUT_MILLIMETERS);
#elif DEBUG2023 || RELEASE2023
            double dH = UnitUtils.ConvertToInternalUnits(viewSheetSettings.RoomBoxHorOffset, UnitTypeId.Millimeters);
            double dV = UnitUtils.ConvertToInternalUnits(viewSheetSettings.RoomBoxVerOffset, UnitTypeId.Millimeters);
#endif
            boxMin = new XYZ(boxMin.X - dH, boxMin.Y - dH, boxMin.Z - dV);
            boxMax = new XYZ(boxMax.X + dH, boxMax.Y + dH, boxMax.Z + dV);

            result.Min = boxMin;
            result.Max = boxMax;

            return result;
        }

        public void SaveNamingRules()
        {
            string assemblyDirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string TTTRevitToolsDirectory = Directory.GetParent(assemblyDirPath).FullName;
            string jsonFilePath = Path.Combine(TTTRevitToolsDirectory, "naming_rules_v2.json");
            string content = JsonConvert.SerializeObject(ViewNamingRules);
            File.WriteAllText(jsonFilePath, content);
        }

        public void SaveParameterValues()
        {
            string assemblyDirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string TTTRevitToolsDirectory = Directory.GetParent(assemblyDirPath).FullName;
            string jsonFilePath = Path.Combine(TTTRevitToolsDirectory, "titleblock_sheet_parameters.json");
            List<ViewSheetParameterSerialized> list = new List<ViewSheetParameterSerialized>();

            foreach (ViewSheetParameter p in ViewSheetParameters)
            {
                if (string.IsNullOrEmpty(p.Value)) continue;
                ViewSheetParameterSerialized serialized = new ViewSheetParameterSerialized() { Name = p.Name, Value = p.Value };
                list.Add(serialized);
            }
            foreach (ViewSheetParameter p in TitleBlockParameters)
            {
                if (string.IsNullOrEmpty(p.Value)) continue;
                ViewSheetParameterSerialized serialized = new ViewSheetParameterSerialized() { Name = p.Name, Value = p.Value };
                if(!list.Select(e => e.Name).Contains(p.Name)) list.Add(serialized);
            }
            string content = JsonConvert.SerializeObject(list);
            File.WriteAllText(jsonFilePath, content);
        }

        public void SaveParameterModifiers()
        {
            string assemblyDirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string TTTRevitToolsDirectory = Directory.GetParent(assemblyDirPath).FullName;
            string jsonFilePath1 = Path.Combine(TTTRevitToolsDirectory, "modifier_substring.json");
            string jsonFilePath2 = Path.Combine(TTTRevitToolsDirectory, "modifier_findreplace.json");

            string content1 = JsonConvert.SerializeObject(ParameterSubstrings);
            string content2 = JsonConvert.SerializeObject(FindReplaceParameters);

            File.WriteAllText(jsonFilePath1, content1);
            File.WriteAllText(jsonFilePath2, content2);
        }

        public void ReadParameterModifiers()
        {
            string assemblyDirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string TTTRevitToolsDirectory = Directory.GetParent(assemblyDirPath).FullName;
            string jsonFilePath1 = Path.Combine(TTTRevitToolsDirectory, "modifier_substring.json");
            string jsonFilePath2 = Path.Combine(TTTRevitToolsDirectory, "modifier_findreplace.json");

            if(File.Exists(jsonFilePath1))
            {
                try
                {
                    ParameterSubstrings = JsonConvert.DeserializeObject<List<ParameterSubstring>>(File.ReadAllText(jsonFilePath1));
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", "Can't deserialize parameter modifiers substring: " + ex.ToString());
                }
            }
            if (File.Exists(jsonFilePath2))
            {
                try
                {
                    FindReplaceParameters = JsonConvert.DeserializeObject<List<ParameterFindReplace>>(File.ReadAllText(jsonFilePath2));
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", "Can't deserialize parameter modifiers find replace: " + ex.ToString());

                }
            }
        }

        public void GetNamingRulesList()
        {
            string assemblyDirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string TTTRevitToolsDirectory = Directory.GetParent(assemblyDirPath).FullName;
            string jsonFilePath = Path.Combine(TTTRevitToolsDirectory, "naming_rules_v2.json");
            if (File.Exists(jsonFilePath))
            {
                string content = File.ReadAllText(jsonFilePath);
                ViewNamingRules = JsonConvert.DeserializeObject<List<ViewNamingRule>>(content);
                //fix update
                if (ViewNamingRules.Count < 6)
                {
                    CreateDefaultNamingRules(jsonFilePath);
                }
            }
            else
            {
                CreateDefaultNamingRules(jsonFilePath);
            }
            foreach (ViewNamingRule rule in ViewNamingRules)
            {
                rule.Parameter1.GetIndex(SelectableParameters);
                rule.Parameter2.GetIndex(SelectableParameters);
                rule.Parameter3.GetIndex(SelectableParameters);
                rule.Parameter4.GetIndex(SelectableParameters);
            }
        }

        private void CreateDefaultNamingRules(string jsonFilePath)
        {
            ViewNamingRules = new List<ViewNamingRule>();
            ViewNamingRule floorPlanRule = new ViewNamingRule()
            {
                RuleName = "Floor Plan",
                ViewType = ViewType.FloorPlan,
                Prefix = "FloorPlan_",
                Parameter1 = new SelectableParameter() { Name = "Number"},              
                Break1 = "",
                Parameter2 = new SelectableParameter() { Name = "Empty"},
                Break2 = "",
                Parameter3 = new SelectableParameter() { Name = "Empty"},
                Break3 = "",
                Parameter4 = new SelectableParameter() { Name = "Empty" },
                Suffix = "_Suffix"
            };
            ViewNamingRule ceilingPlanRule = new ViewNamingRule()
            {
                RuleName = "Ceiling Plan",
                ViewType = ViewType.CeilingPlan,
                Prefix = "CeilingPlan_",
                Parameter1 = new SelectableParameter() { Name = "Number" },
                Break1 = "",
                Parameter2 = new SelectableParameter() { Name = "Empty" },
                Break2 = "",
                Parameter3 = new SelectableParameter() { Name = "Empty"},
                Break3 = "",
                Parameter4 = new SelectableParameter() { Name = "Empty" },
                Suffix = "_Suffix"
            };
            ViewNamingRule elevationViewRule = new ViewNamingRule()
            {
                RuleName = "Elevation",
                ViewType = ViewType.Elevation,
                Prefix = "Elevation_",
                Parameter1 = new SelectableParameter() { Name = "Number" },
                Break1 = "_",
                Parameter2 = new SelectableParameter() { Name = "ViewOrientation" },
                Break2 = "",
                Parameter3 = new SelectableParameter() { Name = "Empty" },
                Break3 = "",
                Parameter4 = new SelectableParameter() { Name = "Empty" },
                Suffix = "_Suffix"
            };
            ViewNamingRule threeDviewRule = new ViewNamingRule()
            {
                RuleName = "3D View",
                ViewType = ViewType.ThreeD,
                Prefix = "3D_View_",
                Parameter1 = new SelectableParameter() { Name = "Number" },
                Break1 = "",
                Parameter2 = new SelectableParameter() { Name = "Empty" },
                Break2 = "",
                Parameter3 = new SelectableParameter() { Name = "Empty" },
                Break3 = "",
                Parameter4 = new SelectableParameter() { Name = "Empty" },
                Suffix = "_Suffix"
            };
            ViewNamingRule sheetViewNameRule = new ViewNamingRule()
            {
                RuleName = "Sheet Name",
                ViewType = ViewType.DrawingSheet,
                Prefix = "Sheet_",
                Parameter1 = new SelectableParameter() { Name = "Number" },
                Break1 = "",
                Parameter2 = new SelectableParameter() { Name = "Empty" },
                Break2 = "",
                Parameter3 = new SelectableParameter() { Name = "Empty" },
                Break3 = "",
                Parameter4 = new SelectableParameter() { Name = "Empty" },
                Suffix = "_Suffix"
            };
            ViewNamingRule sheetViewNumberRule = new ViewNamingRule()
            {
                RuleName = "Sheet Number",
                ViewType = ViewType.DrawingSheet,
                Prefix = "Sheet_",
                Parameter1 = new SelectableParameter() { Name = "Number" },
                Break1 = "",
                Parameter2 = new SelectableParameter() { Name = "Empty" },
                Break2 = "",
                Parameter3 = new SelectableParameter() { Name = "Empty" },
                Break3 = "",
                Parameter4 = new SelectableParameter() { Name = "Empty" },
                Suffix = "_Suffix"
            };
            ViewNamingRules.Add(floorPlanRule);
            ViewNamingRules.Add(ceilingPlanRule);
            ViewNamingRules.Add(elevationViewRule);
            ViewNamingRules.Add(threeDviewRule);
            ViewNamingRules.Add(sheetViewNameRule);
            ViewNamingRules.Add(sheetViewNumberRule);
            foreach (ViewNamingRule rule in ViewNamingRules)
            {
                rule.Parameter1.GetIndex(SelectableParameters);
                rule.Parameter2.GetIndex(SelectableParameters);
                rule.Parameter3.GetIndex(SelectableParameters);
                rule.Parameter4.GetIndex(SelectableParameters);
            }
            string jsonContent = JsonConvert.SerializeObject(ViewNamingRules);
            File.WriteAllText(jsonFilePath, jsonContent);
        }

        public void GetSettings()
        {
            TitleBlocks = new FilteredElementCollector(_doc).OfClass(typeof(Family)).Select(e => e as Family).Where(e => e.FamilyCategory.Id.IntegerValue == (int)BuiltInCategory.OST_TitleBlocks).ToList();
            string assemblyDirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string TTTRevitToolsDirectory = Directory.GetParent(assemblyDirPath).FullName;
            string jsonFilePath = Path.Combine(TTTRevitToolsDirectory, "sheetview_settings.json");
            if (File.Exists(jsonFilePath))
            {
                string content = File.ReadAllText(jsonFilePath);
                ViewSheetSettings = JsonConvert.DeserializeObject<ViewSheetSettings>(content);

                //old version fix
                if(!ViewSheetSettings.UseExistingSizes && !ViewSheetSettings.CreateNewSizes)
                {
                    ViewSheetSettings.UseExistingSizes = true;
                    ViewSheetSettings.CreateNewSizes = false;
                    ViewSheetSettings.AddBoundarySegments = true;
                    ViewSheetSettings.RoomBoxVerOffset = 200;
                    ViewSheetSettings.RoomBoxHorOffset = 200;
                    ViewSheetSettings.UseParameterModifiers = false;
                    string jsonContent = JsonConvert.SerializeObject(ViewSheetSettings);
                    File.WriteAllText(jsonFilePath, jsonContent);
                }
            }
            else
            {
                ViewSheetSettings = ViewSheetSettings.CreateDefault(TitleBlocks, ViewPorts);
                string jsonContent = JsonConvert.SerializeObject(ViewSheetSettings);
                File.WriteAllText(jsonFilePath, jsonContent);
            }
        }

        public void SaveViewPortSettings()
        {
            string assemblyDirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string TTTRevitToolsDirectory = Directory.GetParent(assemblyDirPath).FullName;
            string jsonFilePath = Path.Combine(TTTRevitToolsDirectory, "sheetview_settings.json");
            string content = JsonConvert.SerializeObject(ViewSheetSettings);
            File.WriteAllText(jsonFilePath, content);           
        }

        public int GetTitleBlockIndex()
        {
            return TitleBlocks.FindIndex(e => e.Id.IntegerValue == ViewSheetSettings.TitleBlockFamilyId);
        }

        public int GetPortTypeIndex()
        {
            return ViewPorts.FindIndex(e => e.Id.IntegerValue == ViewSheetSettings.ViewPortTypeId);
        }

        class Options : SpatialElementBoundaryOptions
        {

        }
    }
}
