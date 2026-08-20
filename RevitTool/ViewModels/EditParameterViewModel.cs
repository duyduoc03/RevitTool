using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Spreadsheet;
using RevitTool.Models;
using RevitTool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace RevitTool.ViewModels
{
    public sealed partial class EditParameterViewModel : ObservableObject
    {
        private readonly ParameterService parameterService = new();
        private readonly RevitEventHandler revitEvent;
        private readonly ElementId elementId;
        private readonly Action onApplied;

        public EditParameterViewModel(ElementId elementId, string elementName, RevitEventHandler revitEvent, Action onApplied = null)
        {
            this.elementId = elementId;
            this.revitEvent = revitEvent;
            this.onApplied = onApplied;

            Title = $"Sửa Parameter - {elementName}";

            LoadParameters();
        }

        public string Title { get; }

        [ObservableProperty]
        private ObservableCollection<ParameterModel> parameters = new();

        [ObservableProperty]
        private bool isApplying;

        private void LoadParameters()
        {
            // Đọc dữ liệu không cần Transaction nên gọi trực tiếp được, giống các RefreshX khác.
            Document doc = Context.ActiveDocument;

            if (doc == null)
            {
                return;
            }

            Element element = doc.GetElement(elementId);

            if (element == null)
            {
                return;
            }

            Parameters = new ObservableCollection<ParameterModel>(parameterService.GetParameters(doc, element));
        }

        [RelayCommand]
        private void Apply()
        {
            IsApplying = true;

            List<ParameterModel> snapshot = Parameters.ToList();

            revitEvent.Run(app =>
            {
                Document doc = app.ActiveUIDocument?.Document;

                if (doc == null)
                {
                    IsApplying = false;
                    TaskDialog.Show(Title, "Không tìm thấy tài liệu Revit đang mở.");
                    return;
                }

                Element element = doc.GetElement(elementId);

                if (element == null)
                {
                    IsApplying = false;
                    TaskDialog.Show(Title, "Element không còn tồn tại (có thể đã bị xóa).");
                    return;
                }

                List<string> errors;

                using (Transaction t = new Transaction(doc, "Cập nhật Parameter"))
                {
                    t.Start();
                    errors = parameterService.ApplyParameters(doc, element, snapshot);

                    if (errors.Count > 0)
                    {
                        t.RollBack();
                    }
                    else
                    {
                        t.Commit();
                    }
                }

                IsApplying = false;

                if (errors.Count > 0)
                {
                    TaskDialog.Show(Title, "Có lỗi khi cập nhật:\n\n" + string.Join("\n", errors));
                }
                else
                {
                    TaskDialog.Show(Title, "Đã cập nhật parameter thành công.");
                    onApplied?.Invoke();
                    LoadParameters();
                }
            });
        }
    }
}