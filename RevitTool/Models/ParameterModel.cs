using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RevitTool.Models
{
    public partial class ParameterModel : ObservableObject
    {
        public ElementId Id { get; set; }

        public string Name { get; set; }

        public StorageType StorageType { get; set; }

        public bool IsReadOnly { get; set; }

        [ObservableProperty]
        private string value = string.Empty;

        // Giá trị gốc để so sánh, tránh Set lại Parameter không đổi
        public string OriginalValue { get; set; } = string.Empty;
    }
}