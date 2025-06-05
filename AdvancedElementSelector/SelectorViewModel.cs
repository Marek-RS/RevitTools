using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.AdvancedElementSelector
{
    public class SelectorViewModel : INotifyPropertyChanged
    {
        public ExpanderModel CurrentExpander { get; set; }
        public AdvancedElementSelectorWindow MainWindow { get; set; }
        public ElementSelectorAction TheAction { get; set; }
        public List<ElementId> CheckedElementIds { get; set; } = new List<ElementId>();
        public ExternalEvent TheEvent { get; set; }
        private ObservableCollection<ExpanderModel> _mainExpanders;
        public ObservableCollection<ExpanderModel> MainExpanders
        {
            get => _mainExpanders;
            set
            {
                if (_mainExpanders != value)
                {
                    _mainExpanders = value;
                    OnPropertyChanged(nameof(MainExpanders));
                }
            }
        }

        List<RevitCategory> _revitCategories;
        Document _doc;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private SelectorViewModel()
        {

        }

        public void RemoveElements(ICollection<ElementId> removedElements)
        {
            for (int i = 0; i < MainExpanders.Count; i++)
            {
                ExpanderModel mainExpander = MainExpanders[i];
                for (int j = 0; j < mainExpander.SubExpanders.Count; j++)
                {
                    ExpanderModel typeExpander = mainExpander.SubExpanders[j];
                    for (int k = 0; k < typeExpander.SubExpanders.Count; k++)
                    {
                        ExpanderModel instanceExpander = typeExpander.SubExpanders[k];
                        foreach (ElementId removedId in removedElements)
                        {
                            if (instanceExpander.ElementId == removedId)
                            {
                                typeExpander.SubExpanders.RemoveAt(k);
                                CheckedElementIds.Remove(removedId);
                                typeExpander.ItemsCount--;
                                mainExpander.ItemsCount--;
                            }
                        }
                    }
                    if (typeExpander.ItemsCount == 0) mainExpander.SubExpanders.RemoveAt(j);
                }
                if (mainExpander.ItemsCount == 0) MainExpanders.RemoveAt(i);
            }
            MainWindow.RefreshContext();
        }

        public void AddNewElements(ICollection<ElementId> addedElements) 
        {
            foreach (ElementId id in addedElements)
            {
                Element el = _doc.GetElement(id);
                if (el.Category == null) continue;
                if (el.Category.Parent != null && el.Category.Parent.Id.IntegerValue == (int)BuiltInCategory.OST_Lines) continue;
                ElementType elementType = _doc.GetElement(el.GetTypeId()) as ElementType;
                if (elementType == null)
                {
                    string elementTypeName = "BuiltInType of: " + el.Category.Name;
                    if (!_revitCategories.Select(e => e.Category.Id).Contains(el.Category.Id))
                    {
                        RevitCategory revitCategory = new RevitCategory() { CategoryName = el.Category.Name, Category = el.Category };
                        revitCategory.RevitElementTypes = new List<RevitElementType>();
                        RevitElementType revitElementType = new RevitElementType() { ElementTypeName = elementTypeName };
                        revitElementType.Elements = new List<Element>() { el };
                        revitCategory.RevitElementTypes.Add(revitElementType);
                        _revitCategories.Add(revitCategory);

                        //Add new category expander
                        ExpanderModel mainExpander = new ExpanderModel();
                        mainExpander.Category = revitCategory.Category;
                        mainExpander.ExpanderName = revitCategory.CategoryName;
                        mainExpander.SubExpanders = new ObservableCollection<ExpanderModel>();
                        //Add new type expander
                        ExpanderModel subExpanderModel = new ExpanderModel();
                        subExpanderModel.ExpanderName = revitElementType.ElementTypeName;
                        subExpanderModel.SubExpanders = new ObservableCollection<ExpanderModel>();
                        //add new instance
                        ExpanderModel elementInstance = new ExpanderModel();
                        elementInstance.ExpanderName = el.Name;
                        subExpanderModel.SubExpanders.Add(elementInstance);
                        elementInstance.Element = el;
                        elementInstance.ElementId = el.Id;
                        //increment amounts
                        mainExpander.ItemsCount++;
                        subExpanderModel.ItemsCount++;
                        elementInstance.ItemsCount++;
                    }
                    else
                    {
                        RevitCategory rc = _revitCategories.Where(e => e.CategoryName == el.Category.Name).FirstOrDefault();
                        //Get category expander
                        ExpanderModel mainExpander = MainExpanders.Where(e => e.Category.Id == rc.Category.Id).FirstOrDefault();

                        if (!rc.RevitElementTypes.Select(e => e.ElementTypeName).Contains(elementTypeName))
                        {
                            RevitElementType revitElementType = new RevitElementType() { ElementTypeName = elementTypeName };
                            revitElementType.Elements = new List<Element>() { el };
                            rc.RevitElementTypes.Add(revitElementType);
                            //Add new type expander
                            ExpanderModel subExpanderModel = new ExpanderModel();
                            subExpanderModel.ExpanderName = revitElementType.ElementTypeName;
                            subExpanderModel.SubExpanders = new ObservableCollection<ExpanderModel>();
                            //add new instance
                            ExpanderModel elementInstance = new ExpanderModel();
                            elementInstance.ExpanderName = el.Name;
                            subExpanderModel.SubExpanders.Add(elementInstance);
                            elementInstance.Element = el;
                            elementInstance.ElementId = el.Id;
                            //increment amounts
                            mainExpander.ItemsCount++;
                            subExpanderModel.ItemsCount++;
                            elementInstance.ItemsCount++;
                        }
                        else
                        {
                            RevitElementType rt = rc.RevitElementTypes.Where(e => e.ElementTypeName.Equals(elementTypeName)).FirstOrDefault();
                            rt.Elements.Add(el);

                            //get type expander
                            ExpanderModel typeExpander = mainExpander.SubExpanders.Where(e => e.ExpanderName == rt.ElementTypeName).FirstOrDefault();
                            //add new instance
                            ExpanderModel elementInstance = new ExpanderModel();
                            elementInstance.ExpanderName = el.Name;
                            typeExpander.SubExpanders.Add(elementInstance);
                            elementInstance.Element = el;
                            elementInstance.ElementId = el.Id;
                            //increment amounts
                            mainExpander.ItemsCount++;
                            typeExpander.ItemsCount++;
                            elementInstance.ItemsCount++;
                        }
                    }
                }
                else
                {
                    if (!_revitCategories.Select(e => e.Category.Id).Contains(el.Category.Id))
                    {
                        RevitCategory revitCategory = new RevitCategory() { CategoryName = el.Category.Name, Category = el.Category };
                        revitCategory.RevitElementTypes = new List<RevitElementType>();
                        RevitElementType revitElementType = new RevitElementType() { ElementTypeName = elementType.Name };
                        revitElementType.Elements = new List<Element>() { el };
                        revitCategory.RevitElementTypes.Add(revitElementType);
                        _revitCategories.Add(revitCategory);

                        //Add new category expander
                        ExpanderModel mainExpander = new ExpanderModel();
                        mainExpander.Category = revitCategory.Category;
                        mainExpander.ExpanderName = revitCategory.CategoryName;
                        mainExpander.SubExpanders = new ObservableCollection<ExpanderModel>();
                        //Add new type expander
                        ExpanderModel subExpanderModel = new ExpanderModel();
                        subExpanderModel.ExpanderName = revitElementType.ElementTypeName;
                        subExpanderModel.SubExpanders = new ObservableCollection<ExpanderModel>();
                        //add new instance
                        ExpanderModel elementInstance = new ExpanderModel();
                        elementInstance.ExpanderName = el.Name;
                        subExpanderModel.SubExpanders.Add(elementInstance);
                        elementInstance.Element = el;
                        elementInstance.ElementId = el.Id;
                        //increment amounts
                        mainExpander.ItemsCount++;
                        subExpanderModel.ItemsCount++;
                        elementInstance.ItemsCount++;
                    }
                    else
                    {
                        RevitCategory rc = _revitCategories.Where(e => e.CategoryName == el.Category.Name).FirstOrDefault();
                        //Get category expander
                        ExpanderModel mainExpander = MainExpanders.Where(e => e.Category.Id == rc.Category.Id).FirstOrDefault();
                        if (!rc.RevitElementTypes.Select(e => e.ElementTypeName).Contains(elementType.Name))
                        {
                            RevitElementType revitElementType = new RevitElementType() { ElementTypeName = elementType.Name };
                            revitElementType.Elements = new List<Element>() { el };
                            rc.RevitElementTypes.Add(revitElementType);
                            //Add new type expander
                            ExpanderModel subExpanderModel = new ExpanderModel();
                            subExpanderModel.ExpanderName = revitElementType.ElementTypeName;
                            subExpanderModel.SubExpanders = new ObservableCollection<ExpanderModel>();
                            //add new instance
                            ExpanderModel elementInstance = new ExpanderModel();
                            elementInstance.ExpanderName = el.Name;
                            subExpanderModel.SubExpanders.Add(elementInstance);
                            elementInstance.Element = el;
                            elementInstance.ElementId = el.Id;
                            //increment amounts
                            mainExpander.ItemsCount++;
                            subExpanderModel.ItemsCount++;
                            elementInstance.ItemsCount++;
                        }
                        else
                        {
                            RevitElementType rt = rc.RevitElementTypes.Where(e => e.ElementTypeName.Equals(elementType.Name)).FirstOrDefault();
                            rt.Elements.Add(el);

                            //get type expander
                            ExpanderModel typeExpander = mainExpander.SubExpanders.Where(e => e.ExpanderName == rt.ElementTypeName).FirstOrDefault();
                            //add new instance
                            ExpanderModel elementInstance = new ExpanderModel();
                            elementInstance.ExpanderName = el.Name;
                            typeExpander.SubExpanders.Add(elementInstance);
                            elementInstance.Element = el;
                            elementInstance.ElementId = el.Id;
                            //increment amounts
                            mainExpander.ItemsCount++;
                            typeExpander.ItemsCount++;
                            elementInstance.ItemsCount++;
                        }
                    }
                }

            }
            MainWindow.RefreshContext();
        }

        public void ReinitializeViewModel(Document doc)
        {
            _doc = doc;
            MainWindow.CloseChildWindows();
            GetAllElementCategories();
            CreateExpanderModels();
            MainWindow.RefreshContext();
        }

        public static SelectorViewModel Initialize(Document doc)
        {
            SelectorViewModel result = new SelectorViewModel();
            result._doc = doc;
            result.GetAllElementCategories();
            result.CreateExpanderModels();
            return result;
        }

        public void DisplayWindow()
        {
            AdvancedElementSelectorWindow window = new AdvancedElementSelectorWindow(this);
            window.Show();
        }
        public bool CheckParameterFilters()
        {
            if (CurrentExpander == null) return false;
            foreach (ParameterModel parameterModel in CurrentExpander.ElementParameters)
            {
                if(!string.IsNullOrEmpty(parameterModel.SelectedOperator) && !string.IsNullOrEmpty(parameterModel.Value))
                {
                    foreach (ExpanderModel subExpander in CurrentExpander.SubExpanders)
                    {
                        foreach (ExpanderModel instanceExpander in subExpander.SubExpanders)
                        {
                            if (instanceExpander.IsSelected == true)
                            {
                                Parameter checkedParameter = instanceExpander.Element.get_Parameter(parameterModel.Parameter.Definition);
                                if (!parameterModel.CheckParameterWithValue(checkedParameter))
                                {
                                    instanceExpander.IsSelected = false;
                                    CheckedElementIds.Remove(instanceExpander.ElementId);
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        private void CreateExpanderModels()
        {
            MainExpanders = new ObservableCollection<ExpanderModel>();
            foreach (RevitCategory revCat in _revitCategories)
            {
                ExpanderModel expanderModel = new ExpanderModel();
                expanderModel.Category = revCat.Category;
                expanderModel.ExpanderName = revCat.CategoryName;
                expanderModel.SubExpanders = new ObservableCollection<ExpanderModel>();
                foreach (RevitElementType revElType in revCat.RevitElementTypes)
                {
                    ExpanderModel subExpanderModel = new ExpanderModel();
                    subExpanderModel.ExpanderName = revElType.ElementTypeName;
                    subExpanderModel.SubExpanders = new ObservableCollection<ExpanderModel>();
                    foreach (Element element in revElType.Elements)
                    {
                        ExpanderModel elementInstance = new ExpanderModel();
                        elementInstance.ExpanderName = element.Name;
                        subExpanderModel.SubExpanders.Add(elementInstance);
                        expanderModel.ItemsCount++;
                        subExpanderModel.ItemsCount++;
                        elementInstance.ItemsCount++;
                        elementInstance.Element = element;
                        elementInstance.ElementId = element.Id;
                    }
                    var typeList = subExpanderModel.SubExpanders.OrderBy(e => e.ElementId.IntegerValue).ToList();
                    subExpanderModel.SubExpanders = new ObservableCollection<ExpanderModel>(typeList);
                    expanderModel.SubExpanders.Add(subExpanderModel);
                }
                var categoryList = expanderModel.SubExpanders.OrderBy(e => e.ExpanderName).ToList();
                expanderModel.SubExpanders = new ObservableCollection<ExpanderModel>(categoryList);
                MainExpanders.Add(expanderModel);
            }
        }

        private void GetAllElementCategories()
        {
            FilteredElementCollector collector = new FilteredElementCollector(_doc).WhereElementIsNotElementType();
            _revitCategories = new List<RevitCategory>();

            foreach (Element el in collector)
            {
                
                if(null != el.Category)
                {
                    if (el.Category.Parent != null && el.Category.Parent.Id.IntegerValue == (int)BuiltInCategory.OST_Lines) continue;
                    ElementType elementType = _doc.GetElement(el.GetTypeId()) as ElementType;
                    if (elementType == null)
                    {
                        string elementTypeName = "BuiltInType of: " + el.Category.Name;
                        if (!_revitCategories.Select(e => e.Category.Id).Contains(el.Category.Id))
                        {
                            RevitCategory revitCategory = new RevitCategory() { CategoryName = el.Category.Name, Category = el.Category };
                            revitCategory.RevitElementTypes = new List<RevitElementType>();
                            RevitElementType revitElementType = new RevitElementType() { ElementTypeName = elementTypeName };
                            revitElementType.Elements = new List<Element>() { el };
                            revitCategory.RevitElementTypes.Add(revitElementType);
                            _revitCategories.Add(revitCategory);
                        }
                        else
                        {
                            RevitCategory rc = _revitCategories.Where(e => e.CategoryName == el.Category.Name).FirstOrDefault();
                            if (!rc.RevitElementTypes.Select(e => e.ElementTypeName).Contains(elementTypeName))
                            {
                                RevitElementType revitElementType = new RevitElementType() { ElementTypeName = elementTypeName };
                                revitElementType.Elements = new List<Element>() { el };
                                rc.RevitElementTypes.Add(revitElementType);
                            }
                            else
                            {
                                RevitElementType rt = rc.RevitElementTypes.Where(e => e.ElementTypeName.Equals(elementTypeName)).FirstOrDefault();
                                rt.Elements.Add(el);
                            }
                        }
                    }
                    else
                    {
                        if (!_revitCategories.Select(e => e.Category.Id).Contains(el.Category.Id))
                        {
                            RevitCategory revitCategory = new RevitCategory() { CategoryName = el.Category.Name, Category = el.Category };
                            revitCategory.RevitElementTypes = new List<RevitElementType>();
                            RevitElementType revitElementType = new RevitElementType() { ElementTypeName = elementType.Name };
                            revitElementType.Elements = new List<Element>() { el };
                            revitCategory.RevitElementTypes.Add(revitElementType);
                            _revitCategories.Add(revitCategory);
                        }
                        else
                        {
                            RevitCategory rc = _revitCategories.Where(e => e.CategoryName == el.Category.Name).FirstOrDefault();
                            if (!rc.RevitElementTypes.Select(e => e.ElementTypeName).Contains(elementType.Name))
                            {
                                RevitElementType revitElementType = new RevitElementType() { ElementTypeName = elementType.Name };
                                revitElementType.Elements = new List<Element>() { el };
                                rc.RevitElementTypes.Add(revitElementType);
                            }
                            else
                            {
                                RevitElementType rt = rc.RevitElementTypes.Where(e => e.ElementTypeName.Equals(elementType.Name)).FirstOrDefault();
                                rt.Elements.Add(el);
                            }
                        }
                    }

                }
            }
            _revitCategories = _revitCategories.OrderBy(e => e.CategoryName).ToList();
        }
    }
}
