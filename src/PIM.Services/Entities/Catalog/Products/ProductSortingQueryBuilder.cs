using PIM.Models.Catalog.Products;
using Regira.Entities.QueryBuilders.Abstractions;

namespace PIM.Services.Entities.Catalog.Products;

public class ProductSortingQueryBuilder : ISortedQueryBuilder<Product, int, ProductSortBy>
{
    public IQueryable<Product> SortBy(IQueryable<Product> query, ProductSortBy? sortBy = null)
    {
        if (typeof(IOrderedQueryable).IsAssignableFrom(query.Expression.Type) && query is IOrderedQueryable<Product> sorted)
        {
            return sortBy switch
            {
                ProductSortBy.Id => sorted.ThenBy(x => x.Id),
                ProductSortBy.IdDesc => sorted.ThenByDescending(x => x.Id),
                ProductSortBy.Title => sorted.ThenBy(x => x.Title),
                ProductSortBy.TitleDesc => sorted.ThenByDescending(x => x.Title),
                _ => sorted.ThenBy(x => x.Title)
            };
        }

        return sortBy switch
        {
            ProductSortBy.Id => query.OrderBy(x => x.Id),
            ProductSortBy.IdDesc => query.OrderByDescending(x => x.Id),
            ProductSortBy.Title => query.OrderBy(x => x.Title),
            ProductSortBy.TitleDesc => query.OrderByDescending(x => x.Title),
            _ => query.OrderBy(x => x.Title)
        };
    }
}
