using PIM.Models.Stakeholders.Parties;
using Regira.Entities.QueryBuilders.Abstractions;

namespace PIM.Services.Entities.Stakeholders.Parties;

public class PartySortingQueryBuilder : ISortedQueryBuilder<Party, int, PartySortBy>
{
    public IQueryable<Party> SortBy(IQueryable<Party> query, PartySortBy? sortBy = null)
    {
        if (typeof(IOrderedQueryable).IsAssignableFrom(query.Expression.Type) && query is IOrderedQueryable<Party> sorted)
        {
            return sortBy switch
            {
                PartySortBy.Id => sorted.ThenBy(x => x.Id),
                PartySortBy.IdDesc => sorted.ThenByDescending(x => x.Id),
                PartySortBy.Title => sorted.ThenBy(x => x.NormalizedTitle),
                PartySortBy.TitleDesc => sorted.ThenByDescending(x => x.NormalizedTitle),
                PartySortBy.Created => sorted.ThenBy(x => x.Created),
                PartySortBy.CreatedDesc => sorted.ThenByDescending(x => x.Created),
                PartySortBy.LastModified => sorted.ThenBy(x => x.LastModified),
                PartySortBy.LastModifiedDesc => sorted.ThenByDescending(x => x.LastModified),
                _ => sorted.ThenBy(x => x.NormalizedTitle)
            };
        }

        return sortBy switch
        {
            PartySortBy.Id => query.OrderBy(x => x.Id),
            PartySortBy.IdDesc => query.OrderByDescending(x => x.Id),
            PartySortBy.Title => query.OrderBy(x => x.NormalizedTitle),
            PartySortBy.TitleDesc => query.OrderByDescending(x => x.NormalizedTitle),
            PartySortBy.Created => query.OrderBy(x => x.Created),
            PartySortBy.CreatedDesc => query.OrderByDescending(x => x.Created),
            PartySortBy.LastModified => query.OrderBy(x => x.LastModified),
            PartySortBy.LastModifiedDesc => query.OrderByDescending(x => x.LastModified),
            _ => query.OrderBy(x => x.NormalizedTitle)
        };
    }
}
