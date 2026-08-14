using Autodesk.Revit.DB;

namespace RevitTool.Models
{
    public sealed class BeamModel : IElementModel
    {
        public required ElementId Id { get; init; }

        public required string Name { get; init; }

        public double Width { get; init; }

        public double Height { get; init; }

        public double Length { get; init; }

        public string? Material { get; init; }

        public required string Level { get; init; }

        public string? Comments { get; init; }
    }
}
