using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Webshop.Models.Entities.Catalog.Articles;
using Webshop.Services.Entities.Catalog.Pricing;

namespace Webshop.Services.Entities.Catalog.Articles;

public interface IArticleValidateManager : IEntityService<Article, ArticleSearchObject, ArticleSortBy, ArticleIncludes>
{
    void Validate(Article item);
}

public class ArticleValidateManager(IEntityRepository<Article, ArticleSearchObject, ArticleSortBy, ArticleIncludes> service)
    : EntityWrappingServiceBase<Article, ArticleSearchObject, ArticleSortBy, ArticleIncludes>(service), IArticleValidateManager
{
    public override Task Add(Article item)
    {
        Validate(item);
        return base.Add(item);
    }
    public override Task<Article?> Modify(Article item)
    {
        Validate(item);
        return base.Modify(item);
    }
    public override Task Save(Article item)
    {
        Validate(item);
        return base.Save(item);
    }

    public void Validate(Article item)
    {
        var errors = new Dictionary<string, string>();

        // Pricing
        var priceErrors = item.Validate<Article, ArticlePricePeriod>();
        foreach (var error in priceErrors)
        {
            errors.Add(error.Key, error.Value);
        }

        // Components
        if (item.Components?.Any(ac => ac.Quantity <= 0) == true)
        {
            errors.Add("Components", "Article components must have a quantity greater than zero.");
        }

        if (errors.Any())
        {
            throw new EntityInputException<Article>("Validation failed")
            {
                InputErrors = errors,
                Item = item
            };
        }
    }
}
