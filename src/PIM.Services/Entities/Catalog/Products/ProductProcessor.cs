using PIM.Core.Abstractions;
using PIM.Data;
using PIM.Models.Catalog.Products;
using PIM.Services.Entities.Catalog.Pricing;
using Regira.Entities.EFcore.Processing.Abstractions;

namespace PIM.Services.Entities.Catalog.Products;

public class ProductProcessor(PimDbContext dbContext, IOrderContext orderContext) : IEntityProcessor<Product, ProductIncludes>
{
    public async Task Process(IList<Product> items, ProductIncludes? includes)
    {
        if (includes?.HasFlag(ProductIncludes.PricePeriod) == true)
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
                .Where(a => itemIds.Contains(a.Id))
                .GetActivePrices<Product, ProductPricePeriod>(priceDate);

            foreach (var item in items)
                item.Price = activePrices.TryGetValue(item.Id, out var price) ? price : null;
        }
    }
}
