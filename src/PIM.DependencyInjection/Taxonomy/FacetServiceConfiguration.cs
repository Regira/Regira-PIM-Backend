using PIM.Data;
using PIM.Models.Taxonomy.Facets;
using PIM.Services.Entities.Taxonomy;
using PIM.Services.Entities.Taxonomy.Abstractions;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Models;

namespace PIM.DependencyInjection.Taxonomy;

public static class FacetServiceConfiguration
{
    public static IEntityServiceCollection<PimDbContext> AddFacets(this IEntityServiceCollection<PimDbContext> services)
    {
        services.For<Facet, FacetSearchObject, EntitySortBy, FacetIncludes>(e =>
        {
            e.AddFilter<FacetQueryFilter>();
            e.SortBy((query, _) => query.OrderBy(x => x.Title));
            e.AddIncludes<FacetIncludingQueryBuilder>();
            e.Related(x => x.ParentEntities);
            e.Related(x => x.ChildEntities);
            e.Related(x => x.FacetParentGroups);
            e.HasRepository<FacetRepository>();
            e.AddTransient<IFacetRepository, FacetRepository>();
            e.AddTransient<IFacetService, FacetRepository>();
        });
        return services;
    }
}
