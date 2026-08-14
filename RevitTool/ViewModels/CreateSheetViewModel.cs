using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Spreadsheet;
using RevitTool.Services;

namespace RevitTool.ViewModels
{
    public sealed partial class CreateSheetViewModel : ObservableObject
    {
        private readonly CreateSheetHandler handler;
        private readonly ExternalEvent createEvent;

        public CreateSheetViewModel(CreateSheetHandler handler, ExternalEvent createEvent)
        {
            this.handler = handler;
            this.createEvent = createEvent;
        }

        [ObservableProperty]
        private string sheetName;

        [ObservableProperty]
        private bool isCreating;

        [RelayCommand(CanExecute = nameof(CanCreate))]
        private void Create()
        {
            IsCreating = true;

            handler.OnCompleted = OnCreateCompleted;
            handler.SheetName = SheetName;
            createEvent.Raise();
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

        private void OnCreateCompleted(bool success, string message)
        {
            IsCreating = false;

            // TaskDialog hợp lý ở đây vì Sheet vừa tạo xong nằm trong Revit -
            // user cần quay lại Revit để thấy kết quả, khác với Export không cần.
            TaskDialog.Show("Create Sheet", message);
        }
    }
}