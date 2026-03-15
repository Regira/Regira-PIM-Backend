using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Webshop.Data;
using Webshop.Models.Entities.Catalog.Products;
using Webshop.Services.Entities.Catalog.Products;

namespace Webshop.DependencyInjection.Catalog;

public static class ProductServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddProducts(this IEntityServiceCollection<WebshopDbContext> services)
    {
        services.For<Product, ProductSearchObject, ProductSortBy, ProductIncludes>(e =>
        {
            e.AddQueryFilter<ProductQueryBuilder>();
            e.SortBy<ProductSortingQueryBuilder>();
            e.Includes<ProductIncludingQueryBuilder>();
            e.Related(x => x.Categories);
            e.Related(x => x.Parts);
            e.Related(x => x.AllowedPartAdditions);
            e.Related(x => x.Allergens);
            e.Related(x => x.Prices);
            e.Process<ProductProcessor>();
            e.AddNormalizer<ProductNormalizer>();
            e.UseEntityService<ProductValidateManager>();
        });
        return services;
    }
}
