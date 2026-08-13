using Autodesk.Revit.DB;
using RevitTool.Models;
using System.Collections.Generic;
using System.Linq;

namespace RevitTool.Services
{
    public class FamilyTypeService
    {
        public List<FamilyTypeItem> GetTypes(Document doc, BuiltInCategory category)
        {
            return new FilteredElementCollector(doc)
                .OfCategory(category)
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .Select(s => new FamilyTypeItem
                {
                    Id = s.Id,
                    Name = $"{s.FamilyName} - {s.Name}"
                })
                .OrderBy(t => t.Name)
                .ToList();
        }
    }
}