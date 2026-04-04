using PIM.Models.Catalog.Pricing;

namespace PIM.Models.Catalog.Products;

public class ProductPricePeriod : PricePeriodBase
{
    public Product? Product { get; set; }
}