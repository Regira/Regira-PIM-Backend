namespace Webshop.Models.Entities.Orders;

[Flags]
public enum OrderIncludes
{
    Default = 0,
    Customer = 1 << 0,
    OrderLines = 1 << 1,
    OrderLinesProducts = 1 << 2,
    OrderLinesParts = 1 << 3,
    OrderLinesProductsParts = OrderLinesProducts | OrderLinesParts,
    All = Customer | OrderLinesProductsParts
}
