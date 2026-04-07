using Regira.Entities.Models.Abstractions;

namespace PIM.Services.Entities.Extensions;

public static class QueryUtility
{
    public static IQueryable<TEntity> FilterIsActiveOn<TEntity>(this IQueryable<TEntity> query, DateTime? date)
        where TEntity : IHasStartDate
        => query.Where(x => x.StartDate == null || x.StartDate <= date);
}
