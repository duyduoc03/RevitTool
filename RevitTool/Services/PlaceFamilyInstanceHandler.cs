using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
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

                uiDoc.PromptForFamilyInstancePlacement(symbol);

                OnCompleted?.Invoke(true, "Đã đặt element thành công.");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                OnCompleted?.Invoke(false, "Đã hủy thao tác đặt.");
            }
            catch (Exception ex)
            {
                OnCompleted?.Invoke(false, "Lỗi khi đặt element:\n\n" + ex.Message);
            }
        }

        public string GetName()
        {
            return "Place Family Instance Handler";
        }
    }
}