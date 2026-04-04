using Microsoft.Extensions.DependencyInjection;
using PIM.Core.Abstractions;
using PIM.Data;
using PIM.Models.Catalog.Products;
using PIM.Services.Entities.Catalog.Products;
using Xunit;

namespace PIM.API.Tests.Catalog;

/// <summary>
/// Tests that <see cref="ProductProcessor"/> selects the correct code path based on
/// <see cref="ProductIncludes"/>. Price-selection edge-cases live in PricingExtensionsTests.
/// </summary>
[Collection("Integration")]
public class ProductProcessorTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    private static readonly DateTime RefDate = new(2026, 3, 14, 12, 0, 0, DateTimeKind.Utc);

    // ── No price-related includes ──────────────────────────────────────────────

    [Fact]
    public async Task None_Include_Does_Not_Change_Price()
    {
        using var scope = fixture.CreateScope();
        var product = new Product { Price = 42m };

        var processor = scope.ServiceProvider.GetRequiredService<ProductProcessor>();
        await processor.Process([product], ProductIncludes.None);

        Assert.Equal(42m, product.Price);
    }

    [Fact]
    public async Task Unrelated_Include_Does_Not_Change_Price()
    {
        using var scope = fixture.CreateScope();
        var product = new Product { Price = 42m };

        var processor = scope.ServiceProvider.GetRequiredService<ProductProcessor>();
        await processor.Process([product], ProductIncludes.Facets);

        Assert.Equal(42m, product.Price);
    }

    // ── ProductIncludes.Price (DB path) ─────────────────────────────────────

    [Fact]
    public async Task Price_Include_Fetches_Price_From_Db()
    {
        using var scope = fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PimDbContext>();

        var product = new Product
        {
            Title = "DB Price Product",
            Prices = [new ProductPricePeriod { Price = 9.99m }]
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var target = new Product { Id = product.Id };
        var processor = scope.ServiceProvider.GetRequiredService<ProductProcessor>();
        await processor.Process([target], ProductIncludes.Price);

        Assert.Equal(9.99m, target.Price);
    }

    [Fact]
    public async Task Price_Include_AdjacentBoundary_Selects_Entry_With_Later_EndDate()
    {
        using var scope = fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PimDbContext>();
        var orderContext = scope.ServiceProvider.GetRequiredService<IOrderContext>();
        ((TestFixture.FakeOrderContext)orderContext).OrderDate = RefDate;

        var product = new Product
        {
            Title = "Adjacent Boundary Product",
            Prices =
            [
                new ProductPricePeriod { Price = 8.00m,  StartDate = RefDate.AddDays(-5), EndDate = RefDate              },
                new ProductPricePeriod { Price = 12.00m, StartDate = RefDate,             EndDate = RefDate.AddDays(5)   },
            ]
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var target = new Product { Id = product.Id };
        var processor = scope.ServiceProvider.GetRequiredService<ProductProcessor>();
        await processor.Process([target], ProductIncludes.Price);

        Assert.Equal(12.00m, target.Price);
    }

    // ── ProductIncludes.PricePeriod (in-memory path) ───────────────────────

    [Fact]
    public async Task PricePeriod_Include_WithLoadedPrices_Sets_Price_From_Collection()
    {
        using var scope = fixture.CreateScope();
        var product = new Product
        {
            Prices = [new ProductPricePeriod { Price = 7.50m }]
        };

        var processor = scope.ServiceProvider.GetRequiredService<ProductProcessor>();
        await processor.Process([product], ProductIncludes.PricePeriod);

        Assert.Equal(7.50m, product.Price);
    }

    [Fact]
    public async Task PricePeriod_Include_WithNullPrices_Sets_Price_To_Null()
    {
        using var scope = fixture.CreateScope();
        var product = new Product { Price = 42m, Prices = null };

        var processor = scope.ServiceProvider.GetRequiredService<ProductProcessor>();
        await processor.Process([product], ProductIncludes.PricePeriod);

        Assert.Null(product.Price);
    }

    // ── Flag priority ──────────────────────────────────────────────────────────

    [Fact]
    public async Task PricePeriod_Takes_Priority_Over_Price_Flag()
    {
        using var scope = fixture.CreateScope();
        var product = new Product
        {
            Id = int.MaxValue,
            Prices = [new ProductPricePeriod { Price = 15.00m }]
        };

        var processor = scope.ServiceProvider.GetRequiredService<ProductProcessor>();
        await processor.Process([product], ProductIncludes.Price | ProductIncludes.PricePeriod);

        Assert.Equal(15.00m, product.Price);
    }

    [Fact]
    public async Task All_Includes_Uses_PricePeriod_Path()
    {
        using var scope = fixture.CreateScope();
        var product = new Product
        {
            Id = int.MaxValue,
            Prices = [new ProductPricePeriod { Price = 15.00m }]
        };

        var processor = scope.ServiceProvider.GetRequiredService<ProductProcessor>();
        await processor.Process([product], ProductIncludes.All);

        Assert.Equal(15.00m, product.Price);
    }
}

