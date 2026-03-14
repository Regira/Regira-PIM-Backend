using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.Processing.Abstractions;
using Webshop.Data;
using Webshop.Models.Entities.Catalog.Categories;

namespace Webshop.Services.Entities.Catalog.Categories;

public class CategoryProcessor(WebshopDbContext dbContext) : IEntityProcessor<Category, CategoryIncludes>
{
    public async Task Process(IList<Category> items, CategoryIncludes? includes)
    {
        var itemIds = items.Select(x => x.Id).ToList();
        var counts = await dbContext.Categories
            .Where(x => itemIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Products!.Count })
            .AsNoTrackingWithIdentityResolution()
            .ToDictionaryAsync(x => x.Id, x => x.Count);
        foreach (var item in items)
            item.ProductCount = counts.TryGetValue(item.Id, out var count) ? count : null;
    }
}
