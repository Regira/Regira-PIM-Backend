using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Models;
using Webshop.Data;
using Webshop.Models.Entities.Catalog.Allergens;

namespace Webshop.DependencyInjection.Catalog;

public static class AllergenServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddAllergens(this IEntityServiceCollection<WebshopDbContext> services)
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
