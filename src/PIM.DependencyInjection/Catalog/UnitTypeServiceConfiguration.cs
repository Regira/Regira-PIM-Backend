using PIM.Data;
using PIM.Models.Catalog.UnitTypes;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;

namespace PIM.DependencyInjection.Catalog;

public static class UnitTypeServiceConfiguration
{
    public static IEntityServiceCollection<PimDbContext> AddUnitTypes(this IEntityServiceCollection<PimDbContext> services)
    {
        services.For<UnitType>(e => e.SortBy(query => query.OrderBy(x => x.Title)));
        return services;
    }
}