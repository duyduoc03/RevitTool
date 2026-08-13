using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ClosedXML.Excel;
using RevitTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitTool.Services
{
    public class RebarService
    {
        public List<RebarModel> GetRebars(Document doc)
        {
            List<RebarModel> result = new List<RebarModel>();

            List<Rebar> rebars = new FilteredElementCollector(doc)
                .OfClass(typeof(Rebar))
                .Cast<Rebar>()
                .ToList();

            foreach (Rebar r in rebars)
            {
                RebarShape rebarShape = doc.GetElement(r.GetShapeId()) as RebarShape;
                RebarBarType barType = doc.GetElement(r.GetTypeId()) as RebarBarType;
                Parameter commentsParam = r.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);

                double diameterFeet = barType?.BarNominalDiameter ?? 0;
                double lengthFeet = r.TotalLength;

                ElementId hostId = r.GetHostId();
                Element host = doc.GetElement(hostId);

                result.Add(new RebarModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    ShapeName = rebarShape?.Name ?? "N/A",
                    Diameter = UnitUtils.ConvertFromInternalUnits(diameterFeet, UnitTypeId.Millimeters),
                    Length = UnitUtils.ConvertFromInternalUnits(lengthFeet, UnitTypeId.Meters),
                    HostName = host?.Name ?? "N/A",
                    Level = GetHostLevelName(doc, host),
                    Comments = commentsParam?.AsString() ?? ""
                });
            }

            return result;
        }

        private string GetHostLevelName(Document doc, Element host)
        {
            if (host == null)
            {
                return "N/A";
            }

            ElementId levelId = null;

            if (host is Wall wall)
            {
                levelId = wall.LevelId;
            }
            else if (host is Floor floor)
            {
                levelId = floor.LevelId;
            }
            else if (host is FamilyInstance fi)
            {
                levelId = fi.LevelId;
            }

            if (levelId != null && levelId != ElementId.InvalidElementId)
            {
                return doc.GetElement(levelId)?.Name ?? "N/A";
            }

            Parameter levelParam = host.LookupParameter("Reference Level")
                ?? host.LookupParameter("Base Level")
                ?? host.LookupParameter("Level");

            if (levelParam != null && levelParam.HasValue && levelParam.StorageType == StorageType.ElementId)
            {
                return doc.GetElement(levelParam.AsElementId())?.Name ?? "N/A";
            }

            return "N/A";
        }

        public ExportResult ExportToExcel(List<RebarModel> rebars)
        {
            if (rebars == null || rebars.Count == 0)
            {
                return new ExportResult
                {
                    Success = false,
                    Message = "Không tìm thấy Rebar để xuất."
                };
            }

            string filePath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "Rebar.xlsx");

            try
            {
                using (XLWorkbook workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Rebar");

                    worksheet.Cell(1, 1).Value = "Index";
                    worksheet.Cell(1, 2).Value = "Element ID";
                    worksheet.Cell(1, 3).Value = "Name";
                    worksheet.Cell(1, 4).Value = "Shape Name";
                    worksheet.Cell(1, 5).Value = "Diameter (mm)";
                    worksheet.Cell(1, 6).Value = "Length (m)";
                    worksheet.Cell(1, 7).Value = "Host Name";
                    worksheet.Cell(1, 8).Value = "Level";
                    worksheet.Cell(1, 9).Value = "Comments";

                    int row = 2;
                    int index = 1;

                    foreach (RebarModel r in rebars)
                    {
                        worksheet.Cell(row, 1).Value = index;
                        worksheet.Cell(row, 2).Value = r.Id.Value;
                        worksheet.Cell(row, 3).Value = r.Name;
                        worksheet.Cell(row, 4).Value = r.ShapeName;
                        worksheet.Cell(row, 5).Value = r.Diameter;
                        worksheet.Cell(row, 6).Value = r.Length;
                        worksheet.Cell(row, 7).Value = r.HostName;
                        worksheet.Cell(row, 8).Value = r.Level;
                        worksheet.Cell(row, 9).Value = r.Comments;

                        row++;
                        index++;
                    }

                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(filePath);
                }

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