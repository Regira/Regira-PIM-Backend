using Webshop.Models.Entities.Catalog.Pricing;

namespace Webshop.Models.Entities.Catalog.Products;

public class ProductPriceHistory : PriceHistoryBase
{
    public Product? Product { get; set; }
}