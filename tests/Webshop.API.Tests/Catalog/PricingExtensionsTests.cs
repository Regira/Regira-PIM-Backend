using Webshop.Models.Catalog.Articles;
using Webshop.Services.Entities.Catalog.Pricing;
using Xunit;

namespace Webshop.API.Tests.Catalog;

public class PricingExtensionsTests
{
    private static readonly DateTime RefDate = new(2026, 3, 14, 12, 0, 0, DateTimeKind.Utc);

    private static ArticlePricePeriod Price(decimal amount, DateTime? start = null, DateTime? end = null)
        => new() { Price = amount, StartDate = start, EndDate = end };

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
    public void FindActivePrices_NoStartDate_EndDateInFuture_IsActive()
    {
        var prices = new[]
        {
            Price(5m, end: RefDate.AddDays(1)),  // active
            Price(9m, end: RefDate.AddDays(-1)), // expired
        };

        var result = prices.FindActivePrices(RefDate).ToList();

        Assert.Single(result);
        Assert.Equal(5m, result[0].Price);
    }

    [Fact]
    public void FindActivePrices_BothDatesSet_InRange_IsActive()
    {
        var prices = new[]
        {
            Price(5m,  start: RefDate.AddDays(-3),  end: RefDate.AddDays(3)),  // active
            Price(9m,  start: RefDate.AddDays(-10), end: RefDate.AddDays(-1)), // expired
            Price(15m, start: RefDate.AddDays(1),   end: RefDate.AddDays(5)),  // future
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
    public void FindActivePrices_EndDate_Equals_RefDate_IsActive()
    {
        // Upper bound is inclusive (EndDate >= RefDate)
        var price = Price(6m, end: RefDate);

        Assert.Single(new[] { price }.FindActivePrices(RefDate));
    }

    [Fact]
    public void FindActivePrices_AdjacentEntries_BothActiveAtSharedBoundary()
    {
        // A.EndDate == B.StartDate == RefDate: both entries are active at RefDate
        var priceA = Price(8m,  start: RefDate.AddDays(-5), end: RefDate);
        var priceB = Price(12m, start: RefDate,             end: RefDate.AddDays(5));

        var result = new[] { priceA, priceB }.FindActivePrices(RefDate).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void FindActivePrices_ExpiredPrice_NotReturned()
    {
        var price = Price(9m, start: RefDate.AddDays(-10), end: RefDate.AddDays(-1));

        Assert.Empty(new[] { price }.FindActivePrices(RefDate));
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
            Price(10m, start: RefDate.AddDays(-1), end: RefDate.AddDays(5)),   // active
            Price(20m, start: RefDate.AddDays(-10), end: RefDate.AddDays(-1)), // expired
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
        // Same boundary rule applies through the queryable (expression-compiled) path
        var priceA = Price(8m,  start: RefDate.AddDays(-5), end: RefDate);
        var priceB = Price(12m, start: RefDate,             end: RefDate.AddDays(5));

        var result = new[] { priceA, priceB }.AsQueryable().FilterActivePrices(RefDate).ToList();

        Assert.Equal(2, result.Count);
    }

    // ── FindCurrentPrice ───────────────────────────────────────────────────────

    [Fact]
    public void FindCurrentPrice_MultipleActivePrices_ReturnsMostRecentByStartDate()
    {
        // Overlapping ranges are prevented by validation but may still exist in edge cases
        var article = new Article
        {
            Prices =
            [
                new ArticlePricePeriod { Price = 8m,  StartDate = RefDate.AddDays(-5), EndDate = RefDate.AddDays(5) }, // active, older
                new ArticlePricePeriod { Price = 12m, StartDate = RefDate.AddDays(-2), EndDate = RefDate.AddDays(8) }, // active, more recent
            ]
        };

        var result = article.FindActivePrice(RefDate);

        Assert.NotNull(result);
        Assert.Equal(12m, result.Price);
    }

    [Fact]
    public void FindCurrentPrice_ReturnsFirstActivePrice()
    {
        var article = new Article
        {
            Prices =
            [
                new ArticlePricePeriod { Price = 5m,  StartDate = RefDate.AddDays(-10), EndDate = RefDate.AddDays(-1) }, // expired
                new ArticlePricePeriod { Price = 12m, StartDate = RefDate.AddDays(-3),  EndDate = RefDate.AddDays(7)  }, // active
            ]
        };

        var result = article.FindActivePrice(RefDate);

        Assert.NotNull(result);
        Assert.Equal(12m, result.Price);
    }

    [Fact]
    public void FindCurrentPrice_NoActivePrice_ReturnsNull()
    {
        var article = new Article
        {
            Prices = [new ArticlePricePeriod { Price = 5m, StartDate = RefDate.AddDays(-10), EndDate = RefDate.AddDays(-1) }]
        };

        Assert.Null(article.FindActivePrice(RefDate));
    }

    [Fact]
    public void FindCurrentPrice_PricesIsNull_ReturnsNull()
    {
        var article = new Article { Prices = null };

        Assert.Null(article.FindActivePrice(RefDate));
    }
}
