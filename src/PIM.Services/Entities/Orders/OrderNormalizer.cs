using Microsoft.EntityFrameworkCore;
using PIM.Core.Abstractions;
using PIM.Data;
using PIM.Models.Catalog.Products;
using PIM.Models.Orders;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Normalizing.Abstractions;

namespace PIM.Services.Entities.Orders;

public class OrderNormalizer(INormalizer normalizer, PimDbContext dbContext, IOrderContext orderContext) : EntityNormalizerBase<Order>
{
    private List<Product> _products = null!;

    public override async Task HandleNormalizeMany(IEnumerable<Order> items)
    {
        var itemList = items.ToList();
        var productIds = itemList.SelectMany(x => x.OrderLines?.Select(ol => ol.ProductId) ?? []).Distinct().ToList();
        _products = await dbContext.Products
            .Where(a => productIds.Contains(a.Id))
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync();

        await base.HandleNormalizeMany(itemList);
    }
    public override async Task HandleNormalize(Order item)
    {
        var customer = await dbContext.Parties
            .FirstOrDefaultAsync(c => c.Id == item.CustomerId);

        var contentEntries = new List<string?> { item.Code, normalizer.Normalize(item.Code), customer?.NormalizedTitle };

        var products = item.OrderLines
            ?.Select(ol => _products.FirstOrDefault(a => a.Id == ol.ProductId)?.NormalizedTitle)
            .Distinct()
            .ToList();
        contentEntries.AddRange(products ?? []);

        item.NormalizedContent = string.Join(' ', contentEntries.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}
