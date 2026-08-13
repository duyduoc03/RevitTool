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
    public sealed partial class AddDoorViewModel : ObservableObject
    {
        private readonly DoorService doorService = new();
        private readonly PlaceFamilyInstanceHandler placeHandler;
        private readonly ExternalEvent placeEvent;
        private readonly Action onPlaced;

        public AddDoorViewModel(PlaceFamilyInstanceHandler placeHandler, ExternalEvent placeEvent, Action onPlaced = null)
        {
            this.placeHandler = placeHandler;
            this.placeEvent = placeEvent;
            this.onPlaced = onPlaced;

            LoadTypes();
        }

        [ObservableProperty]
        private List<FamilyTypeItem> doorTypes = new();

        [ObservableProperty]
        private FamilyTypeItem selectedDoorType;

        [ObservableProperty]
        private bool isPlacing;

        private void LoadTypes()
        {
            Document doc = Context.ActiveDocument;

            if (doc == null)
            {
                return;
            }

            DoorTypes = doorService.GetDoorTypes(doc);
        }

        [RelayCommand(CanExecute = nameof(CanPlace))]
        private void Place()
        {
            IsPlacing = true;

            // Gán callback ngay trước khi Raise vì handler dùng chung cho mọi cửa sổ Add
            // (Door/Furniture...), tránh trường hợp 2 cửa sổ mở song song ghi đè callback của nhau.
            placeHandler.OnCompleted = OnPlacementCompleted;
            placeHandler.FamilyTypeId = SelectedDoorType.Id;
            placeEvent.Raise();
        }

        private bool CanPlace() => SelectedDoorType != null && !IsPlacing;

        partial void OnSelectedDoorTypeChanged(FamilyTypeItem value)
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

            TaskDialog.Show("Add Door", message);

            if (success)
            {
                onPlaced?.Invoke();
            }
        }
    }
}