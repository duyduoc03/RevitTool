using Autodesk.Revit.DB;

namespace RevitTool.Models
{
    public class RebarModel : IElementModel
    {
        public ElementId Id { get; set; }

        public string Name { get; set; }

        public string ShapeName { get; set; }

        public double Diameter { get; set; }

        public double Length { get; set; }

        public string HostName { get; set; }

        public string Comments { get; set; }

        public string Level { get; set; }
    }
}