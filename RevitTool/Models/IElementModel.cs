using Autodesk.Revit.DB;

namespace RevitTool.Models
{
    public interface IElementModel
    {
        ElementId Id { get; }

        string Name { get; }
    }
}