using Microsoft.Extensions.DependencyInjection;
using PIM.Models.Catalog.Products;
using PIM.Models.Taxonomy.Facets;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Xunit;

namespace PIM.API.Tests.Catalog;

[Collection("Integration")]
public class ProductServiceTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    private IEntityService<Product, ProductSearchObject, ProductSortBy, ProductIncludes> GetProductService(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<IEntityService<Product, ProductSearchObject, ProductSortBy, ProductIncludes>>();

    private IEntityService<Facet, FacetSearchObject, EntitySortBy, FacetIncludes> GetFacetService(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<IEntityService<Facet, FacetSearchObject, EntitySortBy, FacetIncludes>>();

    [Fact]
    public async Task Can_Create_Product_With_Categories()
    {
        using var scope = fixture.CreateScope();
        var facetService = GetFacetService(scope);
        var productService = GetProductService(scope);

        var facet = new Facet { Title = "Test Category" };
        await facetService.Save(facet);
        await facetService.SaveChanges();

        var product = new Product
        {
            Title = "Test Product",
            Facets = [new ProductFacet { FacetId = facet.Id }]
        };
        await productService.Save(product);
        await productService.SaveChanges();

        var results = await productService.List(new ProductSearchObject { FacetId = [facet.Id] });
        Assert.Contains(results, a => a.Title == "Test Product");
    }

    [Fact]
    public async Task Filter_By_Price_Range()
    {
        using var scope = fixture.CreateScope();
        var service = GetProductService(scope);

        await service.Save(new Product { Title = "Cheap Product", Prices = [new ProductPricePeriod { Price = 1.00m }] });
        await service.Save(new Product { Title = "Mid Product", Prices = [new ProductPricePeriod { Price = 5.00m }] });
        await service.Save(new Product { Title = "Expensive Product", Prices = [new ProductPricePeriod { Price = 20.00m }] });
        await service.SaveChanges();

        var results = await service.List([new ProductSearchObject { MinPrice = 3.00m, MaxPrice = 10.00m }], [], ProductIncludes.Price);
        Assert.All(results, a => Assert.InRange(a.Price!.Value, 3.00m, 10.00m));
    }

    [Fact]
    public async Task Sort_By_Price_Ascending()
    {
        using var scope = fixture.CreateScope();
        var service = GetProductService(scope);

        await service.Save(new Product { Title = "B-Priced Product", Prices = [new ProductPricePeriod { Price = 15.00m }] });
        await service.Save(new Product { Title = "A-Priced Product", Prices = [new ProductPricePeriod { Price = 5.00m }] });
        await service.Save(new Product { Title = "C-Priced Product", Prices = [new ProductPricePeriod { Price = 25.00m }] });
        await service.SaveChanges();

        var results = await service.List([new ProductSearchObject()], [ProductSortBy.Price]);
        var prices = results.Select(a => a.Price).ToList();
        Assert.Equal(prices.OrderBy(p => p).ToList(), prices);
    }

    [Fact]
    public async Task Related_Components_Are_Synced()
    {
        using var scope = fixture.CreateScope();
        var service = GetProductService(scope);

        // Create component products
        var comp1 = new Product { Title = "Component A" };
        var comp2 = new Product { Title = "Component B" };
        await service.Save(comp1);
        await service.Save(comp2);
        await service.SaveChanges();

        // Create assembly with comp1
        var assembly = new Product
        {
            Title = "Assembly Product",
            Components = [new ProductComponent { ComponentId = comp1.Id, Quantity = 1 }]
        };
        await service.Save(assembly);
        await service.SaveChanges();

        // Update: replace comp1 with comp2
        var loaded = await service.Details(assembly.Id);
        Assert.NotNull(loaded);
        loaded.Components = [new ProductComponent { ComponentId = comp2.Id, Quantity = 1 }];
        await service.Save(loaded);
        await service.SaveChanges();

        var updated = await service.Details(assembly.Id);
        Assert.NotNull(updated?.Components);
        Assert.Single(updated.Components);
        Assert.Equal(comp2.Id, updated.Components.First().ComponentId);
    }

    [Fact]
    public async Task Component_Quantity_Is_Persisted()
    {
        using var scope = fixture.CreateScope();
        var service = GetProductService(scope);

        var component = new Product { Title = "Component Part" };
        await service.Save(component);
        await service.SaveChanges();

        var assembly = new Product
        {
            Title = "Assembly With Quantity",
            Components = [new ProductComponent { ComponentId = component.Id, Quantity = 2.5m }]
        };
        await service.Save(assembly);
        await service.SaveChanges();

        var loaded = await service.Details(assembly.Id);
        Assert.NotNull(loaded?.Components);
        Assert.Single(loaded.Components);
        Assert.Equal(2.5m, loaded.Components.First().Quantity);
    }
}
