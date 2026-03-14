using Regira.Entities.EFcore.Processing.Abstractions;
using Webshop.Data;
using Webshop.Models.Contexts;
using Webshop.Models.Entities.Catalog.Parts;
using Webshop.Services.Entities.Catalog.Pricing;

namespace Webshop.Services.Entities.Catalog.Parts;

public class PartProcessor(WebshopDbContext dbContext, IOrderContext orderContext) : IEntityProcessor<Part, PartIncludes>
{
    public async Task Process(IList<Part> items, PartIncludes? includes)
    {
        if (includes?.HasFlag(PartIncludes.PriceHistory) == true)
        {
            foreach (var item in items)
            {
                item.Price = item.FindActivePrice(orderContext.OrderDate ?? DateTime.UtcNow)?.Price;
            }
        }
        else if (includes?.HasFlag(PartIncludes.Price) == true)
        {
            var priceDate = orderContext.OrderDate ?? DateTime.UtcNow;
            var itemIds = items.Select(x => x.Id).ToList();

            var activePrices = await dbContext.Parts
                .Where(ph => itemIds.Contains(ph.Id))
                .GetActivePrices<Part, PartPriceHistory>(priceDate);

            foreach (var item in items)
                item.Price = activePrices.TryGetValue(item.Id, out var price) ? price : null;
        }
    }
}
