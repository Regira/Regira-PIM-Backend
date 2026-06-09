using Regira.Entities.Models;

namespace PIM.Models.Catalog.UnitTypes;

public record UnitTypeSearchObject : SearchObject
{
    public string? Code { get; set; }
}
