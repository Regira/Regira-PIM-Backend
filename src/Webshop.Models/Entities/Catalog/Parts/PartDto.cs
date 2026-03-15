using Webshop.Models.Entities.Catalog.Pricing;
using Webshop.Models.Entities.Catalog.UnitTypes;

namespace Webshop.Models.Entities.Catalog.Parts;

public class PartDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? UnitTypeId { get; set; }
    public UnitTypeDto? UnitType { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsGlobalAddition { get; set; }

    public ICollection<PriceHistoryDto>? Prices { get; set; }
}
