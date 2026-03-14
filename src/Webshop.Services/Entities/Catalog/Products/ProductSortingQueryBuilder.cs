using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Webshop.Models.Contexts;
using Webshop.Models.Entities.Catalog.Products;

namespace Webshop.Services.Entities.Catalog.Products;

public class ProductSortingQueryBuilder(IOrderContext orderContext) : ISortedQueryBuilder<Product, int, ProductSortBy>
{
    public IQueryable<Product> SortBy(IQueryable<Product> query, ProductSortBy? sortBy = null)
    {
        var priceDate = orderContext.OrderDate ?? DateTime.UtcNow;

        if (typeof(IOrderedQueryable).IsAssignableFrom(query.Expression.Type) && query is IOrderedQueryable<Product> sorted)
        {
            return sortBy switch
            {
                ProductSortBy.Title => sorted.ThenBy(x => x.Title),
                ProductSortBy.TitleDesc => sorted.ThenByDescending(x => x.Title),
                ProductSortBy.Price => sorted.ThenBy(x => x.Prices!
                    .Where(ph => (ph.StartDate == null || ph.StartDate <= priceDate) && (ph.EndDate == null || ph.EndDate >= priceDate))
                    .OrderByDescending(ph => ph.StartDate)
                    .Select(ph => ph.Price)
                    .FirstOrDefault()),
                ProductSortBy.PriceDesc => sorted.ThenByDescending(x => x.Prices!
                    .Where(ph => (ph.StartDate == null || ph.StartDate <= priceDate) && (ph.EndDate == null || ph.EndDate >= priceDate))
                    .OrderByDescending(ph => ph.StartDate)
                    .Select(ph => ph.Price)
                    .FirstOrDefault()),
                _ => sorted.ThenBy(x => x.Title)
            };
        }

        return sortBy switch
        {
            ProductSortBy.Title => query.OrderBy(x => x.Title),
            ProductSortBy.TitleDesc => query.OrderByDescending(x => x.Title),
            ProductSortBy.Price => query.OrderBy(x => x.Prices!
                .Where(ph => (ph.StartDate == null || ph.StartDate <= priceDate) && (ph.EndDate == null || ph.EndDate >= priceDate))
                .OrderByDescending(ph => ph.StartDate)
                .Select(ph => ph.Price)
                .FirstOrDefault()),
            ProductSortBy.PriceDesc => query.OrderByDescending(x => x.Prices!
                .Where(ph => (ph.StartDate == null || ph.StartDate <= priceDate) && (ph.EndDate == null || ph.EndDate >= priceDate))
                .OrderByDescending(ph => ph.StartDate)
                .Select(ph => ph.Price)
                .FirstOrDefault()),
            _ => query.OrderBy(x => x.Title)
        };
    }
}
