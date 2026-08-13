using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Wordprocessing;
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
        private readonly PlaceFamilyInstanceHandler placeHandler;
        private readonly ExternalEvent placeEvent;
        private readonly Action onPlaced;

        public AddFamilyInstanceViewModel(
            BuiltInCategory category,
            string title,
            PlaceFamilyInstanceHandler placeHandler,
            ExternalEvent placeEvent,
            Action onPlaced = null)
        {
            this.category = category;
            this.placeHandler = placeHandler;
            this.placeEvent = placeEvent;
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
            Autodesk.Revit.DB.Document doc = Context.ActiveDocument;

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

            // Gán callback ngay trước khi Raise vì handler dùng chung cho mọi cửa sổ Add
            // (Door/Furniture/Beam...), tránh trường hợp 2 cửa sổ mở song song ghi đè callback của nhau.
            placeHandler.OnCompleted = OnPlacementCompleted;
            placeHandler.FamilyTypeId = SelectedType.Id;
            placeEvent.Raise();
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

        private void OnPlacementCompleted(bool success, string message)
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