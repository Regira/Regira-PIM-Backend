using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.Models;
using Webshop.Data;
using Webshop.Models.Taxonomy.FacetGroups;

namespace Webshop.DependencyInjection.Taxonomy;

public static class FacetGroupServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddFacetGroups(this IEntityServiceCollection<WebshopDbContext> services)
    {
        services.For<FacetGroup, FacetGroupSearchObject, EntitySortBy, FacetGroupIncludes>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so?.FacetId?.Any() == true)
                    query = query.Where(x => x.Facets!.Any(f => so.FacetId.Contains(f.FacetId)));
                return query;
            });
            e.SortBy((query, _) => query.OrderBy(x => x.Title));
            e.Includes((query, includes) =>
            {
                if (includes?.HasFlag(FacetGroupIncludes.Facets) == true)
                    query = query.Include(x => x.Facets!).ThenInclude(x => x.Facet);
                return query;
            });
            e.Related(x => x.Facets);
        });
        return services;
    }
}
