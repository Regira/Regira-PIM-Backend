using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Webshop.Data;
using Webshop.DependencyInjection.Taxonomy;

namespace Webshop.DependencyInjection.Catalog;


public static class CatalogServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddCatalog(this IEntityServiceCollection<WebshopDbContext> services)
    {
        return services
            .AddFacets()
            .AddFacetGroups()
            .AddArticles()
            .AddUnitTypes();
    }
}