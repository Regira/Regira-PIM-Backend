using PIM.Data;
using PIM.Models.Taxonomy.FacetGroups;
using PIM.Services.Entities.Taxonomy;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.Models;

namespace PIM.DependencyInjection.Taxonomy;

public static class FacetGroupServiceConfiguration
{
    public static IEntityServiceCollection<PimDbContext> AddFacetGroups(this IEntityServiceCollection<PimDbContext> services)
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
