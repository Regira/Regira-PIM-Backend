using PIM.Data;
using PIM.Models.Catalog.Articles;
using PIM.Services.Entities.Catalog.Articles;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;

namespace PIM.DependencyInjection.Catalog;

public static class ArticleServiceConfiguration
{
    public static IEntityServiceCollection<PimDbContext> AddArticles(this IEntityServiceCollection<PimDbContext> services)
    {
        services.For<Article, ArticleSearchObject, ArticleSortBy, ArticleIncludes>(e =>
        {
            e.AddFilter<ArticleQueryFilter>();
            e.AddSortBy<ArticleSortingQueryBuilder>();
            e.AddIncludes<ArticleIncludingQueryBuilder>();
            e.Related(x => x.Facets);
            e.Related(x => x.Components);
            e.Related(x => x.AllowedComponentAdditions);
            e.Related(x => x.Suppliers);
            e.Related(x => x.Prices);
            e.AddProcessor<ArticleProcessor>();
            e.AddNormalizer<ArticleNormalizer>();
            e.UseEntityService<ArticleValidateManager>();
        });
        return services;
    }
}
