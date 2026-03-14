using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Webshop.Models.Entities.Catalog.Categories;
using Webshop.Models.Entities.Catalog.Parts;
using Webshop.Models.Entities.Catalog.Products;
using Xunit;

namespace Webshop.API.Tests.Catalog;

[Collection("Integration")]
public class ProductServiceTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    private IEntityService<Product, ProductSearchObject, ProductSortBy, ProductIncludes> GetProductService(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<IEntityService<Product, ProductSearchObject, ProductSortBy, ProductIncludes>>();

    private IEntityService<Category, CategorySearchObject, EntitySortBy, CategoryIncludes> GetCategoryService(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<IEntityService<Category, CategorySearchObject, EntitySortBy, CategoryIncludes>>();

    private IEntityService<Part, PartSearchObject, PartSortBy, PartIncludes> GetPartService(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<IEntityService<Part, PartSearchObject, PartSortBy, PartIncludes>>();

    [Fact]
    public async Task Can_Create_Product_With_Categories()
    {
        using var scope = fixture.CreateScope();
        var catService = GetCategoryService(scope);
        var productService = GetProductService(scope);

        var cat = new Category { Title = "Test Food" };
        await catService.Save(cat);
        await catService.SaveChanges();

        var product = new Product
        {
            Title = "Test Burger",
            Price = 9.99m,
            Categories = [new ProductCategory { CategoryId = cat.Id }]
        };
        await productService.Save(product);
        await productService.SaveChanges();

        var results = await productService.List(new ProductSearchObject { CategoryId = [cat.Id] });
        Assert.Contains(results, p => p.Title == "Test Burger");
    }

    [Fact]
    public async Task Filter_By_Price_Range()
    {
        using var scope = fixture.CreateScope();
        var service = GetProductService(scope);

        await service.Save(new Product { Title = "Cheap Item", Prices = [new ProductPriceHistory { Price = 1.00m }] });
        await service.Save(new Product { Title = "Mid Item", Prices = [new ProductPriceHistory { Price = 5.00m }] });
        await service.Save(new Product { Title = "Expensive Item", Prices = [new ProductPriceHistory { Price = 20.00m }] });
        await service.SaveChanges();

        var results = await service.List([new ProductSearchObject { MinPrice = 3.00m, MaxPrice = 10.00m }], [], ProductIncludes.Price);
        Assert.All(results, p => Assert.InRange(p.Price!.Value, 3.00m, 10.00m));
    }

    [Fact]
    public async Task Sort_By_Price_Ascending()
    {
        using var scope = fixture.CreateScope();
        var service = GetProductService(scope);

        await service.Save(new Product { Title = "B-Priced", Prices = [new ProductPriceHistory { Price = 15.00m }] });
        await service.Save(new Product { Title = "A-Priced", Prices = [new ProductPriceHistory { Price = 5.00m }] });
        await service.Save(new Product { Title = "C-Priced", Prices = [new ProductPriceHistory { Price = 25.00m }] });
        await service.SaveChanges();

        var results = await service.List([new ProductSearchObject()], [ProductSortBy.Price]);
        var prices = results.Select(p => p.Price).ToList();
        Assert.Equal(prices.OrderBy(p => p).ToList(), prices);
    }

    [Fact]
    public async Task Related_Parts_Are_Synced()
    {
        using var scope = fixture.CreateScope();
        var partService = GetPartService(scope);
        var productService = GetProductService(scope);

        var part1 = new Part { Title = "PartA", Price = 0.50m };
        var part2 = new Part { Title = "PartB", Price = 0.75m };
        await partService.Save(part1);
        await partService.Save(part2);
        await partService.SaveChanges();

        var product = new Product
        {
            Title = "Product With Parts",
            Price = 10.00m,
            Parts = [new ProductPart { PartId = part1.Id, Quantity = 1 }]
        };
        await productService.Save(product);
        await productService.SaveChanges();

        // Update: replace part1 with part2
        var loaded = await productService.Details(product.Id);
        Assert.NotNull(loaded);
        loaded.Parts = [new ProductPart { PartId = part2.Id, Quantity = 1 }];
        await productService.Save(loaded);
        await productService.SaveChanges();

        var updated = await productService.Details(product.Id);
        Assert.NotNull(updated?.Parts);
        Assert.Single(updated.Parts);
        Assert.Equal(part2.Id, updated.Parts.First().PartId);
    }

    [Fact]
    public async Task Part_Quantity_Is_Persisted()
    {
        using var scope = fixture.CreateScope();
        var partService = GetPartService(scope);
        var productService = GetProductService(scope);

        var part = new Part { Title = "Patty", Price = 0.80m };
        await partService.Save(part);
        await partService.SaveChanges();

        var product = new Product
        {
            Title = "Burger With Quantity",
            Price = 8.50m,
            Parts = [new ProductPart { PartId = part.Id, Quantity = 2.5m }]
        };
        await productService.Save(product);
        await productService.SaveChanges();

        var loaded = await productService.Details(product.Id);
        Assert.NotNull(loaded?.Parts);
        Assert.Single(loaded.Parts);
        Assert.Equal(2.5m, loaded.Parts.First().Quantity);
    }
}
