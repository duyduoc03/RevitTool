using RevitTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTool.Services
{
    public class BeamService
    {
        public List<BeamModel> GetBeams(Document doc)
        {
            List<BeamModel> result = new List<BeamModel>();

            List<FamilyInstance> beams = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_StructuralFraming)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .ToList();

            foreach (FamilyInstance b in beams)
            {
                Parameter widthParam = b.Symbol?.LookupParameter("Width") ?? b.LookupParameter("Width");
                Parameter heightParam = b.Symbol?.LookupParameter("Height") ?? b.LookupParameter("Height");
                Parameter lengthParam = b.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);
                Parameter materialParam = b.Symbol?.LookupParameter("Material") ?? b.LookupParameter("Material");
                Parameter commentsParam = b.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);

                double widthFeet = widthParam != null && widthParam.HasValue ? widthParam.AsDouble() : 0;
                double heightFeet = heightParam != null && heightParam.HasValue ? heightParam.AsDouble() : 0;
                double lengthFeet = lengthParam != null && lengthParam.HasValue ? lengthParam.AsDouble() : 0;

                result.Add(new BeamModel
                {
                    Id = b.Id,
                    Name = b.Name,
                    Width = UnitUtils.ConvertFromInternalUnits(widthFeet, UnitTypeId.Meters),
                    Height = UnitUtils.ConvertFromInternalUnits(heightFeet, UnitTypeId.Meters),
                    Length = UnitUtils.ConvertFromInternalUnits(lengthFeet, UnitTypeId.Meters),
                    Material = materialParam?.AsString() ?? "",
                    Level = doc.GetElement(b.LevelId)?.Name ?? "N/A",
                    Comments = commentsParam?.AsString() ?? ""
                });
            }

            return result;
        }
    }
}
