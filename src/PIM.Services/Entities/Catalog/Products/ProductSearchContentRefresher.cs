using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PIM.Data;
using PIM.Models.Catalog.Products;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Normalizing.Abstractions;

namespace PIM.Services.Entities.Catalog.Products;

/// <summary>
/// Rebuilds the search text of the products that embed a renamed facet, component or supplier: <see cref="ProductNormalizer"/>
/// copies those titles into <see cref="Product.NormalizedContent"/> when a product is saved, so a rename elsewhere leaves it stale.
/// Only that column is written — the products' timestamps and concurrency tokens stay, since nobody edited the products.
/// </summary>
public class ProductSearchContentRefresher(PimDbContext dbContext, IEnumerable<IEntityNormalizer> normalizers, ILogger<ProductSearchContentRefresher> logger)
{
    const int BatchSize = 500;

    public Task<int> ForFacet(int facetId, CancellationToken token = default)
        => Refresh(dbContext.Set<ProductFacet>().Where(x => x.FacetId == facetId).Select(x => x.ProductId), $"facet {facetId}", token);
    public Task<int> ForComponent(int productId, CancellationToken token = default)
        => Refresh(dbContext.Set<ProductComponent>().Where(x => x.ComponentId == productId).Select(x => x.AssemblyId), $"component {productId}", token);
    public Task<int> ForSupplier(int partyId, CancellationToken token = default)
        => Refresh(dbContext.Set<ProductSupplier>().Where(x => x.SupplierId == partyId).Select(x => x.ProductId), $"supplier {partyId}", token);

    async Task<int> Refresh(IQueryable<int> productIds, string cause, CancellationToken token)
    {
        var ids = await productIds.Distinct().OrderBy(id => id).ToListAsync(token);
        var updated = 0;
        foreach (var batch in ids.Chunk(BatchSize))
        {
            // archived products are searchable too (?archived), so they are refreshed as well
            var products = await dbContext.Products.IgnoreQueryFilters()
                .Include(p => p.Facets)
                .Include(p => p.Components)
                .Include(p => p.Suppliers)
                .Where(p => batch.Contains(p.Id))
                .AsNoTracking()
                .ToListAsync(token);
            var stored = products.ToDictionary(p => p.Id, p => p.NormalizedContent);
            await Normalize(products, token);
            foreach (var product in products.Where(p => p.NormalizedContent != stored[p.Id]))
            {
                updated += await dbContext.Products.IgnoreQueryFilters()
                    .Where(p => p.Id == product.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.NormalizedContent, product.NormalizedContent), token);
            }
        }
        logger.LogInformation("Refreshed the search content of {Updated} of {Count} products after a change to {Cause}", updated, ids.Count, cause);
        return updated;
    }

    // The normalizers a save runs on a product, as the save runs them: Regira's default rebuilds the [Normalized]
    // columns from the product's own fields, then ProductNormalizer appends the related titles. ProductNormalizer
    // alone would append to the stored text.
    async Task Normalize(IReadOnlyList<Product> products, CancellationToken token)
    {
        var matching = normalizers.Distinct().ToArray().FindMatchingServices(typeof(Product));
        var exclusive = matching.FirstOrDefault(n => n.IsExclusive);
        foreach (var normalizer in exclusive != null ? [exclusive] : matching)
        {
            await normalizer.HandleNormalizeMany(products, token);
        }
    }
}
