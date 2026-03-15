using Regira.Entities.Models.Abstractions;

namespace Webshop.Services.Entities.Extensions;

public static class QueryUtility
{
    public static IQueryable<TEntity> FilterIsActiveOn<TEntity>(this IQueryable<TEntity> query, DateTime? date)
        where TEntity : IHasStartEndDate
        => query.Where(x => (x.StartDate == null || x.StartDate <= date) && (x.EndDate == null || x.EndDate >= date));
}
