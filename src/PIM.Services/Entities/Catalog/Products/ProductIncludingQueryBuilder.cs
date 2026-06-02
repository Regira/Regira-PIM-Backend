using Microsoft.EntityFrameworkCore;
using PIM.Models.Catalog.Products;
using Regira.Entities.QueryBuilders.Abstractions;

namespace PIM.Services.Entities.Catalog.Products;

public class ProductIncludingQueryBuilder : IIncludableQueryBuilder<Product, int, ProductIncludes>
{
    public IQueryable<Product> AddIncludes(IQueryable<Product> query, ProductIncludes? includes = null)
    {
        query = query.Include(x => x.UnitType);

        if (includes == null)
            return query;

        if (includes.Value.HasFlag(ProductIncludes.Facets))
            query = query.Include(x => x.Facets!.Where(f => !f.Facet!.IsArchived)).ThenInclude(ac => ac.Facet);
        if (includes.Value.HasFlag(ProductIncludes.Components))
            query = query
                .Include(x => x.Components!.Where(c => !c.Component!.IsArchived).OrderBy(c => c.SortOrder).ThenBy(c => c.Component!.Title))
                    .ThenInclude(ac => ac.Component!).ThenInclude(x => x.UnitType);
        if (includes.Value.HasFlag(ProductIncludes.Suppliers))
            query = query.Include(x => x.Suppliers!.Where(s => !s.Supplier!.IsArchived).OrderBy(s => s.Supplier!.Id)).ThenInclude(s => s.Supplier);

        return query;
    }
}
