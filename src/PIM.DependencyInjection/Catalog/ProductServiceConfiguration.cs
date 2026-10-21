using Microsoft.Extensions.DependencyInjection;
using PIM.Data;
using PIM.Models.Catalog.Products;
using PIM.Services.Entities.Catalog.Abstractions;
using PIM.Services.Entities.Catalog.Products;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Extensions;
using Regira.Entities.Reactors.Abstractions;

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
            e.AddValidator<ProductValidator>();
            // A renamed, archived or restored product changes the search text of the assemblies it is a component of
            e.React(c => c.Kind == EntityChangeKind.Modified && (c.HasChanged(x => x.Title) || c.HasChanged(x => x.IsArchived)),
                (c, sp, _) => ProductSearchContentReactions.Refresh(sp, (refresher, token) => refresher.ForComponent(c.Entity.Id, token)));

            e.HasRepository<ProductRepository>();
            e.AddTransient<IProductRepository, ProductRepository>();
            e.AddTransient<IProductService, ProductRepository>();
        });
        services.Services.AddTransient<ProductSearchContentRefresher>();
        return services;
    }
}
