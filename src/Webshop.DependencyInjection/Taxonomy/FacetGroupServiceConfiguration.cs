using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.Models;
using Webshop.Data;
using Webshop.Models.Taxonomy.FacetGroups;
using Webshop.Services.Entities.Taxonomy;

namespace Webshop.DependencyInjection.Taxonomy;

public static class FacetGroupServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddFacetGroups(this IEntityServiceCollection<WebshopDbContext> services)
    {
        services.For<FacetGroup, FacetGroupSearchObject, EntitySortBy, FacetGroupIncludes>(e =>
        {
            e.AddFilter<FacetGroupQueryFilter>();
            e.SortBy((query, _) => query.OrderBy(x => x.Title));
            e.AddIncludes<FacetGroupIncludingQueryBuilder>();
            e.Related(x => x.ParentFacets);
            e.Related(x => x.ChildFacets);
        });
        return services;
    }
}
