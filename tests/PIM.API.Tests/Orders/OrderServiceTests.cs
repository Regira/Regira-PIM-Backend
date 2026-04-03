using Microsoft.Extensions.DependencyInjection;
using PIM.Models.Catalog.Articles;
using PIM.Models.Orders;
using PIM.Models.Stakeholders.Parties;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Xunit;

namespace PIM.API.Tests.Orders;

[Collection("Integration")]
public class OrderServiceTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    private IEntityService<Order, OrderSearchObject, OrderSortBy, OrderIncludes> GetService(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<IEntityService<Order, OrderSearchObject, OrderSortBy, OrderIncludes>>();

    private async Task<(Party Customer, Article Article, Article Component)> SeedPrerequisites(IServiceScope scope)
    {
        var customerService = scope.ServiceProvider.GetRequiredService<IEntityService<Party, PartySearchObject, PartySortBy, PartyIncludes>>();
        var articleService = scope.ServiceProvider.GetRequiredService<IEntityService<Article, ArticleSearchObject, ArticleSortBy, ArticleIncludes>>();

        var customer = new Person { GivenName = "Order", FamilyName = $"TestUser-{Guid.NewGuid():N}" };
        await customerService.Save(customer);
        await customerService.SaveChanges();

        var article = new Article { Title = "Order Test Article", Prices = [new ArticlePricePeriod { Price = 10.00m }] };
        await articleService.Save(article);
        await articleService.SaveChanges();

        var component = new Article { Title = "Order Test Component", Prices = [new ArticlePricePeriod { Price = 1.50m }] };
        await articleService.Save(component);
        await articleService.SaveChanges();

        return (customer, article, component);
    }

    [Fact]
    public async Task Order_Without_Lines_Throws_EntityInputException()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);
        var (customer, _, _) = await SeedPrerequisites(scope);

        var order = new Order { CustomerId = customer.Id };
        var ex = await Assert.ThrowsAsync<EntityInputException<Order>>(() => service.Save(order));
        Assert.Contains("OrderLines", ex.InputErrors.Keys);
    }

    [Fact]
    public async Task Order_Code_Is_Auto_Generated()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);
        var (customer, article, _) = await SeedPrerequisites(scope);

        var order = new Order
        {
            CustomerId = customer.Id,
            OrderLines = [new OrderLine { ArticleId = article.Id, Quantity = 1, UnitPrice = 10.00m }]
        };
        await service.Save(order);
        await service.SaveChanges();

        Assert.NotNull(order.Code);
        Assert.StartsWith("ORD", order.Code);
    }

    [Fact]
    public async Task Total_Is_Calculated_From_Lines()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);
        var (customer, article, _) = await SeedPrerequisites(scope);

        var order = new Order
        {
            CustomerId = customer.Id,
            OrderLines =
            [
                new OrderLine { ArticleId = article.Id, Quantity = 2, UnitPrice = 10.00m },
                new OrderLine { ArticleId = article.Id, Quantity = 1, UnitPrice = 5.00m }
            ]
        };
        await service.Save(order);
        await service.SaveChanges();

        Assert.Equal(25.00m, order.Total);
    }

    [Fact]
    public async Task SubTotal_Includes_Part_Additions()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);
        var (customer, article, component) = await SeedPrerequisites(scope);

        var order = new Order
        {
            CustomerId = customer.Id,
            OrderLines =
            [
                new OrderLine
                {
                    ArticleId = article.Id,
                    Quantity = 2,
                    UnitPrice = 10.00m,
                    ComponentAdditions = [new OrderLineComponentAddition { ArticleId = component.Id, Price = 1.50m }]
                }
            ]
        };
        await service.Save(order);
        await service.SaveChanges();

        // (10.00 + 1.50) * 2 = 23.00
        Assert.Equal(23.00m, order.OrderLines!.First().SubTotal);
        Assert.Equal(23.00m, order.Total);
    }

    [Fact]
    public async Task SubTotal_Includes_Part_Omissions()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);
        var (customer, article, component) = await SeedPrerequisites(scope);

        var order = new Order
        {
            CustomerId = customer.Id,
            OrderLines =
            [
                new OrderLine
                {
                    ArticleId = article.Id,
                    Quantity = 3,
                    UnitPrice = 10.00m,
                    ComponentOmissions = [new OrderLineComponentOmission { ArticleId = component.Id, Price = 1.50m }]
                }
            ]
        };
        await service.Save(order);
        await service.SaveChanges();

        // (10.00 - 1.50) * 3 = 25.50
        Assert.Equal(25.50m, order.OrderLines!.First().SubTotal);
        Assert.Equal(25.50m, order.Total);
    }

    [Fact]
    public async Task SubTotal_Combines_Additions_And_Omissions()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);
        var (customer, article, component) = await SeedPrerequisites(scope);

        var order = new Order
        {
            CustomerId = customer.Id,
            OrderLines =
            [
                new OrderLine
                {
                    ArticleId = article.Id,
                    Quantity = 2,
                    UnitPrice = 10.00m,
                    ComponentAdditions  = [new OrderLineComponentAddition { ArticleId = component.Id, Price = 2.00m }],
                    ComponentOmissions = [new OrderLineComponentOmission { ArticleId = component.Id, Price = 0.50m }]
                }
            ]
        };
        await service.Save(order);
        await service.SaveChanges();

        // (10.00 + 2.00 - 0.50) * 2 = 23.00
        Assert.Equal(23.00m, order.OrderLines!.First().SubTotal);
        Assert.Equal(23.00m, order.Total);
    }

    [Fact]
    public async Task Past_Scheduled_Order_Cannot_Be_Modified()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);
        var (customer, article, _) = await SeedPrerequisites(scope);

        var order = new Order
        {
            CustomerId = customer.Id,
            ScheduledDate = DateTime.UtcNow.AddDays(-5),
            OrderLines = [new OrderLine { ArticleId = article.Id, Quantity = 1, UnitPrice = 10.00m }]
        };
        await service.Save(order);
        await service.SaveChanges();

        // Try to modify
        order.Status = OrderStatus.Processing;
        var ex = await Assert.ThrowsAsync<EntityInputException<Order>>(() => service.Modify(order));
        Assert.Contains("ScheduledDate", ex.InputErrors.Keys);
    }

    [Fact]
    public async Task Future_Scheduled_Order_Can_Be_Modified()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);
        var (customer, article, _) = await SeedPrerequisites(scope);

        var order = new Order
        {
            CustomerId = customer.Id,
            ScheduledDate = DateTime.UtcNow.AddDays(30),
            OrderLines = [new OrderLine { ArticleId = article.Id, Quantity = 1, UnitPrice = 10.00m }]
        };
        await service.Save(order);
        await service.SaveChanges();

        order.Status = OrderStatus.Processing;
        await service.Modify(order);
        await service.SaveChanges();

        var modified = await service.Details(order.Id);

        Assert.NotNull(modified);
        Assert.Equal(OrderStatus.Processing, modified.Status);
    }
}
