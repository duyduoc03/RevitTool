using Autodesk.Revit.DB;
using RevitTool.Models;
using System.Collections.Generic;
using System.Linq;

namespace RevitTool.Services
{
    public class WallService
    {
        public List<WallModel> GetWalls(Document doc)
        {
            List<WallModel> result = new List<WallModel>();

            List<Wall> walls = new FilteredElementCollector(doc)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .Cast<Wall>()
                .ToList();

            foreach (Wall w in walls)
            {
                Parameter lengthParam = w.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);
                Parameter areaParam = w.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED);
                Parameter volumeParam = w.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED);
                Parameter commentsParam = w.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);

                double lengthFeet = lengthParam != null && lengthParam.HasValue ? lengthParam.AsDouble() : 0;
                double areaFeet = areaParam != null && areaParam.HasValue ? areaParam.AsDouble() : 0;
                double volumeFeet = volumeParam != null && volumeParam.HasValue ? volumeParam.AsDouble() : 0;

                result.Add(new WallModel
                {
                    Id = w.Id,
                    Name = w.Name,
                    Length = UnitUtils.ConvertFromInternalUnits(lengthFeet, UnitTypeId.Meters),
                    Area = UnitUtils.ConvertFromInternalUnits(areaFeet, UnitTypeId.SquareMeters),
                    Volume = UnitUtils.ConvertFromInternalUnits(volumeFeet, UnitTypeId.CubicMeters),
                    Level = doc.GetElement(w.LevelId)?.Name ?? "N/A",
                    Comments = commentsParam?.AsString() ?? ""
                });
            }

            return result;
        }
    }
}