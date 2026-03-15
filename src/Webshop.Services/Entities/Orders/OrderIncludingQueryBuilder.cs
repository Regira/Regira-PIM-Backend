using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Webshop.Models.Entities.Orders;

namespace Webshop.Services.Entities.Orders;

public class OrderIncludingQueryBuilder : IIncludableQueryBuilder<Order, int, OrderIncludes>
{
    public IQueryable<Order> AddIncludes(IQueryable<Order> query, OrderIncludes? includes = null)
    {
        if (includes == null)
            return query;

        if (includes.Value.HasFlag(OrderIncludes.Customer))
            query = query.Include(x => x.Customer!)
                .ThenInclude(x => x.ParentRelationships!).ThenInclude(x => x.Parent);

        if (includes.Value.HasFlag(OrderIncludes.OrderLines))
            query = query.Include(x => x.OrderLines);

        if (includes.Value.HasFlag(OrderIncludes.OrderLinesArticles))
            query = query
                .Include(x => x.OrderLines!.OrderBy(l => l.SortOrder))
                    .ThenInclude(ol => ol.Article!)
                        .ThenInclude(a => a.Components!).ThenInclude(ac => ac.Component!).ThenInclude(c => c.Prices);

        if (includes.Value.HasFlag(OrderIncludes.OrderLinesArticleComponents))
            query = query
                .Include(x => x.OrderLines!)
                    .ThenInclude(ol => ol.ComponentAdditions!)
                        .ThenInclude(olp => olp.Article!).ThenInclude(a => a.Prices)
                .Include(x => x.OrderLines!)
                    .ThenInclude(ol => ol.ComponentOmissions!)
                        .ThenInclude(olp => olp.Article!).ThenInclude(a => a.Prices);

        if (includes.Value.HasFlag(OrderIncludes.OrderLinesArticlesWithComponents))
            query = query
                .Include(x => x.OrderLines!.OrderBy(l => l.SortOrder))
                    .ThenInclude(ol => ol.Article!)
                        .ThenInclude(a => a.Components!).ThenInclude(ac => ac.Component!).ThenInclude(c => c.Prices)
                .Include(x => x.OrderLines!)
                    .ThenInclude(ol => ol.ComponentAdditions!)
                        .ThenInclude(olp => olp.Article!).ThenInclude(a => a.Prices)
                .Include(x => x.OrderLines!)
                    .ThenInclude(ol => ol.ComponentOmissions!)
                        .ThenInclude(olp => olp.Article!).ThenInclude(a => a.Prices);

        return query;
    }
}
