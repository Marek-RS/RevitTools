using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.GenerateSheets
{
    public class ViewSheetSettings
    {
        public ViewPortName TopViewLine1 { get; set; }
        public ViewPortName TopViewLine2 { get; set; }
        public ViewPortName TopViewLine3 { get; set; }
        public ViewPortName TopViewLine4 { get; set; }

        public ViewPortName BottomViewLine1 { get; set; }
        public ViewPortName BottomViewLine2 { get; set; }
        public ViewPortName BottomViewLine3 { get; set; }
        public ViewPortName BottomViewLine4 { get; set; }
        public int ViewScaleSelectedIndex { get; set; }
        public bool OverrideScaleInTemplate { get; set; }

        public int TitleBlockFamilyId { get; set; }
        public int ViewPortTypeId { get; set; }
        public double TopOffset { get; set; }
        public double BottomOffset { get; set; }
        public double LeftOffset { get; set; }
        public double RightOffset { get; set; }
        public double ViewportDistance { get; set; }
        public bool UseExistingSizes { get; set; }
        public bool CreateNewSizes { get; set; }
        public bool AddBoundarySegments { get; set; }
        public double RoomBoxVerOffset { get; set; }
        public double RoomBoxHorOffset { get; set; }

        public bool UseParameterModifiers { get; set; }

        public static ViewSheetSettings CreateDefault(List<Family> titleBlocks, List<ElementType> viewports)
        {
            ViewSheetSettings result = new ViewSheetSettings();
            result.GetDefaultViewLines();
            result.GetDefaultOffsets();
            result.TryGetTitleBlockFamily(titleBlocks);
            result.TryGetViewportTypes(viewports);
            result.ViewScaleSelectedIndex = 7;
            result.OverrideScaleInTemplate = false;
            result.UseExistingSizes = true;
            result.AddBoundarySegments = true;
            result.UseParameterModifiers = false;
            return result;
        }

        public bool IsAlreadySelected(ViewPortName viewPortName)
        {
            bool result = false;
            if (viewPortName == ViewPortName.None) return false;
            if (TopViewLine1 == viewPortName) result = true;
            if (TopViewLine2 == viewPortName) result = true;
            if (TopViewLine3 == viewPortName) result = true;
            if (TopViewLine4 == viewPortName) result = true;
            if (BottomViewLine1 == viewPortName) result = true;
            if (BottomViewLine2 == viewPortName) result = true;
            if (BottomViewLine3 == viewPortName) result = true;
            if (BottomViewLine4 == viewPortName) result = true;
            if(result) TaskDialog.Show("Warning!", "Viewport type is already selected!");
            return result;
        }

        private void GetDefaultViewLines()
        {

            TopViewLine1 = ViewPortName.ElevationNorth;
            TopViewLine2 = ViewPortName.ElevationSouth;
            TopViewLine3 = ViewPortName.ElevationWest;
            TopViewLine4 = ViewPortName.ElevationEast;

            BottomViewLine1 = ViewPortName.ThreeD;
            BottomViewLine2 = ViewPortName.FloorPlan;
            BottomViewLine3 = ViewPortName.CeilingPlan;
            BottomViewLine4 = ViewPortName.None;
        }

        private void GetDefaultOffsets()
        {
            TopOffset = 20;
            BottomOffset = 20;
            LeftOffset = 20;
            RightOffset = 100;
            ViewportDistance = 20;
            RoomBoxHorOffset = 200;
            RoomBoxVerOffset = 200;
        }

        private void TryGetTitleBlockFamily(List<Family> titleBlocks)
        {
            if(titleBlocks.Count > 0)
            {
                TitleBlockFamilyId = titleBlocks.First().Id.IntegerValue;
            }
        }

        private void TryGetViewportTypes(List<ElementType> viewports)
        {
            if(viewports.Count > 0)
            {
                ViewPortTypeId = viewports.First().Id.IntegerValue;
            }
        }
    }
}
