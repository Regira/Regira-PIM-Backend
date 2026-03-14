using Microsoft.Extensions.DependencyInjection;
using Webshop.Data;
using Webshop.Models.Contexts;
using Webshop.Models.Entities.Catalog.Products;
using Webshop.Services.Entities.Catalog.Products;
using Xunit;

namespace Webshop.API.Tests.Catalog;

/// <summary>
/// Tests that <see cref="ProductProcessor"/> selects the correct code path based on
/// <see cref="ProductIncludes"/>. Price-selection edge-cases live in PricingExtensionsTests.
/// </summary>
[Collection("Integration")]
public class ProductProcessorTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    private static readonly DateTime RefDate = new(2026, 3, 14, 12, 0, 0, DateTimeKind.Utc);

    private static ProductProcessor CreateProcessor(WebshopDbContext db, DateTime? orderDate = null)
        => new(db, new FakeOrderContext { OrderDate = orderDate ?? RefDate });

    private static WebshopDbContext GetDb(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<WebshopDbContext>();

    // ── No price-related includes ──────────────────────────────────────────────

    [Fact]
    public async Task None_Include_Does_Not_Change_Price()
    {
        using var scope = fixture.CreateScope();
        var product = new Product { Price = 42m };

        await CreateProcessor(GetDb(scope)).Process([product], ProductIncludes.None);

        Assert.Equal(42m, product.Price);
    }

    [Fact]
    public async Task Unrelated_Include_Does_Not_Change_Price()
    {
        using var scope = fixture.CreateScope();
        var product = new Product { Price = 42m };

        await CreateProcessor(GetDb(scope)).Process([product], ProductIncludes.Categories);

        Assert.Equal(42m, product.Price);
    }

    // ── ProductIncludes.Price (DB path) ───────────────────────────────────────

    [Fact]
    public async Task Price_Include_Fetches_Price_From_Db()
    {
        using var scope = fixture.CreateScope();
        var db = GetDb(scope);

        var product = new Product
        {
            Title = "DB Price Product",
            Prices = [new ProductPriceHistory { Price = 9.99m }] // always active (no dates)
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var target = new Product { Id = product.Id }; // detached — Prices not loaded
        await CreateProcessor(db).Process([target], ProductIncludes.Price);

        Assert.Equal(9.99m, target.Price);
    }

    [Fact]
    public async Task Price_Include_AdjacentBoundary_Selects_Entry_With_Later_EndDate()
    {
        // A.EndDate == B.StartDate == RefDate → both active at RefDate.
        // Processor sorts by EndDate ?? StartDate DESC, so B (later EndDate) wins.
        using var scope = fixture.CreateScope();
        var db = GetDb(scope);

        var product = new Product
        {
            Title = "Adjacent Boundary Product",
            Prices =
            [
                new ProductPriceHistory { Price = 8.00m,  StartDate = RefDate.AddDays(-5), EndDate = RefDate              },
                new ProductPriceHistory { Price = 12.00m, StartDate = RefDate,             EndDate = RefDate.AddDays(5)   },
            ]
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var target = new Product { Id = product.Id };
        await CreateProcessor(db).Process([target], ProductIncludes.Price);

        Assert.Equal(12.00m, target.Price);
    }

    // ── ProductIncludes.PriceHistory (in-memory path) ─────────────────────────

    [Fact]
    public async Task PriceHistory_Include_WithLoadedPrices_Sets_Price_From_Collection()
    {
        using var scope = fixture.CreateScope();
        var product = new Product
        {
            Prices = [new ProductPriceHistory { Price = 7.50m }] // always active (no dates)
        };

        await CreateProcessor(GetDb(scope)).Process([product], ProductIncludes.PriceHistory);

        Assert.Equal(7.50m, product.Price);
    }

    [Fact]
    public async Task PriceHistory_Include_WithNullPrices_Sets_Price_To_Null()
    {
        using var scope = fixture.CreateScope();
        var product = new Product { Price = 42m, Prices = null };

        await CreateProcessor(GetDb(scope)).Process([product], ProductIncludes.PriceHistory);

        Assert.Null(product.Price);
    }

    // ── Flag priority ──────────────────────────────────────────────────────────

    [Fact]
    public async Task PriceHistory_Takes_Priority_Over_Price_Flag()
    {
        // Use an ID not in DB: if the DB path were taken the price would be null.
        // PriceHistory path uses the in-memory Prices collection → must return 15m.
        using var scope = fixture.CreateScope();
        var product = new Product
        {
            Id = int.MaxValue,
            Prices = [new ProductPriceHistory { Price = 15.00m }]
        };

        await CreateProcessor(GetDb(scope)).Process([product], ProductIncludes.Price | ProductIncludes.PriceHistory);

        Assert.Equal(15.00m, product.Price);
    }

    [Fact]
    public async Task All_Includes_Uses_PriceHistory_Path()
    {
        using var scope = fixture.CreateScope();
        var product = new Product
        {
            Id = int.MaxValue,
            Prices = [new ProductPriceHistory { Price = 15.00m }]
        };

        await CreateProcessor(GetDb(scope)).Process([product], ProductIncludes.All);

        Assert.Equal(15.00m, product.Price);
    }
}

file sealed class FakeOrderContext : IOrderContext
{
    public int? CustomerId => null;
    public int? OrganizationId => null;
    public DateTime? OrderDate { get; set; }
}
