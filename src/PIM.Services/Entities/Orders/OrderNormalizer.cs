using Microsoft.EntityFrameworkCore;
using PIM.Core.Abstractions;
using PIM.Data;
using PIM.Models.Catalog.Articles;
using PIM.Models.Orders;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Normalizing.Abstractions;

namespace PIM.Services.Entities.Orders;

public class OrderNormalizer(INormalizer normalizer, PimDbContext dbContext, IOrderContext orderContext) : EntityNormalizerBase<Order>
{
    private List<Article> _articles = null!;

    public override async Task HandleNormalizeMany(IEnumerable<Order> items)
    {
        var itemList = items.ToList();
        var articleIds = itemList.SelectMany(x => x.OrderLines?.Select(ol => ol.ArticleId) ?? []).Distinct().ToList();
        _articles = await dbContext.Articles
            .Where(a => articleIds.Contains(a.Id))
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync();

        await base.HandleNormalizeMany(itemList);
    }
    public override async Task HandleNormalize(Order item)
    {
        var customer = await dbContext.Parties
            .FirstOrDefaultAsync(c => c.Id == item.CustomerId);

        var contentEntries = new List<string?> { item.Code, normalizer.Normalize(item.Code), customer?.NormalizedTitle };

        var articles = item.OrderLines
            ?.Select(ol => _articles.FirstOrDefault(a => a.Id == ol.ArticleId)?.NormalizedTitle)
            .Distinct()
            .ToList();
        contentEntries.AddRange(articles ?? []);

        item.NormalizedContent = string.Join(' ', contentEntries.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}
