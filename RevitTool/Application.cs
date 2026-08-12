using Nice3point.Revit.Toolkit.External;
using Autodesk.Revit.UI;
using RevitTool.Commands;
using Nice3point.Revit.Extensions.UI;

namespace RevitTool
{
    /// <summary>
    ///     Application entry point
    /// </summary>
    [UsedImplicitly]
    public class Application : ExternalApplication
    {
        public override void OnStartup()
        {
            TaskDialog.Show("RevitTool", "RevitTool Startup successfully.");
            CreateRibbon();
        }

        private void CreateRibbon()
        {
            var panel = Application.CreatePanel("Commands", "RevitTool");

            panel.AddPushButton<StartupCommand>("Execute")
                .SetImage("/RevitTool;component/Resources/Icons/RibbonIcon16.png")
                .SetLargeImage("/RevitTool;component/Resources/Icons/RibbonIcon32.png");
        }
    }
}