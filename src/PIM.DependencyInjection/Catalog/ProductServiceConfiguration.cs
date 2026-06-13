using PIM.Data;
using PIM.Models.Catalog.Products;
using PIM.Services.Entities.Catalog.Abstractions;
using PIM.Services.Entities.Catalog.Products;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Extensions;

namespace PIM.DependencyInjection.Catalog;

public static class ProductServiceConfiguration
{
    public static IEntityServiceCollection<PimDbContext> AddProducts(this IEntityServiceCollection<PimDbContext> services)
    {
        services.For<Product, ProductSearchObject, ProductSortBy, ProductIncludes>(e =>
        {
            e.AddFilter<ProductQueryFilter>();
            e.AddSortBy<ProductSortingQueryBuilder>();
            e.AddIncludes<ProductIncludingQueryBuilder>();

            e.Related(item => item.Facets);
            e.Related(item => item.Components, item => item.Components?.SetSortOrder());
            e.Related(item => item.Suppliers);

            e.AddNormalizer<ProductNormalizer>();

            e.HasRepository<ProductRepository>();
            e.AddTransient<IProductRepository, ProductRepository>();
            e.AddTransient<IProductService, ProductValidateManager>();
            e.UseEntityService<ProductValidateManager>();
        });
        return services;
    }
}
