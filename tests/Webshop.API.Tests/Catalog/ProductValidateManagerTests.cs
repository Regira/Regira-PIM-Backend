using Regira.Entities.Models;
using Webshop.Models.Entities.Catalog.Products;
using Webshop.Services.Entities.Catalog.Products;
using Xunit;

namespace Webshop.API.Tests.Catalog;

/// <summary>
/// Unit tests for <see cref="ProductValidateManager.Validate"/>.
/// No database or DI needed — Validate() only inspects the product's Prices collection.
/// </summary>
public class ProductValidateManagerTests
{
    // Validate() never calls the repository, so null! is safe here.
    private static ProductValidateManager CreateSut() => new(null!);

    // -------------------------------------------------------------------------
    // Valid — no exception expected
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_NullPrices_DoesNotThrow()
    {
        var sut = CreateSut();
        sut.Validate(new Product { Title = "Test", Prices = null });
    }

    [Fact]
    public void Validate_EmptyPrices_DoesNotThrow()
    {
        var sut = CreateSut();
        sut.Validate(new Product { Title = "Test", Prices = [] });
    }

    [Fact]
    public void Validate_SinglePrice_DoesNotThrow()
    {
        var sut = CreateSut();
        sut.Validate(new Product
        {
            Title = "Test",
            Prices = [new ProductPriceHistory { Price = 10m }]
        });
    }

    [Fact]
    public void Validate_TwoNonOverlappingBoundedPrices_DoesNotThrow()
    {
        // Jan 1–31 followed by Feb 1–28: clear gap between the two ranges.
        var sut = CreateSut();
        sut.Validate(new Product
        {
            Title = "Test",
            Prices =
            [
                new ProductPriceHistory { Price = 9m,  StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 1, 31) },
                new ProductPriceHistory { Price = 10m, StartDate = new DateTime(2025, 2, 1), EndDate = new DateTime(2025, 2, 28) }
            ]
        });
    }

    [Fact]
    public void Validate_OpenStartFollowedByOpenEnd_DoesNotThrow()
    {
        // Price1: open start → Jan 31 (legacy/fallback price)
        // Price2: Feb 1 → open end (current price)
        var sut = CreateSut();
        sut.Validate(new Product
        {
            Title = "Test",
            Prices =
            [
                new ProductPriceHistory { Price = 9m,  StartDate = null,                    EndDate = new DateTime(2025, 1, 31) },
                new ProductPriceHistory { Price = 10m, StartDate = new DateTime(2025, 2, 1), EndDate = null }
            ]
        });
    }

    [Fact]
    public void Validate_PricesTouchingAtBoundary_DoesNotThrow()
    {
        // Price1 ends Jan 31 and price2 starts Jan 31.
        // The overlap check uses strict inequalities (> and <) on the boundary,
        // so touching at the same date is allowed.
        var sut = CreateSut();
        sut.Validate(new Product
        {
            Title = "Test",
            Prices =
            [
                new ProductPriceHistory { Price = 9m,  StartDate = new DateTime(2025, 1, 1),  EndDate = new DateTime(2025, 1, 31) },
                new ProductPriceHistory { Price = 10m, StartDate = new DateTime(2025, 1, 31), EndDate = new DateTime(2025, 2, 28) }
            ]
        });
    }

    // -------------------------------------------------------------------------
    // Invalid — EntityInputException<Product> expected
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_TwoPricesWithoutStartDate_Throws()
    {
        var sut = CreateSut();
        var product = new Product
        {
            Title = "Test",
            Prices =
            [
                new ProductPriceHistory { Price = 9m,  StartDate = null, EndDate = new DateTime(2025, 1, 31) },
                new ProductPriceHistory { Price = 10m, StartDate = null, EndDate = new DateTime(2025, 3, 31) }
            ]
        };

        var ex = Assert.Throws<EntityInputException<Product>>(() => sut.Validate(product));
        Assert.True(ex.InputErrors.ContainsKey("Prices"));
        Assert.Contains("start date", ex.InputErrors["Prices"], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_TwoPricesWithoutEndDate_Throws()
    {
        var sut = CreateSut();
        var product = new Product
        {
            Title = "Test",
            Prices =
            [
                new ProductPriceHistory { Price = 9m,  StartDate = new DateTime(2025, 1, 1), EndDate = null },
                new ProductPriceHistory { Price = 10m, StartDate = new DateTime(2025, 2, 1), EndDate = null }
            ]
        };

        var ex = Assert.Throws<EntityInputException<Product>>(() => sut.Validate(product));
        Assert.True(ex.InputErrors.ContainsKey("Prices"));
        Assert.Contains("end date", ex.InputErrors["Prices"], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_PartiallyOverlappingDateRanges_Throws()
    {
        // Jan 1–Feb 28 overlaps Feb 1–Mar 31 (overlap window: Feb 1–28).
        var sut = CreateSut();
        var product = new Product
        {
            Title = "Test",
            Prices =
            [
                new ProductPriceHistory { Price = 9m,  StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 2, 28) },
                new ProductPriceHistory { Price = 10m, StartDate = new DateTime(2025, 2, 1), EndDate = new DateTime(2025, 3, 31) }
            ]
        };

        var ex = Assert.Throws<EntityInputException<Product>>(() => sut.Validate(product));
        Assert.True(ex.InputErrors.ContainsKey("Prices"));
        Assert.Contains("overlapping", ex.InputErrors["Prices"], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_FullyContainedDateRange_Throws()
    {
        // Price2 (Feb 1–Feb 28) is entirely inside price1 (Jan 1–Mar 31).
        var sut = CreateSut();
        var product = new Product
        {
            Title = "Test",
            Prices =
            [
                new ProductPriceHistory { Price = 9m,  StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 3, 31) },
                new ProductPriceHistory { Price = 10m, StartDate = new DateTime(2025, 2, 1), EndDate = new DateTime(2025, 2, 28) }
            ]
        };

        var ex = Assert.Throws<EntityInputException<Product>>(() => sut.Validate(product));
        Assert.True(ex.InputErrors.ContainsKey("Prices"));
        Assert.Contains("overlapping", ex.InputErrors["Prices"], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_OpenStartOverlapsAnotherRange_Throws()
    {
        // Price1 has no start date and ends Feb 28.
        // Price2 starts Feb 1, so the two share Feb 1–28.
        var sut = CreateSut();
        var product = new Product
        {
            Title = "Test",
            Prices =
            [
                new ProductPriceHistory { Price = 9m,  StartDate = null,                    EndDate = new DateTime(2025, 2, 28) },
                new ProductPriceHistory { Price = 10m, StartDate = new DateTime(2025, 2, 1), EndDate = new DateTime(2025, 3, 31) }
            ]
        };

        var ex = Assert.Throws<EntityInputException<Product>>(() => sut.Validate(product));
        Assert.True(ex.InputErrors.ContainsKey("Prices"));
        Assert.Contains("overlapping", ex.InputErrors["Prices"], StringComparison.OrdinalIgnoreCase);
    }
}
