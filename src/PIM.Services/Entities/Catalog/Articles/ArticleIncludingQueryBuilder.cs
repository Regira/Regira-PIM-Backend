using Microsoft.EntityFrameworkCore;
using PIM.Models.Catalog.Articles;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

namespace PIM.Services.Entities.Catalog.Articles;

public class ArticleIncludingQueryBuilder : IIncludableQueryBuilder<Article, int, ArticleIncludes>
{
    public IQueryable<Article> AddIncludes(IQueryable<Article> query, ArticleIncludes? includes = null)
    {
        query = query.Include(x => x.UnitType);

        if (includes == null)
            return query;

        if (includes.Value.HasFlag(ArticleIncludes.Facets))
            query = query.Include(x => x.Facets!).ThenInclude(ac => ac.Facet);
        if (includes.Value.HasFlag(ArticleIncludes.Components))
            query = query.Include(x => x.Components!.OrderBy(c => c.Component!.Title)).ThenInclude(ac => ac.Component!).ThenInclude(x => x.UnitType);
        if (includes.Value.HasFlag(ArticleIncludes.AllowedComponentAdditions))
            query = query.Include(x => x.AllowedComponentAdditions!).ThenInclude(ac => ac.Component!).ThenInclude(a => a.Prices!.OrderByDescending(ph => ph.EndDate ?? ph.StartDate));
        if (includes.Value.HasFlag(ArticleIncludes.Suppliers))
            query = query.Include(x => x.Suppliers!.OrderBy(s => s.Supplier!.Id)).ThenInclude(s => s.Supplier);
        if (includes.Value.HasFlag(ArticleIncludes.PricePeriod))
            query = query.Include(x => x.Prices!.OrderByDescending(ph => ph.EndDate ?? ph.StartDate));

        return query;
    }
}
