using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Webshop.Models.Contexts;
using Webshop.Models.Entities.Catalog.Parts;

namespace Webshop.Services.Entities.Catalog.Parts;

public class PartSortingQueryBuilder(IOrderContext orderContext) : ISortedQueryBuilder<Part, int, PartSortBy>
{
    public IQueryable<Part> SortBy(IQueryable<Part> query, PartSortBy? sortBy = null)
    {
        var priceDate = orderContext.OrderDate ?? DateTime.UtcNow;

        if (typeof(IOrderedQueryable).IsAssignableFrom(query.Expression.Type) && query is IOrderedQueryable<Part> sorted)
        {
            return sortBy switch
            {
                PartSortBy.Title => sorted.ThenBy(x => x.Title),
                PartSortBy.TitleDesc => sorted.ThenByDescending(x => x.Title),
                PartSortBy.Price => sorted.ThenBy(x => x.Prices!
                    .Where(ph => (ph.StartDate == null || ph.StartDate <= priceDate) && (ph.EndDate == null || ph.EndDate >= priceDate))
                    .OrderByDescending(ph => ph.StartDate)
                    .Select(ph => ph.Price)
                    .FirstOrDefault()),
                PartSortBy.PriceDesc => sorted.ThenByDescending(x => x.Prices!
                    .Where(ph => (ph.StartDate == null || ph.StartDate <= priceDate) && (ph.EndDate == null || ph.EndDate >= priceDate))
                    .OrderByDescending(ph => ph.StartDate)
                    .Select(ph => ph.Price)
                    .FirstOrDefault()),
                _ => sorted.ThenBy(x => x.Title)
            };
        }

        return sortBy switch
        {
            PartSortBy.Title => query.OrderBy(x => x.Title),
            PartSortBy.TitleDesc => query.OrderByDescending(x => x.Title),
            PartSortBy.Price => query.OrderBy(x => x.Prices!
                .Where(ph => (ph.StartDate == null || ph.StartDate <= priceDate) && (ph.EndDate == null || ph.EndDate >= priceDate))
                .OrderByDescending(ph => ph.StartDate)
                .Select(ph => ph.Price)
                .FirstOrDefault()),
            PartSortBy.PriceDesc => query.OrderByDescending(x => x.Prices!
                .Where(ph => (ph.StartDate == null || ph.StartDate <= priceDate) && (ph.EndDate == null || ph.EndDate >= priceDate))
                .OrderByDescending(ph => ph.StartDate)
                .Select(ph => ph.Price)
                .FirstOrDefault()),
            _ => query.OrderBy(x => x.Title)
        };
    }
}
