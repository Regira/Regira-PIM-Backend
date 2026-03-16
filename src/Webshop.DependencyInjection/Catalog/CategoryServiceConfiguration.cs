using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.Models;
using Webshop.Data;
using Webshop.Models.Entities.Classification.Categories;

namespace Webshop.DependencyInjection.Catalog;

public static class CategoryServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddCategories(this IEntityServiceCollection<WebshopDbContext> services)
    {
        services.For<Category, CategorySearchObject, EntitySortBy, CategoryIncludes>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so?.ParentId?.Any() == true)
                    query = query.Where(x => x.ParentEntities!.Any(pe => so.ParentId.Contains(pe.ParentId)));
                if (so?.ChildId?.Any() == true)
                    query = query.Where(x => x.ChildEntities!.Any(ce => so.ChildId.Contains(ce.ChildId)));
                if (so?.IsRoot != null)
                    query = so.IsRoot.Value
                        ? query.Where(x => !x.ParentEntities!.Any())
                        : query.Where(x => x.ParentEntities!.Any());
                return query;
            });
            e.SortBy((query, _) => query.OrderBy(x => x.Title));
            e.Includes((query, includes) =>
            {
                if (includes?.HasFlag(CategoryIncludes.Parents) == true)
                    query = query.Include(x => x.ParentEntities!).ThenInclude(x => x.Parent);
                if (includes?.HasFlag(CategoryIncludes.Children) == true)
                    query = query.Include(x => x.ChildEntities!).ThenInclude(x => x.Child);
                return query;
            });
            e.Related(x => x.ParentEntities);
            e.Related(x => x.ChildEntities);
        });
        return services;
    }
}
