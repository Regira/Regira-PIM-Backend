using Webshop.Models.Entities.Catalog.Products;
using Webshop.Services.Entities.Catalog.Products;
using Xunit;

namespace Webshop.API.Tests.Catalog;

public class ProductQueryBuilderTests
{
    private static readonly DateTime RefDate = new(2026, 3, 14, 12, 0, 0, DateTimeKind.Utc);
    private readonly ProductQueryBuilder _builder = new();

    private IEnumerable<Product> Build(ProductSearchObject? so, params Product[] products)
        => _builder.Build(products.AsQueryable(), so);

    private static ProductPriceHistory Price(decimal amount, DateTime? start = null, DateTime? end = null)
        => new() { Price = amount, StartDate = start, EndDate = end };

    // ── Null SearchObject ──────────────────────────────────────────────────────

    [Fact]
    public void NullSearchObject_ReturnsAll()
    {
        var products = new[] { new Product { Title = "A" }, new Product { Title = "B" } };

        Assert.Equal(2, Build(null, products).Count());
    }

    // ── CategoryId ─────────────────────────────────────────────────────────────

    [Fact]
    public void CategoryId_IncludesProductInMatchingCategory()
    {
        var match = new Product { Title = "A", Categories = [new ProductCategory { CategoryId = 1 }] };
        var noMatch = new Product { Title = "B", Categories = [new ProductCategory { CategoryId = 2 }] };

        var result = Build(new ProductSearchObject { CategoryId = [1] }, match, noMatch).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Title);
    }

    [Fact]
    public void CategoryId_MatchesAnyOfMultipleIds()
    {
        var cat1 = new Product { Title = "Cat1", Categories = [new ProductCategory { CategoryId = 1 }] };
        var cat2 = new Product { Title = "Cat2", Categories = [new ProductCategory { CategoryId = 2 }] };
        var cat3 = new Product { Title = "Cat3", Categories = [new ProductCategory { CategoryId = 3 }] };

        var result = Build(new ProductSearchObject { CategoryId = [1, 2] }, cat1, cat2, cat3).ToList();

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, p => p.Title == "Cat3");
    }

    // ── PartId ─────────────────────────────────────────────────────────────────

    [Fact]
    public void PartId_IncludesProductWithAnyMatchingPart()
    {
        var match = new Product { Title = "A", Parts = [new ProductPart { PartId = 10 }] };
        var noMatch = new Product { Title = "B", Parts = [new ProductPart { PartId = 99 }] };

        var result = Build(new ProductSearchObject { PartId = [10] }, match, noMatch).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Title);
    }

    [Fact]
    public void PartId_MatchesAnyOfMultiplePartIds()
    {
        var partA = new Product { Title = "A", Parts = [new ProductPart { PartId = 1 }] };
        var partB = new Product { Title = "B", Parts = [new ProductPart { PartId = 2 }] };
        var partC = new Product { Title = "C", Parts = [new ProductPart { PartId = 3 }] };

        var result = Build(new ProductSearchObject { PartId = [1, 2] }, partA, partB, partC).ToList();

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, p => p.Title == "C");
    }

    // ── AllPartId ──────────────────────────────────────────────────────────────

    [Fact]
    public void AllPartId_IncludesProductWithAllSpecifiedParts()
    {
        var hasAll = new Product
        {
            Title = "A",
            Parts = [new ProductPart { PartId = 1 }, new ProductPart { PartId = 2 }]
        };
        var missingOne = new Product
        {
            Title = "B",
            Parts = [new ProductPart { PartId = 1 }]
        };

        var result = Build(new ProductSearchObject { AllPartId = [1, 2] }, hasAll, missingOne).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Title);
    }

    [Fact]
    public void AllPartId_ExcludesProductMissingAnyRequiredPart()
    {
        var product = new Product { Title = "A", Parts = [new ProductPart { PartId = 1 }] };

        var result = Build(new ProductSearchObject { AllPartId = [1, 2] }, product);

        Assert.Empty(result);
    }

    // ── AllergenId ─────────────────────────────────────────────────────────────

    [Fact]
    public void AllergenId_IncludesProductWithAnyMatchingAllergen()
    {
        var match = new Product { Title = "A", Allergens = [new ProductAllergen { AllergenId = 5 }] };
        var noMatch = new Product { Title = "B", Allergens = [new ProductAllergen { AllergenId = 9 }] };

        var result = Build(new ProductSearchObject { AllergenId = [5] }, match, noMatch).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Title);
    }

    [Fact]
    public void AllergenId_MatchesAnyOfMultipleAllergenIds()
    {
        var a1 = new Product { Title = "A", Allergens = [new ProductAllergen { AllergenId = 1 }] };
        var a2 = new Product { Title = "B", Allergens = [new ProductAllergen { AllergenId = 2 }] };
        var a3 = new Product { Title = "C", Allergens = [new ProductAllergen { AllergenId = 3 }] };

        var result = Build(new ProductSearchObject { AllergenId = [1, 2] }, a1, a2, a3).ToList();

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, p => p.Title == "C");
    }

    // ── MinPrice ───────────────────────────────────────────────────────────────

    [Fact]
    public void MinPrice_IncludesProductWithActivePriceAtOrAboveMinimum()
    {
        var cheap = new Product { Title = "Cheap", Prices = [Price(2m)] };
        var pricy = new Product { Title = "Pricy", Prices = [Price(10m)] };

        var result = Build(new ProductSearchObject { MinPrice = 5m, PriceDate = RefDate }, cheap, pricy).ToList();

        Assert.Single(result);
        Assert.Equal("Pricy", result[0].Title);
    }

    [Fact]
    public void MinPrice_ExcludesProductWhosePriceIsExpired()
    {
        var product = new Product
        {
            Title = "A",
            Prices = [Price(10m, start: RefDate.AddDays(-10), end: RefDate.AddDays(-1))]
        };

        var result = Build(new ProductSearchObject { MinPrice = 5m, PriceDate = RefDate }, product);

        Assert.Empty(result);
    }

    // ── MaxPrice ───────────────────────────────────────────────────────────────

    [Fact]
    public void MaxPrice_IncludesProductWithActivePriceAtOrBelowMaximum()
    {
        var cheap = new Product { Title = "Cheap", Prices = [Price(2m)] };
        var pricy = new Product { Title = "Pricy", Prices = [Price(20m)] };

        var result = Build(new ProductSearchObject { MaxPrice = 10m, PriceDate = RefDate }, cheap, pricy).ToList();

        Assert.Single(result);
        Assert.Equal("Cheap", result[0].Title);
    }

    [Fact]
    public void MaxPrice_ExcludesProductWhosePriceIsExpired()
    {
        var product = new Product
        {
            Title = "A",
            Prices = [Price(2m, start: RefDate.AddDays(-10), end: RefDate.AddDays(-1))]
        };

        var result = Build(new ProductSearchObject { MaxPrice = 10m, PriceDate = RefDate }, product);

        Assert.Empty(result);
    }

    // ── MinPrice + MaxPrice ────────────────────────────────────────────────────

    [Fact]
    public void MinPrice_And_MaxPrice_OnlyIncludesProductsInRange()
    {
        var low = new Product { Title = "Low", Prices = [Price(1m)] };
        var mid = new Product { Title = "Mid", Prices = [Price(5m)] };
        var high = new Product { Title = "High", Prices = [Price(15m)] };

        var result = Build(new ProductSearchObject { MinPrice = 3m, MaxPrice = 10m, PriceDate = RefDate }, low, mid, high).ToList();

        Assert.Single(result);
        Assert.Equal("Mid", result[0].Title);
    }

    // ── PriceDate ──────────────────────────────────────────────────────────────

    [Fact]
    public void PriceDate_UsesSpecifiedDateForPriceActivity()
    {
        var futureStart = RefDate.AddDays(5);
        var product = new Product { Title = "A", Prices = [Price(10m, start: futureStart)] };

        // Before futureStart: price not yet active → excluded
        var excluded = Build(new ProductSearchObject { MinPrice = 5m, PriceDate = RefDate }, product);
        Assert.Empty(excluded);

        // At futureStart: price becomes active → included
        var included = Build(new ProductSearchObject { MinPrice = 5m, PriceDate = futureStart }, product).ToList();
        Assert.Single(included);
    }

    // ── HasPrice ───────────────────────────────────────────────────────────────

    [Fact]
    public void HasPrice_True_IncludesProductWithActivePrice()
    {
        var withPrice = new Product { Title = "A", Prices = [Price(5m)] };
        var withoutPrice = new Product { Title = "B", Prices = [] };

        var result = Build(new ProductSearchObject { HasPrice = true, PriceDate = RefDate }, withPrice, withoutPrice).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Title);
    }

    [Fact]
    public void HasPrice_True_ExcludesProductWithExpiredPrice()
    {
        var product = new Product
        {
            Title = "A",
            Prices = [Price(5m, start: RefDate.AddDays(-10), end: RefDate.AddDays(-1))]
        };

        var result = Build(new ProductSearchObject { HasPrice = true, PriceDate = RefDate }, product);

        Assert.Empty(result);
    }

    [Fact]
    public void HasPrice_False_IncludesProductWithNoActivePrice()
    {
        var expired = new Product
        {
            Title = "A",
            Prices = [Price(5m, start: RefDate.AddDays(-10), end: RefDate.AddDays(-1))]
        };
        var active = new Product { Title = "B", Prices = [Price(5m)] };

        var result = Build(new ProductSearchObject { HasPrice = false, PriceDate = RefDate }, expired, active).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Title);
    }

    [Fact]
    public void HasPrice_False_IncludesProductWithNoPrices()
    {
        var noPrice = new Product { Title = "A", Prices = [] };
        var withPrice = new Product { Title = "B", Prices = [Price(5m)] };

        var result = Build(new ProductSearchObject { HasPrice = false, PriceDate = RefDate }, noPrice, withPrice).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Title);
    }
}
