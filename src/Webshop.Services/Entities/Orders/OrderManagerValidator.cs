using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Webshop.Models.Orders;

namespace Webshop.Services.Entities.Orders;

public interface IOrderServiceValidator : IEntityService<Order, OrderSearchObject, OrderSortBy, OrderIncludes>
{
    void Validate(Order item);
}

public class OrderManagerValidator(IEntityRepository<Order, OrderSearchObject, OrderSortBy, OrderIncludes> service)
    : EntityWrappingServiceBase<Order, OrderSearchObject, OrderSortBy, OrderIncludes>(service), IOrderServiceValidator
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
        return base.Modify(item);
    }

    public override Task Save(Order item)
    {
        Validate(item);
        if (string.IsNullOrWhiteSpace(item.Code))
            item.Code = $"ORD{DateTime.UtcNow.Ticks:X}";
        return base.Save(item);
    }

    public void Validate(Order item)
    {
        if (item.OrderLines?.Any() != true)
        {
            throw new EntityInputException<Order>("Saving order failed")
            {
                InputErrors = { ["OrderLines"] = "Order must contain at least one order line." }
            };
        }
    }
}