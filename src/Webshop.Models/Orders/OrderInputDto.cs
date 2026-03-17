using System.ComponentModel.DataAnnotations;

namespace Webshop.Models.Orders;

public class OrderInputDto
{
    public int Id { get; set; }
    [MaxLength(16)] public string? Code { get; set; }
    public int CustomerId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime? ScheduledDate { get; set; }
    public ICollection<OrderLineInputDto>? OrderLines { get; set; }
}

public class OrderLineInputDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ArticleId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public ICollection<OrderLineComponentInputDto>? PartAdditions { get; set; }
    public ICollection<OrderLineComponentInputDto>? PartOmissions { get; set; }
}

public class OrderLineComponentInputDto
{
    public int Id { get; set; }
    public int OrderLineId { get; set; }
    public int ArticleId { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; } = 1;
}
