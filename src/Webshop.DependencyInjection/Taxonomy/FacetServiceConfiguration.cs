using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.Models;
using Webshop.Data;
using Webshop.Models.Taxonomy.Facets;

namespace Webshop.DependencyInjection.Taxonomy;

public static class FacetServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddFacets(this IEntityServiceCollection<WebshopDbContext> services)
    {
        services.For<Facet, FacetSearchObject, EntitySortBy, FacetIncludes>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so?.ParentId?.Any() == true)
                    query = query.Where(x => x.ParentEntities!.Any(pe => so.ParentId.Contains(pe.ParentId)));
                if (so?.ChildId?.Any() == true)
                    query = query.Where(x => x.ChildEntities!.Any(ce => so.ChildId.Contains(ce.ChildId)));
                if (so?.FacetGroupId?.Any() == true)
                    query = query.Where(x => x.FacetGroups!.Any(fg => so.FacetGroupId.Contains(fg.FacetGroupId)));
                if (so?.IsRoot != null)
                    query = so.IsRoot.Value
                        ? query.Where(x => !x.ParentEntities!.Any())
                        : query.Where(x => x.ParentEntities!.Any());
                return query;
            });
            e.SortBy((query, _) => query.OrderBy(x => x.Title));
            e.Includes((query, includes) =>
            {
                if (includes?.HasFlag(FacetIncludes.Parents) == true)
                    query = query.Include(x => x.ParentEntities!).ThenInclude(x => x.Parent);
                if (includes?.HasFlag(FacetIncludes.Children) == true)
                    query = query.Include(x => x.ChildEntities!).ThenInclude(x => x.Child);
                if (includes?.HasFlag(FacetIncludes.FacetGroups) == true)
                    query = query.Include(x => x.FacetGroups!).ThenInclude(x => x.FacetGroup);
                return query;
            });
            e.Related(x => x.ParentEntities);
            e.Related(x => x.ChildEntities);
            e.Related(x => x.FacetGroups);
        });
        return services;
    }
}
