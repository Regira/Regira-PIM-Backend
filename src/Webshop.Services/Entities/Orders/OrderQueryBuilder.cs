using Regira.Entities.EFcore.Extensions;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Webshop.Models.Entities.Orders;

namespace Webshop.Services.Entities.Orders;

public class OrderQueryBuilder : IFilteredQueryBuilder<Order, int, OrderSearchObject>
{
    public IQueryable<Order> Build(IQueryable<Order> query, OrderSearchObject? so)
    {
        if (so == null) return query;
        if (!string.IsNullOrWhiteSpace(so.Code))
            query = query.FilterCode(so.Code);
        if (so.CustomerId?.Any() == true)
            query = query.Where(x => so.CustomerId.Contains(x.CustomerId));
        if (so.ArticleId?.Any() == true)
            query = query.Where(x => x.OrderLines!.Any(ol => so.ArticleId.Contains(ol.ArticleId)));
        if (so.HasAdditions.HasValue)
            query = query.Where(x => so.HasAdditions == x.OrderLines!.Any(ol => ol.ComponentAdditions!.Any()));
        if (so.HasOmissions.HasValue)
            query = query.Where(x => so.HasOmissions == x.OrderLines!.Any(ol => ol.ComponentOmissions!.Any()));
        if (so.Status?.Any() == true)
            query = query.Where(x => so.Status.Contains(x.Status));
        if (so.MinScheduledDate.HasValue)
            query = query.Where(x => x.ScheduledDate >= so.MinScheduledDate.Value);
        if (so.MaxScheduledDate.HasValue)
            query = query.Where(x => x.ScheduledDate <= so.MaxScheduledDate.Value);
        if (so.MinTotal.HasValue)
            query = query.Where(x => x.Total >= so.MinTotal.Value);
        if (so.MaxTotal.HasValue)
            query = query.Where(x => x.Total <= so.MaxTotal.Value);
        return query;
    }
}
