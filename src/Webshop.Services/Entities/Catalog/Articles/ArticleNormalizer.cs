using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Normalizing.Abstractions;
using Webshop.Data;
using Webshop.Models.Entities.Catalog.Articles;
using Webshop.Models.Entities.Catalog.Categories;
using Webshop.Models.Entities.Stakeholders.Parties;

namespace Webshop.Services.Entities.Catalog.Articles;

public class ArticleNormalizer(INormalizer normalizer, WebshopDbContext dbContext) : EntityNormalizerBase<Article>
{
    private IList<Category> _categories = null!;
    private IList<Article> _components = null!;
    private IList<Party> _suppliers = null!;

    public override async Task HandleNormalizeMany(IEnumerable<Article> items)
    {
        var categoryIds = items.SelectMany(x => x.Categories?.Select(c => c.CategoryId) ?? []).Distinct().ToList();
        _categories = await dbContext.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync();
        var componentIds = items.SelectMany(x => x.Components?.Select(c => c.ComponentId) ?? []).Distinct().ToList();
        _components = await dbContext.Articles
            .Where(a => componentIds.Contains(a.Id))
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync();
        var supplierIds = items.SelectMany(x => x.Suppliers?.Select(s => s.SupplierId) ?? []).Distinct().ToList();
        _suppliers = await dbContext.Parties
            .Where(p => supplierIds.Contains(p.Id))
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync();
        await base.HandleNormalizeMany(items);
    }

    public override async Task HandleNormalize(Article item)
    {
        await base.HandleNormalize(item);

        var contentEntries = new List<string?> { item.NormalizedContent };

        var categories = item.Categories?.Select(ac => _categories.FirstOrDefault(c => c.Id == ac.CategoryId)?.Title).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
        contentEntries.AddRange(categories?.Select(normalizer.Normalize) ?? []);

        var components = item.Components?.Select(ac => _components.FirstOrDefault(a => a.Id == ac.ComponentId)?.Title).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
        contentEntries.AddRange(components?.Select(normalizer.Normalize) ?? []);

        var suppliers = item.Suppliers?.Select(s => _suppliers.FirstOrDefault(p => p.Id == s.SupplierId)?.Title).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
        contentEntries.AddRange(suppliers?.Select(normalizer.Normalize) ?? []);

        item.NormalizedContent = string.Join(' ', contentEntries.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}
