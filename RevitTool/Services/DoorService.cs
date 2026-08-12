using Autodesk.Revit.DB;
using RevitTool.Models;
using System.Collections.Generic;
using System.Linq;

namespace RevitTool.Services
{
    public class DoorService
    {
        public List<DoorModel> GetDoors(Document doc)
        {
            List<DoorModel> result = new List<DoorModel>();

            List<FamilyInstance> doors = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .ToList();

            foreach (FamilyInstance d in doors)
            {
                Parameter widthParam = d.Symbol?.LookupParameter("Width") ?? d.LookupParameter("Width");
                Parameter heightParam = d.Symbol?.LookupParameter("Height") ?? d.LookupParameter("Height");
                Parameter sillParam = d.LookupParameter("Sill Height");
                Parameter commentsParam = d.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);

                double widthFeet = widthParam != null && widthParam.HasValue ? widthParam.AsDouble() : 0;
                double heightFeet = heightParam != null && heightParam.HasValue ? heightParam.AsDouble() : 0;
                double sillFeet = sillParam != null && sillParam.HasValue ? sillParam.AsDouble() : 0;

                result.Add(new DoorModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Width = UnitUtils.ConvertFromInternalUnits(widthFeet, UnitTypeId.Meters),
                    Height = UnitUtils.ConvertFromInternalUnits(heightFeet, UnitTypeId.Meters),
                    SillHeight = UnitUtils.ConvertFromInternalUnits(sillFeet, UnitTypeId.Meters),
                    Comments = commentsParam?.AsString() ?? "",
                    Level = doc.GetElement(d.LevelId)?.Name ?? "N/A"
                });
            }

            return result;
        }

        public int GetCount(List<DoorModel> doors)
        {
            return doors.Count;
        }

        public List<FamilyTypeItem> GetDoorTypes(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .Select(s => new FamilyTypeItem
                {
                    Id = s.Id,
                    Name = $"{s.FamilyName} - {s.Name}"
                })
                .OrderBy(t => t.Name)
                .ToList();
        }
    }
}