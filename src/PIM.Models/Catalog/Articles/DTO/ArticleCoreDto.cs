using PIM.Models.Catalog.UnitTypes;

namespace PIM.Models.Catalog.Articles.DTO;

public class ArticleCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int? UnitTypeId { get; set; }
    public UnitTypeDto? UnitType { get; set; }
}