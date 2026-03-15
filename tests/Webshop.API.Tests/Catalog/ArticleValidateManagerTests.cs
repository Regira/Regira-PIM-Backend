using Regira.Entities.Models;
using Webshop.Models.Entities.Catalog.Articles;
using Webshop.Services.Entities.Catalog.Articles;
using Xunit;

namespace Webshop.API.Tests.Catalog;

/// <summary>
/// Unit tests for <see cref="ArticleValidateManager.Validate"/>.
/// No database or DI needed — Validate() only inspects the article's Prices and Components collections.
/// </summary>
public class ArticleValidateManagerTests
{
    private static ArticleValidateManager CreateSut() => new(null!);

    // -------------------------------------------------------------------------
    // Valid — no exception expected
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_NullPrices_DoesNotThrow()
    {
        var sut = CreateSut();
        sut.Validate(new Article { Title = "Test", Prices = null });
    }

    [Fact]
    public void Validate_EmptyPrices_DoesNotThrow()
    {
        var sut = CreateSut();
        sut.Validate(new Article { Title = "Test", Prices = [] });
    }

    [Fact]
    public void Validate_SinglePrice_DoesNotThrow()
    {
        var sut = CreateSut();
        sut.Validate(new Article
        {
            Title = "Test",
            Prices = [new ArticlePriceHistory { Price = 10m }]
        });
    }

    [Fact]
    public void Validate_TwoNonOverlappingBoundedPrices_DoesNotThrow()
    {
        var sut = CreateSut();
        sut.Validate(new Article
        {
            Title = "Test",
            Prices =
            [
                new ArticlePriceHistory { Price = 9m,  StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 1, 31) },
                new ArticlePriceHistory { Price = 10m, StartDate = new DateTime(2025, 2, 1), EndDate = new DateTime(2025, 2, 28) }
            ]
        });
    }

    [Fact]
    public void Validate_OpenStartFollowedByOpenEnd_DoesNotThrow()
    {
        var sut = CreateSut();
        sut.Validate(new Article
        {
            Title = "Test",
            Prices =
            [
                new ArticlePriceHistory { Price = 9m,  StartDate = null,                    EndDate = new DateTime(2025, 1, 31) },
                new ArticlePriceHistory { Price = 10m, StartDate = new DateTime(2025, 2, 1), EndDate = null }
            ]
        });
    }

    [Fact]
    public void Validate_PricesTouchingAtBoundary_DoesNotThrow()
    {
        var sut = CreateSut();
        sut.Validate(new Article
        {
            Title = "Test",
            Prices =
            [
                new ArticlePriceHistory { Price = 9m,  StartDate = new DateTime(2025, 1, 1),  EndDate = new DateTime(2025, 1, 31) },
                new ArticlePriceHistory { Price = 10m, StartDate = new DateTime(2025, 1, 31), EndDate = new DateTime(2025, 2, 28) }
            ]
        });
    }

    // -------------------------------------------------------------------------
    // Invalid prices — EntityInputException<Article> expected
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_TwoPricesWithoutStartDate_Throws()
    {
        var sut = CreateSut();
        var article = new Article
        {
            Title = "Test",
            Prices =
            [
                new ArticlePriceHistory { Price = 9m,  StartDate = null, EndDate = new DateTime(2025, 1, 31) },
                new ArticlePriceHistory { Price = 10m, StartDate = null, EndDate = new DateTime(2025, 3, 31) }
            ]
        };

        var ex = Assert.Throws<EntityInputException<Article>>(() => sut.Validate(article));
        Assert.True(ex.InputErrors.ContainsKey("Prices"));
        Assert.Contains("start date", ex.InputErrors["Prices"], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_TwoPricesWithoutEndDate_Throws()
    {
        var sut = CreateSut();
        var article = new Article
        {
            Title = "Test",
            Prices =
            [
                new ArticlePriceHistory { Price = 9m,  StartDate = new DateTime(2025, 1, 1), EndDate = null },
                new ArticlePriceHistory { Price = 10m, StartDate = new DateTime(2025, 2, 1), EndDate = null }
            ]
        };

        var ex = Assert.Throws<EntityInputException<Article>>(() => sut.Validate(article));
        Assert.True(ex.InputErrors.ContainsKey("Prices"));
        Assert.Contains("end date", ex.InputErrors["Prices"], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_PartiallyOverlappingDateRanges_Throws()
    {
        var sut = CreateSut();
        var article = new Article
        {
            Title = "Test",
            Prices =
            [
                new ArticlePriceHistory { Price = 9m,  StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 2, 28) },
                new ArticlePriceHistory { Price = 10m, StartDate = new DateTime(2025, 2, 1), EndDate = new DateTime(2025, 3, 31) }
            ]
        };

        var ex = Assert.Throws<EntityInputException<Article>>(() => sut.Validate(article));
        Assert.True(ex.InputErrors.ContainsKey("Prices"));
        Assert.Contains("overlapping", ex.InputErrors["Prices"], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_FullyContainedDateRange_Throws()
    {
        var sut = CreateSut();
        var article = new Article
        {
            Title = "Test",
            Prices =
            [
                new ArticlePriceHistory { Price = 9m,  StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 3, 31) },
                new ArticlePriceHistory { Price = 10m, StartDate = new DateTime(2025, 2, 1), EndDate = new DateTime(2025, 2, 28) }
            ]
        };

        var ex = Assert.Throws<EntityInputException<Article>>(() => sut.Validate(article));
        Assert.True(ex.InputErrors.ContainsKey("Prices"));
        Assert.Contains("overlapping", ex.InputErrors["Prices"], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_OpenStartOverlapsAnotherRange_Throws()
    {
        var sut = CreateSut();
        var article = new Article
        {
            Title = "Test",
            Prices =
            [
                new ArticlePriceHistory { Price = 9m,  StartDate = null,                    EndDate = new DateTime(2025, 2, 28) },
                new ArticlePriceHistory { Price = 10m, StartDate = new DateTime(2025, 2, 1), EndDate = new DateTime(2025, 3, 31) }
            ]
        };

        var ex = Assert.Throws<EntityInputException<Article>>(() => sut.Validate(article));
        Assert.True(ex.InputErrors.ContainsKey("Prices"));
        Assert.Contains("overlapping", ex.InputErrors["Prices"], StringComparison.OrdinalIgnoreCase);
    }

    // -------------------------------------------------------------------------
    // Invalid components — EntityInputException<Article> expected
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_ComponentWithZeroQuantity_Throws()
    {
        var sut = CreateSut();
        var article = new Article
        {
            Title = "Test",
            Components = [new ArticleComponent { ComponentId = 1, Quantity = 0 }]
        };

        var ex = Assert.Throws<EntityInputException<Article>>(() => sut.Validate(article));
        Assert.True(ex.InputErrors.ContainsKey("Components"));
    }

    [Fact]
    public void Validate_ComponentWithNegativeQuantity_Throws()
    {
        var sut = CreateSut();
        var article = new Article
        {
            Title = "Test",
            Components = [new ArticleComponent { ComponentId = 1, Quantity = -1m }]
        };

        var ex = Assert.Throws<EntityInputException<Article>>(() => sut.Validate(article));
        Assert.True(ex.InputErrors.ContainsKey("Components"));
    }

    [Fact]
    public void Validate_ComponentWithPositiveQuantity_DoesNotThrow()
    {
        var sut = CreateSut();
        sut.Validate(new Article
        {
            Title = "Test",
            Components = [new ArticleComponent { ComponentId = 1, Quantity = 2.5m }]
        });
    }
}
