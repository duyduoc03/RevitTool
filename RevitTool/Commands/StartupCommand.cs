using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using RevitTool.Services;
using RevitTool.ViewModels;
using RevitTool.Views;

namespace RevitTool.Commands
{
    /// <summary>
    ///     External command entry point.
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class StartupCommand : ExternalCommand
    {
        public override void Execute()
        {
            var viewModel = new RevitToolViewModel();
            var view = new RevitToolView(viewModel);

            view.Show();
        }
    }
}