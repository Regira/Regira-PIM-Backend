using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Services.Abstractions;
using Webshop.Models.Entities.Catalog.Articles;
using Regira.Entities.Models;
using Xunit;
using Webshop.Models.Entities.Taxonomy.Facets;

namespace Webshop.API.Tests.Catalog;

[Collection("Integration")]
public class ArticleServiceTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    private IEntityService<Article, ArticleSearchObject, ArticleSortBy, ArticleIncludes> GetArticleService(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<IEntityService<Article, ArticleSearchObject, ArticleSortBy, ArticleIncludes>>();

    private IEntityService<Facet, FacetSearchObject, EntitySortBy, FacetIncludes> GetFacetService(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<IEntityService<Facet, FacetSearchObject, EntitySortBy, FacetIncludes>>();

    [Fact]
    public async Task Can_Create_Article_With_Categories()
    {
        using var scope = fixture.CreateScope();
        var facetService = GetFacetService(scope);
        var articleService = GetArticleService(scope);

        var facet = new Facet { Title = "Test Category" };
        await facetService.Save(facet);
        await facetService.SaveChanges();

        var article = new Article
        {
            Title = "Test Article",
            Facets = [new ArticleFacet { FacetId = facet.Id }]
        };
        await articleService.Save(article);
        await articleService.SaveChanges();

        var results = await articleService.List(new ArticleSearchObject { FacetId = [facet.Id] });
        Assert.Contains(results, a => a.Title == "Test Article");
    }

    [Fact]
    public async Task Filter_By_Price_Range()
    {
        using var scope = fixture.CreateScope();
        var service = GetArticleService(scope);

        await service.Save(new Article { Title = "Cheap Article", Prices = [new ArticlePricePeriod { Price = 1.00m }] });
        await service.Save(new Article { Title = "Mid Article", Prices = [new ArticlePricePeriod { Price = 5.00m }] });
        await service.Save(new Article { Title = "Expensive Article", Prices = [new ArticlePricePeriod { Price = 20.00m }] });
        await service.SaveChanges();

        var results = await service.List([new ArticleSearchObject { MinPrice = 3.00m, MaxPrice = 10.00m }], [], ArticleIncludes.Price);
        Assert.All(results, a => Assert.InRange(a.Price!.Value, 3.00m, 10.00m));
    }

    [Fact]
    public async Task Sort_By_Price_Ascending()
    {
        using var scope = fixture.CreateScope();
        var service = GetArticleService(scope);

        await service.Save(new Article { Title = "B-Priced Article", Prices = [new ArticlePricePeriod { Price = 15.00m }] });
        await service.Save(new Article { Title = "A-Priced Article", Prices = [new ArticlePricePeriod { Price = 5.00m }] });
        await service.Save(new Article { Title = "C-Priced Article", Prices = [new ArticlePricePeriod { Price = 25.00m }] });
        await service.SaveChanges();

        var results = await service.List([new ArticleSearchObject()], [ArticleSortBy.Price]);
        var prices = results.Select(a => a.Price).ToList();
        Assert.Equal(prices.OrderBy(p => p).ToList(), prices);
    }

    [Fact]
    public async Task Related_Components_Are_Synced()
    {
        using var scope = fixture.CreateScope();
        var service = GetArticleService(scope);

        // Create component articles
        var comp1 = new Article { Title = "Component A" };
        var comp2 = new Article { Title = "Component B" };
        await service.Save(comp1);
        await service.Save(comp2);
        await service.SaveChanges();

        // Create assembly with comp1
        var assembly = new Article
        {
            Title = "Assembly Article",
            Components = [new ArticleComponent { ComponentId = comp1.Id, Quantity = 1 }]
        };
        await service.Save(assembly);
        await service.SaveChanges();

        // Update: replace comp1 with comp2
        var loaded = await service.Details(assembly.Id);
        Assert.NotNull(loaded);
        loaded.Components = [new ArticleComponent { ComponentId = comp2.Id, Quantity = 1 }];
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
        var service = GetArticleService(scope);

        var component = new Article { Title = "Component Part" };
        await service.Save(component);
        await service.SaveChanges();

        var assembly = new Article
        {
            Title = "Assembly With Quantity",
            Components = [new ArticleComponent { ComponentId = component.Id, Quantity = 2.5m }]
        };
        await service.Save(assembly);
        await service.SaveChanges();

        var loaded = await service.Details(assembly.Id);
        Assert.NotNull(loaded?.Components);
        Assert.Single(loaded.Components);
        Assert.Equal(2.5m, loaded.Components.First().Quantity);
    }
}
