using RevitTool.ViewModels;

namespace RevitTool.Views
{
    public sealed partial class CreateSheetView
    {
        public CreateSheetView(CreateSheetViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
