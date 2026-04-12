using PIM.Data;
using PIM.Models.Catalog.Products;
using PIM.Services.Entities.Catalog.Abstractions;
using PIM.Services.Entities.Catalog.Products;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;

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
            e.Related(x => x.Facets);
            e.Related(x => x.Components);
            e.Related(x => x.AllowedComponentAdditions);
            e.Related(x => x.Suppliers);
            e.Related(x => x.Prices);
            e.AddProcessor<ProductProcessor>();
            e.AddNormalizer<ProductNormalizer>();
            e.HasRepository<ProductRepository>();
            e.AddTransient<IProductRepository, ProductRepository>();
            e.AddTransient<IProductService, ProductValidateManager>();
            e.UseEntityService<ProductValidateManager>();
        });
        return services;
    }
}
