namespace PIM.Models.Orders;

[Flags]
public enum OrderIncludes
{
    Default = 0,
    Customer = 1 << 0,
    OrderLines = 1 << 1,
    OrderLinesProducts = 1 << 2,
    OrderLinesProductComponents = 1 << 3,
    OrderLinesProductsWithComponents = OrderLinesProducts | OrderLinesProductComponents,
    All = Customer | OrderLinesProductsWithComponents
}
