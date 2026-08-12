using Autodesk.Revit.DB;

namespace RevitTool.Models
{
    public class WallModel : IElementModel
    {
        public ElementId Id { get; set; }

        public string Name { get; set; }

        public double Length { get; set; }

        public double Area { get; set; }

        public double Volume { get; set; }

        public string Comments { get; set; }

        public string Level { get; set; }
    }
}