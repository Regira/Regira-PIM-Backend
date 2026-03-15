using Regira.Entities.EFcore.Extensions;
using Regira.Entities.EFcore.Preppers.Abstractions;
using Webshop.Data;
using Webshop.Models.Entities.Orders;

namespace Webshop.Services.Entities.Orders;

public class OrderPrepper(WebshopDbContext dbContext) : EntityPrepperBase<Order>
{
    public override Task Prepare(Order modified, Order? original)
    {
        modified.PrepareOrderLines(dbContext, original);
        modified.PreparePriceTotals();

        return Task.CompletedTask;
    }
}

public static class OrderLineComponentsPrepperExtensions
{
    public static void PrepareOrderLines(this Order item, WebshopDbContext dbContext, Order? original)
    {
        if (item.OrderLines?.Any() == true)
        {
            foreach (var line in item.OrderLines)
            {
                var originalLine = original?.OrderLines?.FirstOrDefault(ol => ol.Id == line.Id);
                if (originalLine != null)
                {
                    dbContext.UpdateRelatedCollection(line, originalLine, ol => ol.ComponentAdditions);
                    dbContext.UpdateRelatedCollection(line, originalLine, ol => ol.ComponentOmissions);
                }
            }
        }
    }
}
public static class OrderPriceTotalsPrepperExtensions
{
    public static void PreparePriceTotals(this Order item)
    {
        if (item.OrderLines?.Any() == true)
        {
            foreach (var line in item.OrderLines)
            {
                var partAddition = line.ComponentAdditions?.Sum(olp => olp.Price * olp.Quantity) ?? 0m;
                var partOmission = line.ComponentOmissions?.Sum(olp => olp.Price * olp.Quantity) ?? 0m;
                line.SubTotal = (line.UnitPrice + partAddition - partOmission) * line.Quantity;
            }
            item.Total = item.OrderLines.Sum(ol => ol.SubTotal);
        }
    }
}