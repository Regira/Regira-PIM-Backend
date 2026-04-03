using PIM.Models.Catalog.Pricing;

namespace PIM.Models.Catalog.Articles;

public class ArticlePricePeriod : PricePeriodBase
{
    public Article? Article { get; set; }
}