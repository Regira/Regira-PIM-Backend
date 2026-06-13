using PIM.Data;
using PIM.DependencyInjection.Taxonomy;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace PIM.DependencyInjection.Catalog;


public static class CatalogServiceConfiguration
{
    public static IEntityServiceCollection<PimDbContext> AddCatalog(this IEntityServiceCollection<PimDbContext> services)
    {
        return services
            .AddFacets()
            .AddFacetGroups()
            .AddProducts()
            .AddUnitTypes();
    }
}