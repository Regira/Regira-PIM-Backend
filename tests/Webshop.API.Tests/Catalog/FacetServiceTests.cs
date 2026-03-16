using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Webshop.Models.Entities.Taxonomy.Facets;
using Xunit;

namespace Webshop.API.Tests.Catalog;

[Collection("Integration")]
public class FacetServiceTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    private IEntityService<Facet, FacetSearchObject, EntitySortBy, FacetIncludes> GetService(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<IEntityService<Facet, FacetSearchObject, EntitySortBy, FacetIncludes>>();

    [Fact]
    public async Task Can_Create_Facet_With_Hierarchy()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);

        var parent = new Facet { Title = "Parent Cat" };
        await service.Save(parent);
        await service.SaveChanges();

        var child = new Facet
        {
            Title = "Child Cat",
            ParentEntities = [new RelatedFacet { ParentId = parent.Id }]
        };
        await service.Save(child);
        await service.SaveChanges();

        var retrieved = await service.List([new FacetSearchObject { ParentId = [parent.Id] }], [], FacetIncludes.Parents);
        Assert.Contains(retrieved, c => c.Title == "Child Cat");
    }

    [Fact]
    public async Task IsRoot_Filter_Returns_Only_Root_Facets()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);

        var root = new Facet { Title = "Root Only" };
        await service.Save(root);
        await service.SaveChanges();

        var rootResults = await service.List(new FacetSearchObject { IsRoot = true });
        Assert.All(rootResults, c => Assert.True(c.ParentEntities == null || c.ParentEntities.Count == 0));
    }

    [Fact]
    public async Task Soft_Delete_Filters_Archived()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);

        var facet = new Facet { Title = "ToArchive" };
        await service.Save(facet);
        await service.SaveChanges();

        facet.IsArchived = true;
        await service.Save(facet);
        await service.SaveChanges();

        var active = await service.List(new FacetSearchObject { IsArchived = false });
        Assert.DoesNotContain(active, c => c.Id == facet.Id);

        var archived = await service.List(new FacetSearchObject { IsArchived = true });
        Assert.Contains(archived, c => c.Id == facet.Id);
    }
}
