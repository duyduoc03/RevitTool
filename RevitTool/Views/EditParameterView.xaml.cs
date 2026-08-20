using RevitTool.Models;
using RevitTool.ViewModels;
using System.Windows.Controls;

namespace RevitTool.Views
{
    public sealed partial class EditParameterView
    {
        public EditParameterView(EditParameterViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Item is ParameterModel model && model.IsReadOnly)
            {
                e.Cancel = true;
            }
        }
    }
}