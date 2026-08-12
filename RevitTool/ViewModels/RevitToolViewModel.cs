using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitTool.Models;
using RevitTool.Services;
using System.Collections.Generic;

namespace RevitTool.ViewModels
{
    public sealed partial class RevitToolViewModel : ObservableObject
    {
        private readonly SelectElementHandler selectElementHandler = new();
        private readonly ExternalEvent selectElementEvent;

        public RevitToolViewModel()
        {
            selectElementEvent = ExternalEvent.Create(selectElementHandler);
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

        private readonly WallService wallService = new();

        [ObservableProperty]
        private List<WallModel> wallList = new();

        [ObservableProperty]
        private int wallCount;

        [RelayCommand]
        private void RefreshWalls()
        {
            Document doc = Context.ActiveDocument;

            if (doc == null)
            {
                return;
            }

            List<WallModel> walls = wallService.GetWalls(doc);

            WallList = walls;
            WallCount = wallService.GetCount(walls);
        }

        private readonly DoorService doorService = new();

        [ObservableProperty]
        private List<DoorModel> doorList = new();

        [ObservableProperty]
        private int doorCount;

        [RelayCommand]
        private void RefreshDoors()
        {
            Document doc = Context.ActiveDocument;

            if (doc == null)
            {
                return;
            }

            List<DoorModel> doors = doorService.GetDoors(doc);

            DoorList = doors;
            DoorCount = doorService.GetCount(doors);
        }

        private readonly FurnitureService furnitureService = new();

        [ObservableProperty]
        private List<FurnitureModel> furnitureList = new();

        [ObservableProperty]
        private int furnitureCount;

        [RelayCommand]
        private void RefreshFurniture()
        {
            Document doc = Context.ActiveDocument;

            if (doc == null)
            {
                return;
            }

            List<FurnitureModel> furniture = furnitureService.GetFurniture(doc);

            FurnitureList = furniture;
            FurnitureCount = furnitureService.GetCount(furniture);
        }

        private readonly RebarService rebarService = new();

        [ObservableProperty]
        private List<RebarModel> rebarList = new();

        [ObservableProperty]
        private int rebarCount;

        [RelayCommand]
        private void RefreshRebars()
        {
            Document doc = Context.ActiveDocument;

            if (doc == null)
            {
                return;
            }

            List<RebarModel> rebars = rebarService.GetRebars(doc);

            RebarList = rebars;
            RebarCount = rebarService.GetCount(rebars);
        }

        [RelayCommand(CanExecute = nameof(CanExportRebars))]
        private void ExportRebars()
        {
            ExportResult result = rebarService.ExportToExcel(RebarList);

            if (result.Success)
                TaskDialog.Show("Export Rebar", result.Message + "\n\n" + result.FilePath);
            else
                TaskDialog.Show("Export Rebar", result.Message);
        }

        private bool CanExportRebars() => RebarList != null && RebarList.Count > 0;
    }
}