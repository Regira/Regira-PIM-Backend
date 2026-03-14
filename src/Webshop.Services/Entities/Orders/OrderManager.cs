using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Webshop.Models.Entities.Orders;

namespace Webshop.Services.Entities.Orders;

public interface IOrderService : IEntityService<Order, OrderSearchObject, OrderSortBy, OrderIncludes>;

public class OrderManager(IEntityRepository<Order, OrderSearchObject, OrderSortBy, OrderIncludes> service)
    : EntityWrappingServiceBase<Order, OrderSearchObject, OrderSortBy, OrderIncludes>(service), IOrderService
{
    public override Task Add(Order item)
    {
        Validate(item);
        if (string.IsNullOrWhiteSpace(item.Code))
            item.Code = $"ORD-{DateTime.UtcNow.Ticks:X}";
        return base.Add(item);
    }

    public override Task<Order?> Modify(Order item)
    {
        Validate(item);
        ValidateModifiable(item);
        return base.Modify(item);
    }

    public override Task Save(Order item)
    {
        Validate(item);
        if (item.Id != 0)
            ValidateModifiable(item);
        if (string.IsNullOrWhiteSpace(item.Code))
            item.Code = $"ORD-{DateTime.UtcNow.Ticks:X}";
        return base.Save(item);
    }

    private static void Validate(Order item)
    {
        if (item.OrderLines?.Any() != true)
            throw new EntityInputException<Order>("Saving order failed")
            {
                InputErrors = { ["OrderLines"] = "Order must contain at least one order line." }
            };
    }

    private static void ValidateModifiable(Order item)
    {
        if (item.ScheduledDate.HasValue && item.ScheduledDate.Value < DateTime.UtcNow)
            throw new EntityInputException<Order>("Modifying order failed")
            {
                InputErrors = { ["ScheduledDate"] = "Past scheduled orders cannot be modified." }
            };
    }
}
