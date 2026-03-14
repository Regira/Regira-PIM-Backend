using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Webshop.Data;
using Webshop.Models.Entities.Catalog.Parts;
using Webshop.Services.Entities.Catalog.Parts;

namespace Webshop.DependencyInjection.Catalog;

public static class PartServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddParts(this IEntityServiceCollection<WebshopDbContext> services)
    {
        services.For<Part, PartSearchObject, PartSortBy, PartIncludes>(e =>
        {
            e.AddQueryFilter<PartQueryBuilder>();
            e.SortBy<PartSortingQueryBuilder>();
            e.Includes<PartIncludingQueryBuilder>();

            e.Related(x => x.Prices);
            e.Process<PartProcessor>();

            e.AddTransient<IPartValidateManager, PartValidateManager>();
            e.UseEntityService<PartValidateManager>();
        });
        return services;
    }
}
