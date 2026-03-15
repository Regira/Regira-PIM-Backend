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

        if (includes.Value.HasFlag(OrderIncludes.OrderLinesProducts))
            query = query
                .Include(x => x.OrderLines!.OrderBy(l => l.SortOrder))
                    .ThenInclude(ol => ol.Product!)
                        .ThenInclude(p => p.Parts!).ThenInclude(p => p.Part!).ThenInclude(p => p.Prices);

        if (includes.Value.HasFlag(OrderIncludes.OrderLinesParts))
            query = query
                .Include(x => x.OrderLines!)
                    .ThenInclude(ol => ol.PartAdditions!)
                        .ThenInclude(olp => olp.Part!).ThenInclude(p => p.Prices)
                .Include(x => x.OrderLines!)
                    .ThenInclude(ol => ol.PartOmissions!)
                        .ThenInclude(olp => olp.Part!).ThenInclude(p => p.Prices);

        if (includes.Value.HasFlag(OrderIncludes.OrderLinesProductsParts))
            query = query
                .Include(x => x.OrderLines!.OrderBy(l => l.SortOrder))
                    .ThenInclude(ol => ol.Product!)
                        .ThenInclude(p => p.Parts!).ThenInclude(p => p.Part!).ThenInclude(p => p.Prices)
                .Include(x => x.OrderLines!)
                    .ThenInclude(ol => ol.PartAdditions!)
                        .ThenInclude(olp => olp.Part!).ThenInclude(p => p.Prices)
                .Include(x => x.OrderLines!)
                    .ThenInclude(ol => ol.PartOmissions!)
                        .ThenInclude(olp => olp.Part!).ThenInclude(p => p.Prices);

        return query;
    }
}
