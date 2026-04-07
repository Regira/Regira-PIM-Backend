using PIM.Models.Catalog.Products;
using PIM.Services.Entities.Catalog.Pricing;
using Xunit;

namespace PIM.API.Tests.Catalog;

public class PricingExtensionsTests
{
    private static readonly DateTime RefDate = new(2026, 3, 14, 12, 0, 0, DateTimeKind.Utc);

    private static ProductPricePeriod Price(decimal amount, DateTime? start = null)
        => new() { Price = amount, StartDate = start };

    // ── FindActivePrices (IEnumerable) ─────────────────────────────────────────

    [Fact]
    public void FindActivePrices_BothDatesNull_AlwaysActive()
    {
        // Single price with no date constraints is always active
        var price = Price(5m);

        Assert.Single(new[] { price }.FindActivePrices(RefDate));
    }

    [Fact]
    public void FindActivePrices_StartDateSet_NoEndDate_IsActiveAfterStart()
    {
        var prices = new[]
        {
            Price(5m, start: RefDate.AddDays(-1)), // active
            Price(9m, start: RefDate.AddDays(1)),  // future
        };

        var result = prices.FindActivePrices(RefDate).ToList();

        Assert.Single(result);
        Assert.Equal(5m, result[0].Price);
    }

    [Fact]
    public void FindActivePrices_StartDate_Equals_RefDate_IsActive()
    {
        // Lower bound is inclusive (StartDate <= RefDate)
        var price = Price(6m, start: RefDate);

        Assert.Single(new[] { price }.FindActivePrices(RefDate));
    }

    [Fact]
    public void FindActivePrices_AdjacentEntries_BothActiveAtSharedBoundary()
    {
        // Both prices have StartDate <= RefDate, so both are active
        var priceA = Price(8m,  start: RefDate.AddDays(-5));
        var priceB = Price(12m, start: RefDate);

        var result = new[] { priceA, priceB }.FindActivePrices(RefDate).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void FindActivePrices_FuturePrice_NotReturned()
    {
        var price = Price(9m, start: RefDate.AddDays(1));

        Assert.Empty(new[] { price }.FindActivePrices(RefDate));
    }

    // ── FilterActivePrices (IQueryable) ────────────────────────────────────────

    [Fact]
    public void FilterActivePrices_ActivePrice_IsReturned()
    {
        var prices = new[]
        {
            Price(10m, start: RefDate.AddDays(-1)), // active
            Price(20m, start: RefDate.AddDays(1)),  // future, not active
        }.AsQueryable();

        var result = prices.FilterActivePrices(RefDate).ToList();

        Assert.Single(result);
        Assert.Equal(10m, result[0].Price);
    }

    [Fact]
    public void FilterActivePrices_BothDatesNull_AlwaysActive()
    {
        var prices = new[] { Price(7m) }.AsQueryable();

        Assert.Single(prices.FilterActivePrices(RefDate));
    }

    [Fact]
    public void FilterActivePrices_AdjacentEntries_BothActiveAtSharedBoundary()
    {
        // Both prices have StartDate <= RefDate, so both are active through the queryable path
        var priceA = Price(8m,  start: RefDate.AddDays(-5));
        var priceB = Price(12m, start: RefDate);

        var result = new[] { priceA, priceB }.AsQueryable().FilterActivePrices(RefDate).ToList();

        Assert.Equal(2, result.Count);
    }

    // ── FindCurrentPrice ───────────────────────────────────────────────────────

    [Fact]
    public void FindCurrentPrice_MultipleActivePrices_ReturnsMostRecentByStartDate()
    {
        // Overlapping ranges are prevented by validation but may still exist in edge cases
        var product = new Product
        {
            Prices =
            [
                new ProductPricePeriod { Price = 8m,  StartDate = RefDate.AddDays(-5) }, // active, older
                new ProductPricePeriod { Price = 12m, StartDate = RefDate.AddDays(-2) }, // active, more recent
            ]
        };

        var result = product.FindActivePrice(RefDate);

        Assert.NotNull(result);
        Assert.Equal(12m, result.Price);
    }

    [Fact]
    public void FindCurrentPrice_ReturnsActivePrice_IgnoresFuturePrice()
    {
        var product = new Product
        {
            Prices =
            [
                new ProductPricePeriod { Price = 5m,  StartDate = RefDate.AddDays(-10) }, // active
                new ProductPricePeriod { Price = 12m, StartDate = RefDate.AddDays(3)   }, // future, not active
            ]
        };

        var result = product.FindActivePrice(RefDate);

        Assert.NotNull(result);
        Assert.Equal(5m, result.Price);
    }

    [Fact]
    public void FindCurrentPrice_NoActivePrice_ReturnsNull()
    {
        var product = new Product
        {
            Prices = [new ProductPricePeriod { Price = 5m, StartDate = RefDate.AddDays(1) }] // future, not active
        };

        Assert.Null(product.FindActivePrice(RefDate));
    }

    [Fact]
    public void FindCurrentPrice_PricesIsNull_ReturnsNull()
    {
        var product = new Product { Prices = null };

        Assert.Null(product.FindActivePrice(RefDate));
    }
}
