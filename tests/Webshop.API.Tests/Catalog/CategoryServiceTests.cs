using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Webshop.Models.Entities.Classification.Categories;
using Xunit;

namespace Webshop.API.Tests.Catalog;

[Collection("Integration")]
public class CategoryServiceTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    private IEntityService<Category, CategorySearchObject, EntitySortBy, CategoryIncludes> GetService(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<IEntityService<Category, CategorySearchObject, EntitySortBy, CategoryIncludes>>();

    [Fact]
    public async Task Can_Create_Category_With_Hierarchy()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);

        var parent = new Category { Title = "Parent Cat" };
        await service.Save(parent);
        await service.SaveChanges();

        var child = new Category
        {
            Title = "Child Cat",
            ParentEntities = [new RelatedCategory { ParentId = parent.Id }]
        };
        await service.Save(child);
        await service.SaveChanges();

        var retrieved = await service.List([new CategorySearchObject { ParentId = [parent.Id] }], [], CategoryIncludes.Parents);
        Assert.Contains(retrieved, c => c.Title == "Child Cat");
    }

    [Fact]
    public async Task IsRoot_Filter_Returns_Only_Root_Categories()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);

        var root = new Category { Title = "Root Only" };
        await service.Save(root);
        await service.SaveChanges();

        var rootResults = await service.List(new CategorySearchObject { IsRoot = true });
        Assert.All(rootResults, c => Assert.True(c.ParentEntities == null || c.ParentEntities.Count == 0));
    }

    [Fact]
    public async Task Soft_Delete_Filters_Archived()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);

        var cat = new Category { Title = "ToArchive" };
        await service.Save(cat);
        await service.SaveChanges();

        cat.IsArchived = true;
        await service.Save(cat);
        await service.SaveChanges();

        var active = await service.List(new CategorySearchObject { IsArchived = false });
        Assert.DoesNotContain(active, c => c.Id == cat.Id);

        var archived = await service.List(new CategorySearchObject { IsArchived = true });
        Assert.Contains(archived, c => c.Id == cat.Id);
    }
}
