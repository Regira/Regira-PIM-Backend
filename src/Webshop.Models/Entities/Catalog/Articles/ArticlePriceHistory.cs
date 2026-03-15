using Webshop.Models.Entities.Catalog.Pricing;

namespace Webshop.Models.Entities.Catalog.Articles;

public class ArticlePriceHistory : PriceHistoryBase
{
    public Article? Article { get; set; }
}