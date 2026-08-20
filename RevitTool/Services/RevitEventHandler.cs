using Autodesk.Revit.UI;
using System;

namespace RevitTool.Services
{
    public class RevitEventHandler : IExternalEventHandler
    {
        private Action<UIApplication> action;
        private readonly ExternalEvent externalEvent;

        public RevitEventHandler()
        {
            externalEvent = ExternalEvent.Create(this);
        }

        public void Run(Action<UIApplication> handler)
        {
            action = handler;
            externalEvent.Raise();
        }

        public void Execute(UIApplication app)
        {
            action?.Invoke(app);
        }

        public string GetName() => "RevitTool Event Handler";
    }
}