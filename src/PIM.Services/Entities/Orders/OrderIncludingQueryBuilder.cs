using Microsoft.EntityFrameworkCore;
using PIM.Models.Orders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

namespace PIM.Services.Entities.Orders;

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

        if (includes.Value.HasFlag(OrderIncludes.OrderLinesProducts))
            query = query
                .Include(x => x.OrderLines!.OrderBy(l => l.SortOrder))
                    .ThenInclude(ol => ol.Product!)
                        .ThenInclude(a => a.Components!).ThenInclude(ac => ac.Component!).ThenInclude(c => c.Prices);

        if (includes.Value.HasFlag(OrderIncludes.OrderLinesProductComponents))
            query = query
                .Include(x => x.OrderLines!)
                    .ThenInclude(ol => ol.ComponentAdditions!)
                        .ThenInclude(olp => olp.Product!).ThenInclude(a => a.Prices)
                .Include(x => x.OrderLines!)
                    .ThenInclude(ol => ol.ComponentOmissions!)
                        .ThenInclude(olp => olp.Product!).ThenInclude(a => a.Prices);

        if (includes.Value.HasFlag(OrderIncludes.OrderLinesProductsWithComponents))
            query = query
                .Include(x => x.OrderLines!.OrderBy(l => l.SortOrder))
                    .ThenInclude(ol => ol.Product!)
                        .ThenInclude(a => a.Components!).ThenInclude(ac => ac.Component!).ThenInclude(c => c.Prices)
                .Include(x => x.OrderLines!)
                    .ThenInclude(ol => ol.ComponentAdditions!)
                        .ThenInclude(olp => olp.Product!).ThenInclude(a => a.Prices)
                .Include(x => x.OrderLines!)
                    .ThenInclude(ol => ol.ComponentOmissions!)
                        .ThenInclude(olp => olp.Product!).ThenInclude(a => a.Prices);

        return query;
    }
}
