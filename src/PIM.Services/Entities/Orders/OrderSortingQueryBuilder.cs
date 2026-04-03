using PIM.Models.Orders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

namespace PIM.Services.Entities.Orders;

public class OrderSortingQueryBuilder : ISortedQueryBuilder<Order, int, OrderSortBy>
{
    public IQueryable<Order> SortBy(IQueryable<Order> query, OrderSortBy? sortBy = null)
    {
        if (typeof(IOrderedQueryable).IsAssignableFrom(query.Expression.Type) && query is IOrderedQueryable<Order> sorted)
        {
            return sortBy switch
            {
                OrderSortBy.Created => sorted.ThenBy(x => x.Created),
                OrderSortBy.CreatedDesc => sorted.ThenByDescending(x => x.Created),
                OrderSortBy.ScheduledDate => sorted.ThenBy(x => x.ScheduledDate),
                OrderSortBy.ScheduledDateDesc => sorted.ThenByDescending(x => x.ScheduledDate),
                OrderSortBy.Total => sorted.ThenBy(x => x.Total),
                OrderSortBy.TotalDesc => sorted.ThenByDescending(x => x.Total),
                OrderSortBy.Status => sorted.ThenBy(x => x.Status),
                OrderSortBy.StatusDesc => sorted.ThenByDescending(x => x.Status),
                OrderSortBy.Code => sorted.ThenBy(x => x.Code),
                OrderSortBy.CodeDesc => sorted.ThenByDescending(x => x.Code),
                _ => sorted.ThenByDescending(x => x.Id)
            };
        }

        return sortBy switch
        {
            OrderSortBy.Created => query.OrderBy(x => x.Created),
            OrderSortBy.CreatedDesc => query.OrderByDescending(x => x.Created),
            OrderSortBy.ScheduledDate => query.OrderBy(x => x.ScheduledDate),
            OrderSortBy.ScheduledDateDesc => query.OrderByDescending(x => x.ScheduledDate),
            OrderSortBy.Total => query.OrderBy(x => x.Total),
            OrderSortBy.TotalDesc => query.OrderByDescending(x => x.Total),
            OrderSortBy.Status => query.OrderBy(x => x.Status),
            OrderSortBy.StatusDesc => query.OrderByDescending(x => x.Status),
            OrderSortBy.Code => query.OrderBy(x => x.Code),
            OrderSortBy.CodeDesc => query.OrderByDescending(x => x.Code),
            _ => query.OrderByDescending(x => x.Id)
        };
    }
}
