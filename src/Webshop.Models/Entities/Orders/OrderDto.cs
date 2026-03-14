using Webshop.Models.Entities.Catalog.Parts;
using Webshop.Models.Entities.Catalog.Products;
using Webshop.Models.Entities.Clients.Customers;

namespace Webshop.Models.Entities.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public int CustomerId { get; set; }
    public CustomerDto? Customer { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<OrderLineDto>? OrderLines { get; set; }
}

public class OrderLineDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public ProductDto? Product { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
    public int SortOrder { get; set; }
    public ICollection<OrderLinePartDto>? PartAdditions { get; set; }
    public ICollection<OrderLinePartDto>? PartOmissions { get; set; }
}

public class OrderLinePartDto
{
    public int Id { get; set; }
    public int OrderLineId { get; set; }
    public int PartId { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public PartDto? Part { get; set; }
}
