using Autodesk.Revit.DB;
using RevitTool.Models;
using System.Collections.Generic;
using System.Linq;

namespace RevitTool.Services
{
    public class FurnitureService
    {
        public List<FurnitureModel> GetFurniture(Document doc)
        {
            List<FurnitureModel> result = new List<FurnitureModel>();

            List<FamilyInstance> items = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Furniture)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .ToList();

            foreach (FamilyInstance f in items)
            {
                Parameter widthParam = f.Symbol?.LookupParameter("Width") ?? f.LookupParameter("Width");
                Parameter depthParam = f.Symbol?.LookupParameter("Depth") ?? f.LookupParameter("Depth");
                Parameter heightParam = f.Symbol?.LookupParameter("Height") ?? f.LookupParameter("Height");
                Parameter commentsParam = f.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);

                double widthFeet = widthParam != null && widthParam.HasValue ? widthParam.AsDouble() : 0;
                double depthFeet = depthParam != null && depthParam.HasValue ? depthParam.AsDouble() : 0;
                double heightFeet = heightParam != null && heightParam.HasValue ? heightParam.AsDouble() : 0;

                result.Add(new FurnitureModel
                {
                    Id = f.Id,
                    Name = f.Name,
                    Width = UnitUtils.ConvertFromInternalUnits(widthFeet, UnitTypeId.Meters),
                    Depth = UnitUtils.ConvertFromInternalUnits(depthFeet, UnitTypeId.Meters),
                    Height = UnitUtils.ConvertFromInternalUnits(heightFeet, UnitTypeId.Meters),
                    Level = doc.GetElement(f.LevelId)?.Name ?? "N/A",
                    Comments = commentsParam?.AsString() ?? ""
                });
            }

            return result;
        }
    }
}