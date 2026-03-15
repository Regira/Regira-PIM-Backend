using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Webshop.Data;
using Webshop.Models.Entities.Catalog.Articles;
using Webshop.Services.Entities.Catalog.Articles;

namespace Webshop.DependencyInjection.Catalog;

public static class ArticleServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddArticles(this IEntityServiceCollection<WebshopDbContext> services)
    {
        services.For<Article, ArticleSearchObject, ArticleSortBy, ArticleIncludes>(e =>
        {
            e.AddQueryFilter<ArticleQueryBuilder>();
            e.SortBy<ArticleSortingQueryBuilder>();
            e.Includes<ArticleIncludingQueryBuilder>();
            e.Related(x => x.Categories);
            e.Related(x => x.Components);
            e.Related(x => x.AllowedComponentAdditions);
            e.Related(x => x.Suppliers);
            e.Related(x => x.Prices);
            e.Process<ArticleProcessor>();
            e.AddNormalizer<ArticleNormalizer>();
            e.UseEntityService<ArticleValidateManager>();
        });
        return services;
    }
}
