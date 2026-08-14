using Autodesk.Revit.DB;

namespace RevitTool.Models
{
    public class FamilyTypeItem
    {
        public required ElementId Id { get; init; }

        public required string Name { get; init; }
    }
}
