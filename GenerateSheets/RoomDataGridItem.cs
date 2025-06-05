using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.GenerateSheets
{
    public class RoomDataGridItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        public string Name { get; set; }
        public string ViewName { get; set; }
        public View DisplayView { get; set; }
        public Level Level { get; set; }
        public Room Room { get; set; }
        public List<View> Views { get; set; }
        public ElementId SheetViewId { get; set; }
        public View SheetView { get; set; }
        private string _sheetViewStatus;
        public string SheetViewStatus
        {
            get => _sheetViewStatus;
            set
            {
                if (_sheetViewStatus != value)
                {
                    _sheetViewStatus = value;
                    OnPropertyChanged(nameof(SheetViewStatus));
                }
            }
        }

        public RoomDataGridItem()
        {
            Views = new List<View>();
        }

        public void OpenSheetView(UIDocument uiDoc)
        {
            if (SheetView == null) return;
            uiDoc.ActiveView = SheetView;
        }

        public void OpenAllViews(UIDocument uiDoc)
        {
            if (SheetView == null) return;
            Document doc = uiDoc.Document;
            ViewSheet viewSheet = SheetView as ViewSheet;
            List<ElementId> allIds = viewSheet.GetAllPlacedViews().ToList();
            allIds.Add(viewSheet.Id);
            foreach (ElementId id in allIds)
            {
                View view = doc.GetElement(id) as View;
                if (view == null) continue;
                if (view.ViewType == ViewType.Elevation || view.ViewType == ViewType.ThreeD || view.ViewType == ViewType.FloorPlan || view.ViewType == ViewType.CeilingPlan)
                {
                    uiDoc.ActiveView = view;
                }
            }
            uiDoc.ActiveView = SheetView;
        }

        private void DeleteElevationMarkers(Document doc)
        {
            List<ElevationMarker> markers = new FilteredElementCollector(doc).OfClass(typeof(ElevationMarker)).Select(e => e as ElevationMarker).ToList();
            if(markers.Count > 0)
            {                
                doc.Delete(markers.Where(e => e.CurrentViewCount == 0).Select(e => e.Id).ToList());
            }
        }

        public void DeleteViews(UIDocument uiDoc)
        {
            if (SheetView == null) return;

            Document doc = uiDoc.Document;
            ViewSheet viewSheet = SheetView as ViewSheet;
            List<ElementId> allIds = viewSheet.GetAllPlacedViews().ToList();
            allIds.Add(viewSheet.Id);
            IList<UIView> uiViews = uiDoc.GetOpenUIViews();
            List<ElementId> viewIds = uiViews.Select(e => e.ViewId).ToList();
            List<ElementId> notDeleteViews = viewIds.Where(e => !allIds.Contains(e)).ToList();
            if(allIds.Contains(doc.ActiveView.Id) && notDeleteViews.Count > 0)
            {
                
                View view = doc.GetElement(notDeleteViews.FirstOrDefault()) as View;
                uiDoc.ActiveView = view;
            }


            using (Transaction tx = new Transaction(doc, "Remove Views and Sheet"))
            {
                tx.Start();
                if (viewSheet != null)
                {
                    foreach (ElementId id in allIds)
                    {
                        View view = doc.GetElement(id) as View;
                        if (view != null)
                        {
                            if (view.ViewType == ViewType.Elevation || view.ViewType == ViewType.ThreeD || view.ViewType == ViewType.FloorPlan || view.ViewType == ViewType.CeilingPlan)
                            {
                                if(view.Id.IntegerValue == doc.ActiveView.Id.IntegerValue)
                                {
                                    TaskDialog.Show("Warning!", "Can't remove currently opened and active view from API. Please remove it manually!");
                                }
                                else
                                {
                                    doc.Delete(view.Id);
                                }
                            }
                        }
                    }
                    if (viewSheet.Id.IntegerValue == doc.ActiveView.Id.IntegerValue)
                    {
                        TaskDialog.Show("Warning", "Can't remove currently opened and active view from API!. Please remove it manually!");
                    }
                    else
                    {
                        doc.Delete(viewSheet.Id);
                    }
                }
                TaskDialogResult result = TaskDialog.Show("ToolInfo", "Would you like to remove empty elevation markers (markers with no views)?", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);
                if (result == TaskDialogResult.Yes)
                {
                    DeleteElevationMarkers(uiDoc.Document);
                }
                tx.Commit();
            }
            SheetView = null;
            SheetViewId = null;
            SheetViewStatus = "Sheet and views removed!";
            IsSelected = false;
            Views = new List<View>();
        }

        public void SetSheetView(string uniqueId, Document doc)
        {
            SheetView = doc.GetElement(uniqueId) as View;
            if (SheetView != null) SheetViewStatus = "Sheet view found! <double click to open>";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void GetName()
        {
            Parameter p = Room.get_Parameter(BuiltInParameter.ROOM_NAME);
            if(p != null)
            {
                Name = p.AsString();
            }
            else
            {
                Name = "empty";
            }
        }
    }
}
