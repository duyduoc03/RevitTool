using Nice3point.Revit.Toolkit.External;
using RevitTool.Commands;
using Autodesk.Revit.UI;

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