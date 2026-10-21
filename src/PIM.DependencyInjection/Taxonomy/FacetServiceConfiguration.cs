using PIM.Data;
using PIM.DependencyInjection.Catalog;
using PIM.Models.Taxonomy.Facets;
using PIM.Services.Entities.Taxonomy;
using PIM.Services.Entities.Taxonomy.Abstractions;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Models;
using Regira.Entities.Reactors.Abstractions;

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
            e.AddValidator<FacetHierarchyValidator>();
            // A renamed, archived or restored facet changes the search text of the products it is linked to
            e.React(c => c.Kind == EntityChangeKind.Modified && (c.HasChanged(x => x.Title) || c.HasChanged(x => x.IsArchived)),
                (c, sp, _) => ProductSearchContentReactions.Refresh(sp, (refresher, token) => refresher.ForFacet(c.Entity.Id, token)));
            e.HasRepository<FacetRepository>();
            e.AddTransient<IFacetRepository, FacetRepository>();
            e.AddTransient<IFacetService, FacetRepository>();
        });
        return services;
    }
}
