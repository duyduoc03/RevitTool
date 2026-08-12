using Autodesk.Revit.DB;

namespace RevitTool.Models
{
    public class DoorModel : IElementModel
    {
        public ElementId Id { get; set; }

        public string Name { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public double SillHeight { get; set; }

        public string Comments { get; set; }

        public string Level { get; set; }
    }
}