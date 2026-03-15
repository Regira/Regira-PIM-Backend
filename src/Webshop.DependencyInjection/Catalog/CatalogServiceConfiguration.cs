using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Webshop.Data;

namespace Webshop.DependencyInjection.Catalog;


public static class CatalogServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddCatalog(this IEntityServiceCollection<WebshopDbContext> services)
    {
        return services
            .AddCategories()
            .AddArticles()
            .AddProducts()
            .AddParts()
            .AddAllergens()
            .AddUnitTypes();
    }
}