using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Webshop.Models.Entities.Catalog.Products;

namespace Webshop.Services.Entities.Catalog.Products;

public class ProductIncludingQueryBuilder : IIncludableQueryBuilder<Product, int, ProductIncludes>
{
    public IQueryable<Product> AddIncludes(IQueryable<Product> query, ProductIncludes? includes = null)
    {
        query = query.Include(x => x.UnitType);

        if (includes == null)
            return query;

        if (includes.Value.HasFlag(ProductIncludes.Categories))
            query = query.Include(x => x.Categories!).ThenInclude(pc => pc.Category);
        if (includes.Value.HasFlag(ProductIncludes.Parts))
            query = query.Include(x => x.Parts!).ThenInclude(pp => pp.Part!).ThenInclude(p => p.Prices!.OrderByDescending(pp => pp.EndDate ?? pp.StartDate));
        if (includes.Value.HasFlag(ProductIncludes.Allergens))
            query = query.Include(x => x.Allergens!).ThenInclude(pa => pa.Allergen);
        if (includes.Value.HasFlag(ProductIncludes.AllowedPartAdditions))
            query = query.Include(x => x.AllowedPartAdditions!).ThenInclude(pa => pa.Part!).ThenInclude(p => p.Prices!.OrderByDescending(pp => pp.EndDate ?? pp.StartDate));
        if (includes.Value.HasFlag(ProductIncludes.PriceHistory))
            query = query.Include(x => x.Prices!.OrderByDescending(pp => pp.EndDate ?? pp.StartDate));

        return query;
    }
}
