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
        private readonly PlaceFamilyInstanceHandler placeHandler = new();
        private readonly ExternalEvent placeEvent;
        private readonly Action onPlaced;

        public AddDoorViewModel(Action onPlaced = null)
        {
            this.onPlaced = onPlaced;

            placeHandler.OnCompleted = OnPlacementCompleted;
            placeEvent = ExternalEvent.Create(placeHandler);

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