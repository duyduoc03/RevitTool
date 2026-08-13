using RevitTool.ViewModels;
using System.Windows.Controls;

namespace RevitTool.Views
{
    public sealed partial class RevitToolView
    {
        public RevitToolView(RevitToolViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();

            viewModel.RefreshWallsCommand.Execute(null);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // DataGrid cũng raise Selector.SelectionChangedEvent (cùng RoutedEvent với TabControl)
            // và bubble lên - phải chặn để không tự refresh lại mỗi khi user chọn 1 dòng trong bảng.
            if (e.OriginalSource is not TabControl tabControl || DataContext is not RevitToolViewModel viewModel)
            {
                return;
            }

            switch (tabControl.SelectedIndex)
            {
                case 0:
                    viewModel.RefreshWallsCommand.Execute(null);
                    break;
                case 1:
                    viewModel.RefreshDoorsCommand.Execute(null);
                    break;
                case 2:
                    viewModel.RefreshFurnitureCommand.Execute(null);
                    break;
                case 3:
                    viewModel.RefreshRebarsCommand.Execute(null);
                    break;
            }
        }
    }
}