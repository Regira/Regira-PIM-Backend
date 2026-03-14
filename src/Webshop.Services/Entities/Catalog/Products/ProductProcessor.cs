using Regira.Entities.EFcore.Processing.Abstractions;
using Webshop.Data;
using Webshop.Models.Contexts;
using Webshop.Models.Entities.Catalog.Products;
using Webshop.Services.Entities.Catalog.Pricing;

namespace Webshop.Services.Entities.Catalog.Products;

public class ProductProcessor(WebshopDbContext dbContext, IOrderContext orderContext) : IEntityProcessor<Product, ProductIncludes>
{
    public async Task Process(IList<Product> items, ProductIncludes? includes)
    {
        if (includes?.HasFlag(ProductIncludes.PriceHistory) == true)
        {
            foreach (var item in items)
            {
                item.Price = item.FindActivePrice(orderContext.OrderDate ?? DateTime.UtcNow)?.Price;
            }
        }
        else if (includes?.HasFlag(ProductIncludes.Price) == true)
        {
            var priceDate = orderContext.OrderDate ?? DateTime.UtcNow;
            var itemIds = items.Select(x => x.Id).ToList();

            var activePrices = await dbContext.Products
                .Where(ph => itemIds.Contains(ph.Id))
                .GetActivePrices<Product, ProductPriceHistory>(priceDate);

            foreach (var item in items)
                item.Price = activePrices.TryGetValue(item.Id, out var price) ? price : null;
        }
    }
}
