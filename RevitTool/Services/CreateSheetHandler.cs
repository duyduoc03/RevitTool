using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitTool.Services
{
    public class CreateSheetHandler : IExternalEventHandler
    {
        public string SheetName { get; set; }

        public Action<bool, string> OnCompleted { get; set; }

        public void Execute(UIApplication app)
        {
            UIDocument uiDoc = app.ActiveUIDocument;
            Document doc = uiDoc?.Document;

            if (uiDoc == null || doc == null)
            {
                OnCompleted?.Invoke(false, "Không tìm thấy tài liệu Revit đang mở.");
                return;
            }

            if (string.IsNullOrWhiteSpace(SheetName))
            {
                OnCompleted?.Invoke(false, "Vui lòng nhập tên Sheet.");
                return;
            }

            View activeView = uiDoc.ActiveView;

            if (activeView == null)
            {
                OnCompleted?.Invoke(false, "Không tìm thấy view đang mở để đặt lên Sheet.");
                return;
            }

            if (activeView is ViewSheet)
            {
                OnCompleted?.Invoke(false, "View đang mở là 1 Sheet - không thể đặt Sheet lên Sheet khác.");
                return;
            }

            using (Transaction t = new Transaction(doc, "Create Sheet"))
            {
                t.Start();

                try
                {
                    ViewSheet sheet = ViewSheet.Create(doc, ElementId.InvalidElementId);
                    sheet.Name = SheetName;
                    sheet.SheetNumber = GenerateUniqueSheetNumber(doc);

                    if (!Viewport.CanAddViewToSheet(doc, sheet.Id, activeView.Id))
                    {
                        t.RollBack();
                        OnCompleted?.Invoke(false, "View hiện tại không thể đặt lên Sheet (có thể là Schedule, Legend, hoặc đã được đặt ở Sheet khác).");
                        return;
                    }

                    XYZ center = GetSheetCenter(sheet);
                    Viewport.Create(doc, sheet.Id, activeView.Id, center);

                    t.Commit();

                    OnCompleted?.Invoke(true, $"Đã tạo Sheet \"{sheet.SheetNumber} - {sheet.Name}\" và đặt view \"{activeView.Name}\" lên đó.");
                }
                catch (Exception ex)
                {
                    if (t.GetStatus() == TransactionStatus.Started)
                    {
                        t.RollBack();
                    }

                    OnCompleted?.Invoke(false, "Lỗi khi tạo Sheet:\n\n" + ex.Message);
                }
            }
        }

        private string GenerateUniqueSheetNumber(Document doc)
        {
            HashSet<string> existingNumbers = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .Select(s => s.SheetNumber)
                .ToHashSet();

            int index = 1;
            string number;

            do
            {
                number = $"A-{index:00}";
                index++;
            }
            while (existingNumbers.Contains(number));

            return number;
        }

        private XYZ GetSheetCenter(ViewSheet sheet)
        {
            try
            {
                BoundingBoxUV outline = sheet.Outline;

                double centerU = (outline.Min.U + outline.Max.U) / 2;
                double centerV = (outline.Min.V + outline.Max.V) / 2;

                if (centerU > 0 && centerV > 0)
                {
                    return new XYZ(centerU, centerV, 0);
                }
            }
            catch
            {
                // Sheet không có Title Block nên không lấy được Outline chính xác - dùng fallback bên dưới.
            }

            // Sheet trống không có khung tên nên không có khổ giấy để tính "chính giữa" chính xác.
            // Dùng toạ độ xấp xỉ - có thể kéo lại vị trí viewport thủ công sau khi tạo nếu cần.
            return new XYZ(1.5, 1.0, 0);
        }

        public string GetName()
        {
            return "Create Sheet Handler";
        }
    }
}