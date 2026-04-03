using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models.Catalog.Articles;
using PIM.Models.Stakeholders.Parties;
using PIM.Models.Taxonomy.Facets;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Normalizing.Abstractions;

namespace PIM.Services.Entities.Catalog.Articles;

public class ArticleNormalizer(INormalizer normalizer, PimDbContext dbContext) : EntityNormalizerBase<Article>
{
    private IList<Facet> _facets = null!;
    private IList<Article> _components = null!;
    private IList<Party> _suppliers = null!;

    public override async Task HandleNormalizeMany(IEnumerable<Article> items)
    {
        var facetIds = items.SelectMany(x => x.Facets?.Select(c => c.FacetId) ?? []).Distinct().ToList();
        _facets = await dbContext.Facets
            .Where(c => facetIds.Contains(c.Id))
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

        var facets = item.Facets?.Select(ac => _facets.FirstOrDefault(c => c.Id == ac.FacetId)?.Title).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
        contentEntries.AddRange(facets?.Select(normalizer.Normalize) ?? []);

        var components = item.Components?.Select(ac => _components.FirstOrDefault(a => a.Id == ac.ComponentId)?.Title).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
        contentEntries.AddRange(components?.Select(normalizer.Normalize) ?? []);

        var suppliers = item.Suppliers?.Select(s => _suppliers.FirstOrDefault(p => p.Id == s.SupplierId)?.Title).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
        contentEntries.AddRange(suppliers?.Select(normalizer.Normalize) ?? []);

        item.NormalizedContent = string.Join(' ', contentEntries.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}
