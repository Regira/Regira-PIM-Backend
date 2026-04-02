using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.Models;
using Webshop.Data;
using Webshop.Models.Taxonomy.Facets;
using Webshop.Services.Entities.Taxonomy;

namespace Webshop.DependencyInjection.Taxonomy;

public static class FacetServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddFacets(this IEntityServiceCollection<WebshopDbContext> services)
    {
        services.For<Facet, FacetSearchObject, EntitySortBy, FacetIncludes>(e =>
        {
            e.AddFilter<FacetQueryFilter>();
            e.SortBy((query, _) => query.OrderBy(x => x.Title));
            e.AddIncludes<FacetIncludingQueryBuilder>();
            e.Related(x => x.ParentEntities);
            e.Related(x => x.ChildEntities);
            e.Related(x => x.FacetParentGroups);
        });
        return services;
    }
}
