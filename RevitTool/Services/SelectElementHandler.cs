using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RevitTool.Services
{
    public class SelectElementHandler : IExternalEventHandler
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(System.IntPtr hWnd);
        public ElementId ElementId { get; set; }

        public void Execute(UIApplication app)
        {
            UIDocument uiDoc = app.ActiveUIDocument;

            if (uiDoc == null || ElementId == null)
            {
                return;
            }

            uiDoc.Selection.SetElementIds(new List<ElementId> { ElementId });
            uiDoc.ShowElements(ElementId);

            SetForegroundWindow(app.MainWindowHandle);
        }

        public string GetName()
        {
            return "Select Element Handler";
        }
    }
}