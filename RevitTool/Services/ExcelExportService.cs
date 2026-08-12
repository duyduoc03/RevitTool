using ClosedXML.Excel;
using RevitTool.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace RevitTool.Services
{
    public class ExcelExportService
    {
        public ExportResult Export<T>(
            List<T> items,
            string sheetName,
            string fileName,
            string[] headers,
            Func<T, object[]> rowMapper)
        {
            if (items == null || items.Count == 0)
            {
                return new ExportResult
                {
                    Success = false,
                    Message = $"Không tìm thấy dữ liệu {sheetName} để xuất."
                };
            }

            string filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                fileName);

            try
            {
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add(sheetName);

                for (int c = 0; c < headers.Length; c++)
                    ws.Cell(1, c + 1).Value = headers[c];

                int row = 2;
                foreach (var item in items)
                {
                    object[] values = rowMapper(item);
                    for (int c = 0; c < values.Length; c++)
                        ws.Cell(row, c + 1).Value = XLCellValue.FromObject(values[c]);
                    row++;
                }

                ws.Columns().AdjustToContents();
                workbook.SaveAs(filePath);

                return new ExportResult
                {
                    Success = true,
                    FilePath = filePath,
                    Message = "Đã xuất file thành công."
                };
            }
            catch (Exception ex)
            {
                return new ExportResult
                {
                    Success = false,
                    Message = "Không thể lưu file. Có thể file đang mở trong Excel.\n\n" + ex.Message
                };
            }
        }
    }
}