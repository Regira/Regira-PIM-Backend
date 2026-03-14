using Webshop.Models.Entities.Catalog.Pricing;

namespace Webshop.Models.Entities.Catalog.Parts;

public class PartPriceHistory : PriceHistoryBase
{
    public Part Part { get; set; } = null!;
}