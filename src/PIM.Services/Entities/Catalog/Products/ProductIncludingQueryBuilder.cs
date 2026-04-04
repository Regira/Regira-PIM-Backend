using Microsoft.EntityFrameworkCore;
using PIM.Models.Catalog.Products;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

namespace PIM.Services.Entities.Catalog.Products;

public class ProductIncludingQueryBuilder : IIncludableQueryBuilder<Product, int, ProductIncludes>
{
    public IQueryable<Product> AddIncludes(IQueryable<Product> query, ProductIncludes? includes = null)
    {
        query = query.Include(x => x.UnitType);

        if (includes == null)
            return query;

        if (includes.Value.HasFlag(ProductIncludes.Facets))
            query = query.Include(x => x.Facets!).ThenInclude(ac => ac.Facet);
        if (includes.Value.HasFlag(ProductIncludes.Components))
            query = query.Include(x => x.Components!.OrderBy(c => c.Component!.Title)).ThenInclude(ac => ac.Component!).ThenInclude(x => x.UnitType);
        if (includes.Value.HasFlag(ProductIncludes.AllowedComponentAdditions))
            query = query.Include(x => x.AllowedComponentAdditions!).ThenInclude(ac => ac.Component!).ThenInclude(a => a.Prices!.OrderByDescending(ph => ph.EndDate ?? ph.StartDate));
        if (includes.Value.HasFlag(ProductIncludes.Suppliers))
            query = query.Include(x => x.Suppliers!.OrderBy(s => s.Supplier!.Id)).ThenInclude(s => s.Supplier);
        if (includes.Value.HasFlag(ProductIncludes.PricePeriod))
            query = query.Include(x => x.Prices!.OrderByDescending(ph => ph.EndDate ?? ph.StartDate));

        return query;
    }
}
