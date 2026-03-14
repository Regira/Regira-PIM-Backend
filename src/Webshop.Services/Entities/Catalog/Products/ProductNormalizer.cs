using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Normalizing.Abstractions;
using Webshop.Data;
using Webshop.Models.Entities.Catalog.Allergens;
using Webshop.Models.Entities.Catalog.Categories;
using Webshop.Models.Entities.Catalog.Parts;
using Webshop.Models.Entities.Catalog.Products;

namespace Webshop.Services.Entities.Catalog.Products;

public class ProductNormalizer(INormalizer normalizer, WebshopDbContext dbContext) : EntityNormalizerBase<Product>
{
    private IList<Category> _categories = null!;
    private IList<Allergen> _allergens = null!;
    private IList<Part> _parts = null!;

    public override async Task HandleNormalizeMany(IEnumerable<Product> items)
    {
        var ids = items.Select(x => x.Id).ToList();
        var categoryIds = items.SelectMany(x => x.Categories?.Select(c => c.CategoryId) ?? []).Distinct().ToList();
        _categories = await dbContext.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync(); ;
        var allergenIds = items.SelectMany(x => x.Allergens?.Select(a => a.AllergenId) ?? []).Distinct().ToList();
        _allergens = await dbContext.Allergens
            .Where(a => allergenIds.Contains(a.Id))
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync();
        var partIds = items.SelectMany(x => x.Parts?.Select(p => p.PartId) ?? []).Distinct().ToList();
        _parts = await dbContext.Parts
            .Where(p => partIds.Contains(p.Id))
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync();
        await base.HandleNormalizeMany(items);
    }

    public override async Task HandleNormalize(Product item)
    {
        await base.HandleNormalize(item);

        var contentEntries = new List<string?> { item.NormalizedContent };

        var categories = item.Categories?.Select(pc => _categories.FirstOrDefault(c => c.Id == pc.CategoryId)?.Title).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
        contentEntries.AddRange(categories?.Select(normalizer.Normalize) ?? []);

        var allergens = item.Allergens?.Select(pa => _allergens.FirstOrDefault(a => a.Id == pa.AllergenId)?.Title).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
        contentEntries.AddRange(allergens?.Select(normalizer.Normalize) ?? []);

        var parts = item.Parts?.Select(pp => _parts.FirstOrDefault(p => p.Id == pp.PartId)?.Title).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
        contentEntries.AddRange(parts?.Select(normalizer.Normalize) ?? []);

        item.NormalizedContent = string.Join(' ', contentEntries.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}
