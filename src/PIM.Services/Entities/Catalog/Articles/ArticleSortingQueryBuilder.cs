using PIM.Core.Abstractions;
using PIM.Models.Catalog.Articles;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

namespace PIM.Services.Entities.Catalog.Articles;

public class ArticleSortingQueryBuilder(IOrderContext orderContext) : ISortedQueryBuilder<Article, int, ArticleSortBy>
{
    public IQueryable<Article> SortBy(IQueryable<Article> query, ArticleSortBy? sortBy = null)
    {
        var priceDate = orderContext.OrderDate ?? DateTime.UtcNow;

        if (typeof(IOrderedQueryable).IsAssignableFrom(query.Expression.Type) && query is IOrderedQueryable<Article> sorted)
        {
            return sortBy switch
            {
                ArticleSortBy.Id => sorted.ThenBy(x => x.Id),
                ArticleSortBy.IdDesc => sorted.ThenByDescending(x => x.Id),
                ArticleSortBy.Title => sorted.ThenBy(x => x.Title),
                ArticleSortBy.TitleDesc => sorted.ThenByDescending(x => x.Title),
                ArticleSortBy.Price => sorted.ThenBy(x => x.Prices!
                    .Where(ph => (ph.StartDate == null || ph.StartDate <= priceDate) && (ph.EndDate == null || ph.EndDate >= priceDate))
                    .OrderByDescending(ph => ph.StartDate)
                    .Select(ph => ph.Price)
                    .FirstOrDefault()),
                ArticleSortBy.PriceDesc => sorted.ThenByDescending(x => x.Prices!
                    .Where(ph => (ph.StartDate == null || ph.StartDate <= priceDate) && (ph.EndDate == null || ph.EndDate >= priceDate))
                    .OrderByDescending(ph => ph.StartDate)
                    .Select(ph => ph.Price)
                    .FirstOrDefault()),
                _ => sorted.ThenBy(x => x.Title)
            };
        }

        return sortBy switch
        {
            ArticleSortBy.Id => query.OrderBy(x => x.Id),
            ArticleSortBy.IdDesc => query.OrderByDescending(x => x.Id),
            ArticleSortBy.Title => query.OrderBy(x => x.Title),
            ArticleSortBy.TitleDesc => query.OrderByDescending(x => x.Title),
            ArticleSortBy.Price => query.OrderBy(x => x.Prices!
                .Where(ph => (ph.StartDate == null || ph.StartDate <= priceDate) && (ph.EndDate == null || ph.EndDate >= priceDate))
                .OrderByDescending(ph => ph.StartDate)
                .Select(ph => ph.Price)
                .FirstOrDefault()),
            ArticleSortBy.PriceDesc => query.OrderByDescending(x => x.Prices!
                .Where(ph => (ph.StartDate == null || ph.StartDate <= priceDate) && (ph.EndDate == null || ph.EndDate >= priceDate))
                .OrderByDescending(ph => ph.StartDate)
                .Select(ph => ph.Price)
                .FirstOrDefault()),
            _ => query.OrderBy(x => x.Title)
        };
    }
}
