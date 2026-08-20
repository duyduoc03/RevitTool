using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitTool.Models;
using RevitTool.Services;
using RevitTool.Views;
using System;
using System.Collections.Generic;
using System.Windows;

namespace RevitTool.ViewModels
{
    public sealed partial class RevitToolViewModel : ObservableObject
    {
        // 1 handler dùng chung cho MỌI thao tác Revit (select, place, create sheet, sửa parameter...)
        // thay vì mỗi thao tác 1 class Handler riêng - pattern lấy từ FirstTool.
        private readonly RevitEventHandler revitEvent = new();

        private readonly WallService wallService = new();
        private readonly DoorService doorService = new();
        private readonly FurnitureService furnitureService = new();
        private readonly RebarService rebarService = new();

        public RevitToolViewModel()
        {
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

            ElementId elementId = element.Id;

            revitEvent.Run(app =>
            {
                UIDocument uiDoc = app.ActiveUIDocument;

                if (uiDoc == null)
                {
                    return;
                }

                uiDoc.Selection.SetElementIds(new List<ElementId> { elementId });
                uiDoc.ShowElements(elementId);
                RevitWindowHelper.BringToFront(app);
            });
        }

        [RelayCommand]
        private void EditParameter(IElementModel element)
        {
            if (element == null)
            {
                return;
            }

            OpenModelessWindow(
                () => new EditParameterView(new EditParameterViewModel(element.Id, element.Name, revitEvent, RefreshAllTabs)),
                "Sửa Parameter");
        }

        private void RefreshAllTabs()
        {
            Walls.RefreshCommand.Execute(null);
            Doors.RefreshCommand.Execute(null);
            Furniture.RefreshCommand.Execute(null);
            Rebars.RefreshCommand.Execute(null);
        }

        public ElementTabViewModel<WallModel> Walls { get; }

        public ElementTabViewModel<DoorModel> Doors { get; }

        public ElementTabViewModel<FurnitureModel> Furniture { get; }

        public ElementTabViewModel<RebarModel> Rebars { get; }

        [RelayCommand]
        private void AddDoor()
        {
            OpenModelessWindow(
                () => new AddFamilyInstanceView(new AddFamilyInstanceViewModel(BuiltInCategory.OST_Doors, "Add Door", revitEvent, () => Doors.RefreshCommand.Execute(null))),
                "Add Door");
        }

        [RelayCommand]
        private void AddFurniture()
        {
            OpenModelessWindow(
                () => new AddFamilyInstanceView(new AddFamilyInstanceViewModel(BuiltInCategory.OST_Furniture, "Add Furniture", revitEvent, () => Furniture.RefreshCommand.Execute(null))),
                "Add Furniture");
        }

        // Sau này thêm Beam/Column chỉ cần thêm 1 command tương tự AddDoor/AddFurniture,
        // đổi BuiltInCategory - không cần View/ViewModel mới.

        [RelayCommand]
        private void CreateSheet()
        {
            OpenModelessWindow(
                () => new CreateSheetView(new CreateSheetViewModel(revitEvent)),
                "Create Sheet");
        }

        /// <summary>
        ///     Dùng chung cho mọi cửa sổ modeless (Add Door/Furniture, Create Sheet, Sửa Parameter...).
        /// </summary>
        private void OpenModelessWindow(Func<System.Windows.Window> factory, string errorTitle)
        {
            try
            {
                System.Windows.Window view = factory();

                view.Show();
                view.Activate();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    "Không thể mở cửa sổ.\n\n" + ex.Message,
                    errorTitle,
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