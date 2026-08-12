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

        public int GetCount(List<RebarModel> items)
        {
            return items.Count;
        }

        private readonly ExcelExportService excelExportService = new();

        public ExportResult ExportToExcel(List<RebarModel> rebars)
        {
            return excelExportService.Export(
                rebars,
                "Rebar",
                "Rebar.xlsx",
                new[] { "Index", "Element ID", "Name", "Shape Name", "Diameter (mm)", "Length (m)", "Host Name", "Level", "Comments" },
                r => new object[] { rebars.IndexOf(r) + 1, r.Id, r.Name, r.ShapeName, r.Diameter, r.Length, r.HostName, r.Level, r.Comments });
        }
    }
}