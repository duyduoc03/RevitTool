using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitTool.Models;
using RevitTool.Services;
using System.Windows;

namespace RevitTool.ViewModels
{
    public sealed partial class CreateSheetViewModel : ObservableObject
    {
        private readonly SheetService sheetService = new();
        private readonly RevitEventHandler revitEvent;

        public CreateSheetViewModel(RevitEventHandler revitEvent)
        {
            this.revitEvent = revitEvent;
        }

        [ObservableProperty]
        private string sheetName;

        [ObservableProperty]
        private bool isCreating;

        [RelayCommand(CanExecute = nameof(CanCreate))]
        private void Create()
        {
            IsCreating = true;

            string name = SheetName;

            revitEvent.Run(app =>
            {
                UIDocument uiDoc = app.ActiveUIDocument;
                OperationResult result = sheetService.CreateSheet(uiDoc?.Document, uiDoc?.ActiveView, name);

                IsCreating = false;

                // TaskDialog hợp lý ở đây vì Sheet vừa tạo xong nằm trong Revit -
                // user cần quay lại Revit để thấy kết quả, khác với Export không cần.
                TaskDialog.Show("Create Sheet", result.Message);
            });
        }

        private bool CanCreate() => !string.IsNullOrWhiteSpace(SheetName) && !IsCreating;

        partial void OnSheetNameChanged(string value)
        {
            CreateCommand.NotifyCanExecuteChanged();
        }

        partial void OnIsCreatingChanged(bool value)
        {
            CreateCommand.NotifyCanExecuteChanged();
        }
    }
}