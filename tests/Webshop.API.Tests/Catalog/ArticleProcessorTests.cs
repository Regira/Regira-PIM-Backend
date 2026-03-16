using Microsoft.Extensions.DependencyInjection;
using Webshop.Data;
using Webshop.Models.Contexts;
using Webshop.Models.Entities.Catalog.Articles;
using Webshop.Services.Entities.Catalog.Articles;
using Xunit;

namespace Webshop.API.Tests.Catalog;

/// <summary>
/// Tests that <see cref="ArticleProcessor"/> selects the correct code path based on
/// <see cref="ArticleIncludes"/>. Price-selection edge-cases live in PricingExtensionsTests.
/// </summary>
[Collection("Integration")]
public class ArticleProcessorTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    private static readonly DateTime RefDate = new(2026, 3, 14, 12, 0, 0, DateTimeKind.Utc);

    private static ArticleProcessor CreateProcessor(WebshopDbContext db, DateTime? orderDate = null)
        => new(db, new FakeOrderContext { OrderDate = orderDate ?? RefDate });

    private static WebshopDbContext GetDb(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<WebshopDbContext>();

    // ── No price-related includes ──────────────────────────────────────────────

    [Fact]
    public async Task None_Include_Does_Not_Change_Price()
    {
        using var scope = fixture.CreateScope();
        var article = new Article { Price = 42m };

        await CreateProcessor(GetDb(scope)).Process([article], ArticleIncludes.None);

        Assert.Equal(42m, article.Price);
    }

    [Fact]
    public async Task Unrelated_Include_Does_Not_Change_Price()
    {
        using var scope = fixture.CreateScope();
        var article = new Article { Price = 42m };

        await CreateProcessor(GetDb(scope)).Process([article], ArticleIncludes.Facets);

        Assert.Equal(42m, article.Price);
    }

    // ── ArticleIncludes.Price (DB path) ─────────────────────────────────────

    [Fact]
    public async Task Price_Include_Fetches_Price_From_Db()
    {
        using var scope = fixture.CreateScope();
        var db = GetDb(scope);

        var article = new Article
        {
            Title = "DB Price Article",
            Prices = [new ArticlePricePeriod { Price = 9.99m }]
        };
        db.Articles.Add(article);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var target = new Article { Id = article.Id };
        await CreateProcessor(db).Process([target], ArticleIncludes.Price);

        Assert.Equal(9.99m, target.Price);
    }

    [Fact]
    public async Task Price_Include_AdjacentBoundary_Selects_Entry_With_Later_EndDate()
    {
        using var scope = fixture.CreateScope();
        var db = GetDb(scope);

        var article = new Article
        {
            Title = "Adjacent Boundary Article",
            Prices =
            [
                new ArticlePricePeriod { Price = 8.00m,  StartDate = RefDate.AddDays(-5), EndDate = RefDate              },
                new ArticlePricePeriod { Price = 12.00m, StartDate = RefDate,             EndDate = RefDate.AddDays(5)   },
            ]
        };
        db.Articles.Add(article);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var target = new Article { Id = article.Id };
        await CreateProcessor(db).Process([target], ArticleIncludes.Price);

        Assert.Equal(12.00m, target.Price);
    }

    // ── ArticleIncludes.PricePeriod (in-memory path) ───────────────────────

    [Fact]
    public async Task PricePeriod_Include_WithLoadedPrices_Sets_Price_From_Collection()
    {
        using var scope = fixture.CreateScope();
        var article = new Article
        {
            Prices = [new ArticlePricePeriod { Price = 7.50m }]
        };

        await CreateProcessor(GetDb(scope)).Process([article], ArticleIncludes.PricePeriod);

        Assert.Equal(7.50m, article.Price);
    }

    [Fact]
    public async Task PricePeriod_Include_WithNullPrices_Sets_Price_To_Null()
    {
        using var scope = fixture.CreateScope();
        var article = new Article { Price = 42m, Prices = null };

        await CreateProcessor(GetDb(scope)).Process([article], ArticleIncludes.PricePeriod);

        Assert.Null(article.Price);
    }

    // ── Flag priority ──────────────────────────────────────────────────────────

    [Fact]
    public async Task PricePeriod_Takes_Priority_Over_Price_Flag()
    {
        using var scope = fixture.CreateScope();
        var article = new Article
        {
            Id = int.MaxValue,
            Prices = [new ArticlePricePeriod { Price = 15.00m }]
        };

        await CreateProcessor(GetDb(scope)).Process([article], ArticleIncludes.Price | ArticleIncludes.PricePeriod);

        Assert.Equal(15.00m, article.Price);
    }

    [Fact]
    public async Task All_Includes_Uses_PricePeriod_Path()
    {
        using var scope = fixture.CreateScope();
        var article = new Article
        {
            Id = int.MaxValue,
            Prices = [new ArticlePricePeriod { Price = 15.00m }]
        };

        await CreateProcessor(GetDb(scope)).Process([article], ArticleIncludes.All);

        Assert.Equal(15.00m, article.Price);
    }
}

file sealed class FakeOrderContext : IOrderContext
{
    public int? CustomerId => null;
    public int? OrganizationId => null;
    public DateTime? OrderDate { get; set; }
}
