using Bogus;
using Microsoft.Extensions.Logging;
using PIM.Models.Catalog.Products;
using PIM.Models.Orders;
using PIM.Models.Stakeholders.Parties;
using Regira.Entities.Services.Abstractions;

namespace PIM.DataGenerator.Infrastructure;

public class OrderSeeder(
    IEntityService<Order> orderService,
    ILogger<OrderSeeder> logger)
{
    private const int BatchSize = 100;

    public async Task<IList<Order>> SeedAsync(IList<Party> customers, IList<Product> products)
    {
        return await SeedOrders(customers, products);
    }

    public async Task<IList<Order>> SeedOrders(IList<Party> customers, IList<Product> products)
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

        // Collect component product ids for additions/omissions
        var componentProducts = products
            .SelectMany(a => a.Components?.Select(c => c.ComponentId) ?? [])
            .Distinct()
            .ToList();

        logger.LogInformation("Seeding orders...");
        for (int i = 0; i < 10000; i++)
        {
            var customer = f.PickRandom(customers);
            var selectedProducts = f.PickRandom(products, f.Random.Int(1, 5)).DistinctBy(a => a.Id).ToList();

            var orderLines = selectedProducts.Select((product, idx) =>
            {
                var unitPrice = product.Prices?.FirstOrDefault()?.Price ?? 0m;
                var quantity = (decimal)f.Random.Int(1, 3);

                List<OrderLineComponentAddition>? partAdditions = null;
                if (f.Random.Bool(0.4f) && componentProducts.Count > 0)
                {
                    partAdditions = f.PickRandom(componentProducts, f.Random.Int(1, Math.Min(3, componentProducts.Count)))
                        .Distinct()
                        .Select(id => new OrderLineComponentAddition { ProductId = id, Price = f.Random.Decimal(0.15m, 1.50m) })
                        .ToList();
                }

                List<OrderLineComponentOmission>? partOmissions = null;
                if (product.Components?.Count > 0 && f.Random.Bool(0.3f))
                {
                    var omittable = product.Components.Where(c => c.IsOmittable).Select(c => c.ComponentId).ToList();
                    if (omittable.Count > 0)
                    {
                        partOmissions = f.PickRandom(omittable, f.Random.Int(1, Math.Min(2, omittable.Count)))
                            .Distinct()
                            .Select(id => new OrderLineComponentOmission { ProductId = id, Price = f.Random.Decimal(0.15m, 1.50m) })
                            .ToList();
                    }
                }

                return new OrderLine
                {
                    ProductId = product.Id,
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
