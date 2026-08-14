using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitTool.Models;
using RevitTool.Services;
using RevitTool.Views;
using System;

namespace RevitTool.ViewModels
{
    public sealed partial class RevitToolViewModel : ObservableObject
    {
        private readonly SelectElementHandler selectElementHandler = new();
        private readonly ExternalEvent selectElementEvent;

        private readonly PlaceFamilyInstanceHandler placeFamilyInstanceHandler = new();
        private readonly ExternalEvent placeFamilyInstanceEvent;

        private readonly WallService wallService = new();
        private readonly DoorService doorService = new();
        private readonly FurnitureService furnitureService = new();
        private readonly RebarService rebarService = new();

        public RevitToolViewModel()
        {
            // ExternalEvent.Create bắt buộc phải chạy trong ngữ cảnh API hợp lệ
            // (đang chạy vì đây là constructor được gọi trong StartupCommand.Execute()).
            // Tạo 1 lần duy nhất ở đây, dùng lại cho mọi cửa sổ Add mở sau này (Door/Furniture/Beam...).
            selectElementEvent = ExternalEvent.Create(selectElementHandler);
            placeFamilyInstanceEvent = ExternalEvent.Create(placeFamilyInstanceHandler);

            Walls = new ElementTabViewModel<WallModel>(wallService.GetWalls);
            Doors = new ElementTabViewModel<DoorModel>(doorService.GetDoors);
            Furniture = new ElementTabViewModel<FurnitureModel>(furnitureService.GetFurniture);
            Rebars = new ElementTabViewModel<RebarModel>(rebarService.GetRebars);
        }

        [RelayCommand]
        private void SelectElement(IElementModel element)
        {
            if (element == null)
            {
                return;
            }

            selectElementHandler.ElementId = element.Id;
            selectElementEvent.Raise();
        }

        public ElementTabViewModel<WallModel> Walls { get; }

        public ElementTabViewModel<DoorModel> Doors { get; }

        public ElementTabViewModel<FurnitureModel> Furniture { get; }

        public ElementTabViewModel<RebarModel> Rebars { get; }

        [RelayCommand]
        private void AddDoor()
        {
            OpenAddWindow(BuiltInCategory.OST_Doors, "Add Door", () => Doors.RefreshCommand.Execute(null));
        }

        [RelayCommand]
        private void AddFurniture()
        {
            OpenAddWindow(BuiltInCategory.OST_Furniture, "Add Furniture", () => Furniture.RefreshCommand.Execute(null));
        }

        // Sau này thêm Beam/Column chỉ cần thêm 1 command tương tự, gọi OpenAddWindow
        // với BuiltInCategory.OST_StructuralFraming / OST_StructuralColumns - không cần View/ViewModel mới.

        private void OpenAddWindow(BuiltInCategory category, string title, Action onPlaced)
        {
            try
            {
                var viewModel = new AddFamilyInstanceViewModel(category, title, placeFamilyInstanceHandler, placeFamilyInstanceEvent, onPlaced);
                var view = new AddFamilyInstanceView(viewModel);

                view.Show();
                view.Activate();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    "Không thể mở cửa sổ.\n\n" + ex.Message,
                    title,
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ExportRebars()
        {
            if (Rebars.Items == null || Rebars.Items.Count == 0)
            {
                System.Windows.MessageBox.Show(
                    "Không tìm thấy Rebar để xuất.",
                    "Export Rebar",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "Rebar.xlsx",
                DefaultExt = ".xlsx",
                Filter = "Excel Files (*.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            ExportResult result = rebarService.ExportToExcel(Rebars.Items, dialog.FileName);

            if (result.Success)
            {
                System.Windows.MessageBox.Show(
                    result.Message + "\n\n" + result.FilePath,
                    "Export Rebar",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            else
            {
                System.Windows.MessageBox.Show(
                    result.Message,
                    "Export Rebar",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }
}