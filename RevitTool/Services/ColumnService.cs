using RevitTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTool.Services
{
    public class ColumnService
    {
        public List<ColumnModel> GetColumns(Document doc)
        {
            List<ColumnModel> result = new List<ColumnModel>();

            List<FamilyInstance> columns = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .ToList();

            foreach (FamilyInstance b in columns)
            {
                Parameter widthParam = b.Symbol?.LookupParameter("Width") ?? b.LookupParameter("Width");
                Parameter heightParam = b.Symbol?.LookupParameter("Height") ?? b.LookupParameter("Height");
                Parameter depthParam = b.Symbol?.LookupParameter("Depth") ?? b.LookupParameter("Depth");
                Parameter materialParam = b.Symbol?.LookupParameter("Material") ?? b.LookupParameter("Material");
                Parameter commentsParam = b.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);

                double widthFeet = widthParam != null && widthParam.HasValue ? widthParam.AsDouble() : 0;
                double heightFeet = heightParam != null && heightParam.HasValue ? heightParam.AsDouble() : 0;
                double depthFeet = depthParam != null && depthParam.HasValue ? depthParam.AsDouble() : 0;

                result.Add(new ColumnModel
                {
                    Id = b.Id,
                    Name = b.Name,
                    Width = UnitUtils.ConvertFromInternalUnits(widthFeet, UnitTypeId.Meters),
                    Height = UnitUtils.ConvertFromInternalUnits(heightFeet, UnitTypeId.Meters),
                    Depth = UnitUtils.ConvertFromInternalUnits(depthFeet, UnitTypeId.Meters),
                    Material = materialParam?.AsString() ?? "",
                    Level = doc.GetElement(b.LevelId)?.Name ?? "N/A",
                    Comments = commentsParam?.AsString() ?? ""
                });
            }

            return result;
        }
    }
}
