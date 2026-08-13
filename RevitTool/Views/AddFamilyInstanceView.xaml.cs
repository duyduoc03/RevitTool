using RevitTool.ViewModels;

namespace RevitTool.Views
{
    public sealed partial class AddFamilyInstanceView
    {
        public AddFamilyInstanceView(AddFamilyInstanceViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}