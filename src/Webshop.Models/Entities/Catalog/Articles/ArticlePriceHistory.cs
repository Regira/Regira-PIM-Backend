using Webshop.Models.Entities.Catalog.Pricing;

namespace Webshop.Models.Entities.Catalog.Articles;

public class ArticlePricePeriod : PricePeriodBase
{
    public Article? Article { get; set; }
}