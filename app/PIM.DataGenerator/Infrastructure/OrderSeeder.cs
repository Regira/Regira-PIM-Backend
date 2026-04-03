using Bogus;
using Microsoft.Extensions.Logging;
using PIM.Models.Catalog.Articles;
using PIM.Models.Orders;
using PIM.Models.Stakeholders.Parties;
using Regira.Entities.Services.Abstractions;

namespace PIM.DataGenerator.Infrastructure;

public class OrderSeeder(
    IEntityService<Order> orderService,
    ILogger<OrderSeeder> logger)
{
    private const int BatchSize = 100;

    public async Task<IList<Order>> SeedAsync(IList<Party> customers, IList<Article> articles)
    {
        return await SeedOrders(customers, articles);
    }

    public async Task<IList<Order>> SeedOrders(IList<Party> customers, IList<Article> articles)
    {
        var orders = await orderService.List();

        if (orders.Any())
        {
            logger.LogInformation("Orders already exist, skipping seeding.");
            return orders;
        }

        var f = new Faker();
        var statuses = Enum.GetValues<OrderStatus>();
        orders = new List<Order>(10000);

        // Collect component article ids for additions/omissions
        var componentArticles = articles
            .SelectMany(a => a.Components?.Select(c => c.ComponentId) ?? [])
            .Distinct()
            .ToList();

        logger.LogInformation("Seeding orders...");
        for (int i = 0; i < 10000; i++)
        {
            var customer = f.PickRandom(customers);
            var selectedArticles = f.PickRandom(articles, f.Random.Int(1, 5)).DistinctBy(a => a.Id).ToList();

            var orderLines = selectedArticles.Select((article, idx) =>
            {
                var unitPrice = article.Prices?.FirstOrDefault()?.Price ?? 0m;
                var quantity = (decimal)f.Random.Int(1, 3);

                List<OrderLineComponentAddition>? partAdditions = null;
                if (f.Random.Bool(0.4f) && componentArticles.Count > 0)
                {
                    partAdditions = f.PickRandom(componentArticles, f.Random.Int(1, Math.Min(3, componentArticles.Count)))
                        .Distinct()
                        .Select(id => new OrderLineComponentAddition { ArticleId = id, Price = f.Random.Decimal(0.15m, 1.50m) })
                        .ToList();
                }

                List<OrderLineComponentOmission>? partOmissions = null;
                if (article.Components?.Count > 0 && f.Random.Bool(0.3f))
                {
                    var omittable = article.Components.Where(c => c.IsOmittable).Select(c => c.ComponentId).ToList();
                    if (omittable.Count > 0)
                    {
                        partOmissions = f.PickRandom(omittable, f.Random.Int(1, Math.Min(2, omittable.Count)))
                            .Distinct()
                            .Select(id => new OrderLineComponentOmission { ArticleId = id, Price = f.Random.Decimal(0.15m, 1.50m) })
                            .ToList();
                    }
                }

                return new OrderLine
                {
                    ArticleId = article.Id,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    SortOrder = idx,
                    ComponentAdditions = partAdditions,
                    ComponentOmissions = partOmissions
                };
            }).ToList();

            var order = new Order
            {
                CustomerId = customer.Id,
                Status = f.PickRandom(statuses),
                OrderLines = orderLines,
                ScheduledDate = f.Random.Bool(0.3f) ? DateTime.UtcNow.AddDays(f.Random.Int(1, 60)) : null,
            };

            orders.Add(order);
            await orderService.Save(order);
            if ((i + 1) % BatchSize == 0)
                await orderService.SaveChanges();
        }
        await orderService.SaveChanges();

        return orders;
    }
}
