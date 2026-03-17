using Webshop.Models.Catalog.Pricing;

namespace Webshop.Models.Catalog.Articles;

public class ArticlePricePeriod : PricePeriodBase
{
    public Article? Article { get; set; }
}