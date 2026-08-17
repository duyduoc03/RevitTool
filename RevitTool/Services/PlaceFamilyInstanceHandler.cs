using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace RevitTool.Services
{
    public class PlaceFamilyInstanceHandler : IExternalEventHandler
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public ElementId FamilyTypeId { get; set; }

        public Action<bool, string> OnCompleted { get; set; }

        public void Execute(UIApplication app)
        {
            UIDocument uiDoc = app.ActiveUIDocument;
            Document doc = uiDoc?.Document;

            if (uiDoc == null || doc == null || FamilyTypeId == null)
            {
                OnCompleted?.Invoke(false, "Không tìm thấy tài liệu Revit đang mở.");
                return;
            }

            FamilySymbol symbol = doc.GetElement(FamilyTypeId) as FamilySymbol;

            if (symbol == null)
            {
                OnCompleted?.Invoke(false, "Loại family không hợp lệ hoặc đã bị xóa.");
                return;
            }

            HashSet<ElementId> existingInstanceIds = new();

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

                SetForegroundWindow(app.MainWindowHandle);

                existingInstanceIds = GetInstanceIds(doc, symbol.Id);
                uiDoc.PromptForFamilyInstancePlacement(symbol);
                CompletePlacement(doc, symbol, existingInstanceIds);
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                CompletePlacement(doc, symbol, existingInstanceIds);
            }
            catch (Exception ex)
            {
                OnCompleted?.Invoke(false, "Lỗi khi đặt element:\n\n" + ex.Message);
            }
        }

        private void CompletePlacement(Document doc, FamilySymbol symbol, HashSet<ElementId> existingInstanceIds)
        {
            List<FamilyInstance> addedInstances = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .Where(instance => instance.Symbol?.Id == symbol.Id && !existingInstanceIds.Contains(instance.Id))
                .OrderBy(instance => instance.Id.Value)
                .ToList();

            if (addedInstances.Count == 0)
            {
                OnCompleted?.Invoke(false, "Đã hủy thao tác đặt.");
                return;
            }

            FamilyInstance instance = addedInstances[^1];
            string levelName = doc.GetElement(instance.LevelId)?.Name ?? "N/A";
            string message =
                $"Đã thêm {addedInstances.Count} element.\n\n" +
                $"Name: {instance.Name}\n" +
                $"Element ID: {instance.Id.Value}\n" +
                $"Category: {instance.Category?.Name ?? "N/A"}\n" +
                $"Level: {levelName}";

            OnCompleted?.Invoke(true, message);
        }

        private static HashSet<ElementId> GetInstanceIds(Document doc, ElementId symbolId)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .Where(instance => instance.Symbol?.Id == symbolId)
                .Select(instance => instance.Id)
                .ToHashSet();
        }

        public string GetName()
        {
            return "Place Family Instance Handler";
        }
    }
}
