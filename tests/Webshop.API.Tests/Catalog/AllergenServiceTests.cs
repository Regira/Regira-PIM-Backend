using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Webshop.Models.Entities.Catalog.Allergens;
using Xunit;

namespace Webshop.API.Tests.Catalog;

[Collection("Integration")]
public class AllergenServiceTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    private IEntityService<Allergen, AllergenSearchObject, EntitySortBy, EntityIncludes> GetService(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<IEntityService<Allergen, AllergenSearchObject, EntitySortBy, EntityIncludes>>();

    [Fact]
    public async Task Can_Create_And_Retrieve_Allergen()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);

        var allergen = new Allergen { Title = "Test Allergen", Code = "TST", Description = "A test allergen" };
        await service.Save(allergen);
        await service.SaveChanges();

        var retrieved = await service.Details(allergen.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Test Allergen", retrieved.Title);
        Assert.Equal("TST", retrieved.Code);
    }

    [Fact]
    public async Task Filter_By_Code_Returns_Matching()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);

        await service.Save(new Allergen { Title = "Gluten Test", Code = "GLT" });
        await service.Save(new Allergen { Title = "Dairy Test", Code = "DRT" });
        await service.SaveChanges();

        var results = await service.List(new AllergenSearchObject { Code = "GLT" });
        Assert.All(results, a => Assert.Equal("GLT", a.Code));
    }

    [Fact]
    public async Task Timestamps_Are_Set_On_Create()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);

        var allergen = new Allergen { Title = "Timestamp Test", Code = "TMS" };
        await service.Save(allergen);
        await service.SaveChanges();

        var retrieved = await service.Details(allergen.Id);
        Assert.NotNull(retrieved);
        Assert.True(retrieved.Created > DateTime.MinValue);
    }

    [Fact]
    public async Task List_Returns_Sorted_By_Title()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);

        await service.Save(new Allergen { Title = "Zebra Allergen", Code = "ZEB" });
        await service.Save(new Allergen { Title = "Alpha Allergen", Code = "ALP" });
        await service.SaveChanges();

        var results = await service.List(new AllergenSearchObject());
        var titles = results.Select(a => a.Title).ToList();
        Assert.Equal(titles.OrderBy(t => t).ToList(), titles);
    }
}
