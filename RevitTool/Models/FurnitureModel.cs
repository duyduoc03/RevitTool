using Autodesk.Revit.DB;

namespace RevitTool.Models
{
    public class FurnitureModel : IElementModel
    {
        public ElementId Id { get; set; }

        public string Name { get; set; }

        public double Width { get; set; }

        public double Depth { get; set; }

        public double Height { get; set; }

        public string Level { get; set; }

        public string Comments { get; set; }
    }
}