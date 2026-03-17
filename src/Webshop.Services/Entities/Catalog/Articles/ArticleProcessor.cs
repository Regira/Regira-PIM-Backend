using Regira.Entities.EFcore.Processing.Abstractions;
using Webshop.Core.Abstractions;
using Webshop.Data;
using Webshop.Models.Catalog.Articles;
using Webshop.Services.Entities.Catalog.Pricing;

namespace Webshop.Services.Entities.Catalog.Articles;

public class ArticleProcessor(WebshopDbContext dbContext, IOrderContext orderContext) : IEntityProcessor<Article, ArticleIncludes>
{
    public async Task Process(IList<Article> items, ArticleIncludes? includes)
    {
        if (includes?.HasFlag(ArticleIncludes.PricePeriod) == true)
        {
            foreach (var item in items)
            {
                item.Price = item.FindActivePrice(orderContext.OrderDate ?? DateTime.UtcNow)?.Price;
            }
        }
        else if (includes?.HasFlag(ArticleIncludes.Price) == true)
        {
            var priceDate = orderContext.OrderDate ?? DateTime.UtcNow;
            var itemIds = items.Select(x => x.Id).ToList();

            var activePrices = await dbContext.Articles
                .Where(a => itemIds.Contains(a.Id))
                .GetActivePrices<Article, ArticlePricePeriod>(priceDate);

            foreach (var item in items)
                item.Price = activePrices.TryGetValue(item.Id, out var price) ? price : null;
        }
    }
}
