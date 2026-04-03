using PIM.Data;
using PIM.Models.Catalog.Allergens;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Models;

namespace PIM.DependencyInjection.Catalog;

public static class AllergenServiceConfiguration
{
    public static IEntityServiceCollection<PimDbContext> AddAllergens(this IEntityServiceCollection<PimDbContext> services)
    {
        services.For<Allergen, AllergenSearchObject, EntitySortBy, EntityIncludes>(e =>
        {
            e.Filter((query, so) =>
            {
                query = query.FilterCode(so?.Code);
                return query;
            });
            e.SortBy((query, _) => query.OrderBy(x => x.Title));
        });
        return services;
    }
}
