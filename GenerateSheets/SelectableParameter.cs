using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.GenerateSheets
{
    public class SelectableParameter
    {
        public string Name { get; set; }
        public bool IsViewOrientation { get; set; }
        public int Index { get; set; }

        public SelectableParameter()
        {
            IsViewOrientation = false;
        }

        public void GetIndex(List<SelectableParameter> selectableParameters)
        {
            try
            {
                Index = selectableParameters.FindIndex(e => e.Name == Name);
            }
            catch (Exception)
            {
                Index = 0;
            }
        }
    }
}
