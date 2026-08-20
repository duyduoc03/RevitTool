using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitTool.Models;
using RevitTool.Services;
using System;
using System.Collections.Generic;

namespace RevitTool.ViewModels
{
    /// <summary>
    ///     Dùng chung cho mọi loại element đặt qua FamilySymbol + PromptForFamilyInstancePlacement
    ///     (Door, Furniture, và sau này Beam/Column). Chỉ cần đổi BuiltInCategory truyền vào -
    ///     không cần viết lại View/ViewModel riêng cho từng loại.
    /// </summary>
    public sealed partial class AddFamilyInstanceViewModel : ObservableObject
    {
        private readonly FamilyTypeService typeService = new();
        private readonly BuiltInCategory category;
        private readonly RevitEventHandler revitEvent;
        private readonly Action onPlaced;

        public AddFamilyInstanceViewModel(
            BuiltInCategory category,
            string title,
            RevitEventHandler revitEvent,
            Action onPlaced = null)
        {
            this.category = category;
            this.revitEvent = revitEvent;
            this.onPlaced = onPlaced;

            Title = title;

            LoadTypes();
        }

        public string Title { get; }

        [ObservableProperty]
        private List<FamilyTypeItem> types = new();

        [ObservableProperty]
        private FamilyTypeItem selectedType;

        [ObservableProperty]
        private bool isPlacing;

        private void LoadTypes()
        {
            Document doc = Context.ActiveDocument;

            if (doc == null)
            {
                return;
            }

            Types = typeService.GetTypes(doc, category);
        }

        [RelayCommand(CanExecute = nameof(CanPlace))]
        private void Place()
        {
            IsPlacing = true;

            // Chụp lại giá trị đang chọn trước khi vào lambda - tránh phụ thuộc vào
            // SelectedType có thể đổi trong lúc chờ Revit xử lý.
            ElementId typeId = SelectedType.Id;

            revitEvent.Run(app =>
            {
                UIDocument uiDoc = app.ActiveUIDocument;
                Document doc = uiDoc?.Document;

                if (uiDoc == null || doc == null)
                {
                    Complete(false, "Không tìm thấy tài liệu Revit đang mở.");
                    return;
                }

                FamilySymbol symbol = doc.GetElement(typeId) as FamilySymbol;

                if (symbol == null)
                {
                    Complete(false, "Loại family không hợp lệ hoặc đã bị xóa.");
                    return;
                }

                try
                {
                    if (!symbol.IsActive)
                    {
                        using (Transaction t = new Transaction(doc, "Activate Family Type"))
                        {
                            t.Start();
                            symbol.Activate();
                            doc.Regenerate();
                            t.Commit();
                        }
                    }

                    RevitWindowHelper.BringToFront(app);

                    uiDoc.PromptForFamilyInstancePlacement(symbol);

                    Complete(true, "Đã đặt element thành công.");
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    Complete(false, "Đã hủy thao tác đặt.");
                }
                catch (Exception ex)
                {
                    Complete(false, "Lỗi khi đặt element:\n\n" + ex.Message);
                }
            });
        }

        private bool CanPlace() => SelectedType != null && !IsPlacing;

        partial void OnSelectedTypeChanged(FamilyTypeItem value)
        {
            PlaceCommand.NotifyCanExecuteChanged();
        }

        partial void OnIsPlacingChanged(bool value)
        {
            PlaceCommand.NotifyCanExecuteChanged();
        }

        private void Complete(bool success, string message)
        {
            IsPlacing = false;

            TaskDialog.Show(Title, message);

            if (success)
            {
                onPlaced?.Invoke();
            }
        }
    }
}