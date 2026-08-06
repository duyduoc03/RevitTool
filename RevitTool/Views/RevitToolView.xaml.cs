using RevitTool.ViewModels;

namespace RevitTool.Views
{
    public sealed partial class RevitToolView
    {
        public RevitToolView(RevitToolViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}