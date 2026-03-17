namespace Webshop.Models.Orders;

public enum OrderStatus
{
    Pending = 0,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}
