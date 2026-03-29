using Webshop.Models.Catalog.Articles;
using Webshop.Services.Entities.Catalog.Articles;
using Xunit;

namespace Webshop.API.Tests.Catalog;

public class ArticleQueryBuilderTests
{
    private static readonly DateTime RefDate = new(2026, 3, 14, 12, 0, 0, DateTimeKind.Utc);
    private readonly ArticleQueryFilter _builder = new();

    private IEnumerable<Article> Build(ArticleSearchObject? so, params Article[] articles)
        => _builder.Build(articles.AsQueryable(), so);

    private static ArticlePricePeriod Price(decimal amount, DateTime? start = null, DateTime? end = null)
        => new() { Price = amount, StartDate = start, EndDate = end };

    // ── Null SearchObject ──────────────────────────────────────────────────────

    [Fact]
    public void NullSearchObject_ReturnsAll()
    {
        var articles = new[] { new Article { Title = "A" }, new Article { Title = "B" } };

        Assert.Equal(2, Build(null, articles).Count());
    }

    // ── CategoryId ─────────────────────────────────────────────────────────────

    [Fact]
    public void CategoryId_IncludesArticleInMatchingCategory()
    {
        var match = new Article { Title = "A", Facets = [new ArticleFacet { FacetId = 1 }] };
        var noMatch = new Article { Title = "B", Facets = [new ArticleFacet { FacetId = 2 }] };

        var result = Build(new ArticleSearchObject { FacetId = [1] }, match, noMatch).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Title);
    }

    [Fact]
    public void CategoryId_MatchesAnyOfMultipleIds()
    {
        var cat1 = new Article { Title = "Cat1", Facets = [new ArticleFacet { FacetId = 1 }] };
        var cat2 = new Article { Title = "Cat2", Facets = [new ArticleFacet { FacetId = 2 }] };
        var cat3 = new Article { Title = "Cat3", Facets = [new ArticleFacet { FacetId = 3 }] };

        var result = Build(new ArticleSearchObject { FacetId = [1, 2] }, cat1, cat2, cat3).ToList();

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, a => a.Title == "Cat3");
    }

    // ── ComponentId ──────────────────────────────────────────────────────────────

    [Fact]
    public void ComponentId_IncludesArticleWithAnyMatchingComponent()
    {
        var match = new Article { Title = "A", Components = [new ArticleComponent { ComponentId = 10 }] };
        var noMatch = new Article { Title = "B", Components = [new ArticleComponent { ComponentId = 99 }] };

        var result = Build(new ArticleSearchObject { ComponentId = [10] }, match, noMatch).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Title);
    }

    [Fact]
    public void ComponentId_MatchesAnyOfMultipleComponentIds()
    {
        var compA = new Article { Title = "A", Components = [new ArticleComponent { ComponentId = 1 }] };
        var compB = new Article { Title = "B", Components = [new ArticleComponent { ComponentId = 2 }] };
        var compC = new Article { Title = "C", Components = [new ArticleComponent { ComponentId = 3 }] };

        var result = Build(new ArticleSearchObject { ComponentId = [1, 2] }, compA, compB, compC).ToList();

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, a => a.Title == "C");
    }

    // ── AllComponentId ─────────────────────────────────────────────────────────

    [Fact]
    public void AllComponentId_IncludesArticleWithAllSpecifiedComponents()
    {
        var hasAll = new Article
        {
            Title = "A",
            Components = [new ArticleComponent { ComponentId = 1 }, new ArticleComponent { ComponentId = 2 }]
        };
        var missingOne = new Article
        {
            Title = "B",
            Components = [new ArticleComponent { ComponentId = 1 }]
        };

        var result = Build(new ArticleSearchObject { AllComponentId = [1, 2] }, hasAll, missingOne).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Title);
    }

    [Fact]
    public void AllComponentId_ExcludesArticleMissingAnyRequiredComponent()
    {
        var article = new Article { Title = "A", Components = [new ArticleComponent { ComponentId = 1 }] };

        var result = Build(new ArticleSearchObject { AllComponentId = [1, 2] }, article);

        Assert.Empty(result);
    }

    // ── SupplierId ───────────────────────────────────────────────────────────────

    [Fact]
    public void SupplierId_IncludesArticleWithMatchingSupplier()
    {
        var match = new Article { Title = "A", Suppliers = [new ArticleSupplier { SupplierId = 5 }] };
        var noMatch = new Article { Title = "B", Suppliers = [new ArticleSupplier { SupplierId = 9 }] };

        var result = Build(new ArticleSearchObject { SupplierId = [5] }, match, noMatch).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Title);
    }

    [Fact]
    public void SupplierId_MatchesAnyOfMultipleSupplierIds()
    {
        var s1 = new Article { Title = "A", Suppliers = [new ArticleSupplier { SupplierId = 1 }] };
        var s2 = new Article { Title = "B", Suppliers = [new ArticleSupplier { SupplierId = 2 }] };
        var s3 = new Article { Title = "C", Suppliers = [new ArticleSupplier { SupplierId = 3 }] };

        var result = Build(new ArticleSearchObject { SupplierId = [1, 2] }, s1, s2, s3).ToList();

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, a => a.Title == "C");
    }

    // ── MinPrice ───────────────────────────────────────────────────────────────

    [Fact]
    public void MinPrice_IncludesArticleWithActivePriceAtOrAboveMinimum()
    {
        var cheap = new Article { Title = "Cheap", Prices = [Price(2m)] };
        var pricy = new Article { Title = "Pricy", Prices = [Price(10m)] };

        var result = Build(new ArticleSearchObject { MinPrice = 5m, PriceDate = RefDate }, cheap, pricy).ToList();

        Assert.Single(result);
        Assert.Equal("Pricy", result[0].Title);
    }

    [Fact]
    public void MinPrice_ExcludesArticleWhosePriceIsExpired()
    {
        var article = new Article
        {
            Title = "A",
            Prices = [Price(10m, start: RefDate.AddDays(-10), end: RefDate.AddDays(-1))]
        };

        var result = Build(new ArticleSearchObject { MinPrice = 5m, PriceDate = RefDate }, article);

        Assert.Empty(result);
    }

    // ── MaxPrice ───────────────────────────────────────────────────────────────

    [Fact]
    public void MaxPrice_IncludesArticleWithActivePriceAtOrBelowMaximum()
    {
        var cheap = new Article { Title = "Cheap", Prices = [Price(2m)] };
        var pricy = new Article { Title = "Pricy", Prices = [Price(20m)] };

        var result = Build(new ArticleSearchObject { MaxPrice = 10m, PriceDate = RefDate }, cheap, pricy).ToList();

        Assert.Single(result);
        Assert.Equal("Cheap", result[0].Title);
    }

    // ── MinPrice + MaxPrice ────────────────────────────────────────────────────

    [Fact]
    public void MinPrice_And_MaxPrice_OnlyIncludesArticlesInRange()
    {
        var low = new Article { Title = "Low", Prices = [Price(1m)] };
        var mid = new Article { Title = "Mid", Prices = [Price(5m)] };
        var high = new Article { Title = "High", Prices = [Price(15m)] };

        var result = Build(new ArticleSearchObject { MinPrice = 3m, MaxPrice = 10m, PriceDate = RefDate }, low, mid, high).ToList();

        Assert.Single(result);
        Assert.Equal("Mid", result[0].Title);
    }

    // ── HasPrice ───────────────────────────────────────────────────────────────

    [Fact]
    public void HasPrice_True_IncludesArticleWithActivePrice()
    {
        var withPrice = new Article { Title = "A", Prices = [Price(5m)] };
        var withoutPrice = new Article { Title = "B", Prices = [] };

        var result = Build(new ArticleSearchObject { HasPrice = true, PriceDate = RefDate }, withPrice, withoutPrice).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Title);
    }

    [Fact]
    public void HasPrice_False_IncludesArticleWithNoActivePrice()
    {
        var expired = new Article
        {
            Title = "A",
            Prices = [Price(5m, start: RefDate.AddDays(-10), end: RefDate.AddDays(-1))]
        };
        var active = new Article { Title = "B", Prices = [Price(5m)] };

        var result = Build(new ArticleSearchObject { HasPrice = false, PriceDate = RefDate }, expired, active).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Title);
    }
}
