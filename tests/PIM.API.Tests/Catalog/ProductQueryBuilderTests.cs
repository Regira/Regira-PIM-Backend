using PIM.Models.Catalog.Products;
using PIM.Services.Entities.Catalog.Products;
using Xunit;

namespace PIM.API.Tests.Catalog;

public class ProductQueryBuilderTests
{
    private static readonly DateTime RefDate = new(2026, 3, 14, 12, 0, 0, DateTimeKind.Utc);
    private readonly ProductQueryFilter _builder = new();

    private IEnumerable<Product> Build(ProductSearchObject? so, params Product[] products)
        => _builder.Build(products.AsQueryable(), so);

    private static ProductPricePeriod Price(decimal amount, DateTime? start = null)
        => new() { Price = amount, StartDate = start };

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
        var match = new Product { Title = "A", Facets = [new ProductFacet { FacetId = 1 }] };
        var noMatch = new Product { Title = "B", Facets = [new ProductFacet { FacetId = 2 }] };

        var result = Build(new ProductSearchObject { FacetId = [1] }, match, noMatch).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Title);
    }

    [Fact]
    public void CategoryId_MatchesAnyOfMultipleIds()
    {
        var cat1 = new Product { Title = "Cat1", Facets = [new ProductFacet { FacetId = 1 }] };
        var cat2 = new Product { Title = "Cat2", Facets = [new ProductFacet { FacetId = 2 }] };
        var cat3 = new Product { Title = "Cat3", Facets = [new ProductFacet { FacetId = 3 }] };

        var result = Build(new ProductSearchObject { FacetId = [1, 2] }, cat1, cat2, cat3).ToList();

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, a => a.Title == "Cat3");
    }

    // ── ComponentId ──────────────────────────────────────────────────────────────

    [Fact]
    public void ComponentId_IncludesProductWithAnyMatchingComponent()
    {
        var match = new Product { Title = "A", Components = [new ProductComponent { ComponentId = 10 }] };
        var noMatch = new Product { Title = "B", Components = [new ProductComponent { ComponentId = 99 }] };

        var result = Build(new ProductSearchObject { ComponentId = [10] }, match, noMatch).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Title);
    }

    [Fact]
    public void ComponentId_MatchesAnyOfMultipleComponentIds()
    {
        var compA = new Product { Title = "A", Components = [new ProductComponent { ComponentId = 1 }] };
        var compB = new Product { Title = "B", Components = [new ProductComponent { ComponentId = 2 }] };
        var compC = new Product { Title = "C", Components = [new ProductComponent { ComponentId = 3 }] };

        var result = Build(new ProductSearchObject { ComponentId = [1, 2] }, compA, compB, compC).ToList();

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, a => a.Title == "C");
    }

    // ── AllComponentId ─────────────────────────────────────────────────────────

    [Fact]
    public void AllComponentId_IncludesProductWithAllSpecifiedComponents()
    {
        var hasAll = new Product
        {
            Title = "A",
            Components = [new ProductComponent { ComponentId = 1 }, new ProductComponent { ComponentId = 2 }]
        };
        var missingOne = new Product
        {
            Title = "B",
            Components = [new ProductComponent { ComponentId = 1 }]
        };

        var result = Build(new ProductSearchObject { AllComponentId = [1, 2] }, hasAll, missingOne).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Title);
    }

    [Fact]
    public void AllComponentId_ExcludesProductMissingAnyRequiredComponent()
    {
        var product = new Product { Title = "A", Components = [new ProductComponent { ComponentId = 1 }] };

        var result = Build(new ProductSearchObject { AllComponentId = [1, 2] }, product);

        Assert.Empty(result);
    }

    // ── SupplierId ───────────────────────────────────────────────────────────────

    [Fact]
    public void SupplierId_IncludesProductWithMatchingSupplier()
    {
        var match = new Product { Title = "A", Suppliers = [new ProductSupplier { SupplierId = 5 }] };
        var noMatch = new Product { Title = "B", Suppliers = [new ProductSupplier { SupplierId = 9 }] };

        var result = Build(new ProductSearchObject { SupplierId = [5] }, match, noMatch).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Title);
    }

    [Fact]
    public void SupplierId_MatchesAnyOfMultipleSupplierIds()
    {
        var s1 = new Product { Title = "A", Suppliers = [new ProductSupplier { SupplierId = 1 }] };
        var s2 = new Product { Title = "B", Suppliers = [new ProductSupplier { SupplierId = 2 }] };
        var s3 = new Product { Title = "C", Suppliers = [new ProductSupplier { SupplierId = 3 }] };

        var result = Build(new ProductSearchObject { SupplierId = [1, 2] }, s1, s2, s3).ToList();

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, a => a.Title == "C");
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
    public void MinPrice_ExcludesProductWithNoActivePrice()
    {
        var product = new Product
        {
            Title = "A",
            Prices = [Price(10m, start: RefDate.AddDays(1))] // future, not yet active
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
    public void HasPrice_False_IncludesProductWithNoActivePrice()
    {
        var future = new Product
        {
            Title = "A",
            Prices = [Price(5m, start: RefDate.AddDays(1))] // future, not yet active
        };
        var active = new Product { Title = "B", Prices = [Price(5m)] };

        var result = Build(new ProductSearchObject { HasPrice = false, PriceDate = RefDate }, future, active).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Title);
    }
}
