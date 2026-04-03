using PIM.Core.Abstractions;
using PIM.Models.Orders;
using Regira.DAL.Paging;
using Regira.Entities.Services.Abstractions;

namespace PIM.Services.Entities.Orders;

public class OrderManagerPersonal(IOrderServiceValidator service, IOrderContext orderContext)
    : EntityWrappingServiceBase<Order, OrderSearchObject, OrderSortBy, OrderIncludes>(service)
{
    public override async Task<Order?> Details(int id)
    {
        var item = await base.Details(id);
        return item?.CustomerId == (orderContext.CustomerId ?? throw new InvalidOperationException("CustomerId is not available.")) ? item : null;
    }
    public override Task<IList<Order>> List(IList<OrderSearchObject?> searchObjects, IList<OrderSortBy> sortBy, OrderIncludes? includes = null, PagingInfo? pagingInfo = null)
    {
        ProcessSearchObjects(searchObjects);
        return base.List(searchObjects, sortBy, includes, pagingInfo);
    }
    public override Task<long> Count(IList<OrderSearchObject?> searchObjects)
    {
        ProcessSearchObjects(searchObjects);
        return base.Count(searchObjects);
    }

    public override Task Add(Order item)
    {
        item.CustomerId = orderContext.CustomerId ?? throw new InvalidOperationException("CustomerId is not available.");
        return base.Add(item);
    }
    public override Task<Order?> Modify(Order item)
    {
        return item.CustomerId != orderContext.CustomerId
            ? throw new UnauthorizedAccessException("You can only modify your own orders.")
            : base.Modify(item);
    }
    public override Task Save(Order item)
    {
        if (item.Id != 0 && item.CustomerId != orderContext.CustomerId)
            throw new UnauthorizedAccessException("You can only save your own orders.");

        item.CustomerId = orderContext.CustomerId ?? throw new InvalidOperationException("CustomerId is not available.");
        return base.Save(item);
    }

    public override Task Remove(Order item)
    {
        return item.CustomerId != orderContext.CustomerId
            ? throw new UnauthorizedAccessException("You can only remove your own orders.")
            : base.Remove(item);
    }

    void ProcessSearchObjects(IList<OrderSearchObject?> searchObjects)
    {
        foreach (var so in searchObjects)
        {
            so!.CustomerId = [orderContext.CustomerId ?? throw new InvalidOperationException("CustomerId is not available.")];
        }
    }
}