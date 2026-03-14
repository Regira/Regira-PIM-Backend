using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Normalizing.Abstractions;
using Webshop.Data;
using Webshop.Models.Contexts;
using Webshop.Models.Entities.Catalog.Products;
using Webshop.Models.Entities.Orders;

namespace Webshop.Services.Entities.Orders;

public class OrderNormalizer(INormalizer normalizer, WebshopDbContext dbContext, IOrderContext orderContext) : EntityNormalizerBase<Order>
{
    private List<Product> _products = null!;

    public override async Task HandleNormalizeMany(IEnumerable<Order> items)
    {
        var productIds = items.SelectMany(x => x.OrderLines?.Select(ol => ol.ProductId) ?? []).Distinct().ToList();
        _products = await dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync();

        await base.HandleNormalizeMany(items);
    }
    public override async Task HandleNormalize(Order item)
    {
        var customer = await dbContext.Customers
            .Where(c => c.Id == item.CustomerId)
            .Select(x => new
            {
                x.NormalizedTitle,
                Organization = x.Organizations!.First(o => o.OrganizationId == orderContext.OrganizationId).Organization!.Title
            })
            .SingleAsync();

        var contentEntries = new List<string?> { item.Code, normalizer.Normalize(item.Code), customer.NormalizedTitle, customer.Organization };

        var products = item.OrderLines
            ?.Select(ol => _products.FirstOrDefault(p => p.Id == ol.ProductId)?.NormalizedTitle)
            .Distinct()
            .ToList();
        contentEntries.AddRange(products ?? []);

        item.NormalizedContent = string.Join(' ', contentEntries.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}
