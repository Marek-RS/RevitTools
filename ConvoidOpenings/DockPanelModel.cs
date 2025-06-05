using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.ConvoidOpenings
{
    internal class DockPanelModel
    {
        private static Guid dockGuid = new Guid("1B163AB2-1C91-46CC-A3C8-DE9EA2599134");

        public void RegisterDockPanel(UIControlledApplication uIControlledApp, ConvoidViewModel viewModel)
        {
            DockablePaneProviderData data = new DockablePaneProviderData();
            OpeningsManagerDock dockPanelPage = new OpeningsManagerDock(viewModel);
            dockPanelPage.SetupDockablePane(data);
            DockablePaneId dpid = new DockablePaneId(dockGuid);
            uIControlledApp.RegisterDockablePane(dpid, "Openings Manager", dockPanelPage);
        }

        public static void SwitchDockPanelVisibility(UIApplication application)
        {
            DockablePaneId dpid = new DockablePaneId(dockGuid);
            DockablePane dp = application.GetDockablePane(dpid);
            if (dp.IsShown())
            {
                dp.Hide();
            }
            else
            {
                dp.Show();
            }
        }
    }
}
