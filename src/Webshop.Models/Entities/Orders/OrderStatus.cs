namespace Webshop.Models.Entities.Orders;

public enum OrderStatus
{
    Pending = 0,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}
