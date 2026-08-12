using RevitTool.ViewModels;

namespace RevitTool.Views
{
    public sealed partial class AddDoorView
    {
        public AddDoorView(AddDoorViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}