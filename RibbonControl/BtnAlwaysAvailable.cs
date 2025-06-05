using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TeslaRevitTools.RibbonControl
{
    public class BtnAlwaysAvailable : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            return true;
        }
    }
}
