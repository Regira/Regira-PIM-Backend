using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Webshop.Data;
using Webshop.Models.Entities.Catalog.UnitTypes;

namespace Webshop.DependencyInjection.Catalog;

public static class UnitTypeServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddUnitTypes(this IEntityServiceCollection<WebshopDbContext> services)
    {
        services.For<UnitType>(e => e.SortBy(query => query.OrderBy(x => x.Title)));
        return services;
    }
}