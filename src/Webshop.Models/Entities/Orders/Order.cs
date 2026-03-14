using Regira.Entities.Models.Abstractions;
using System.ComponentModel.DataAnnotations;
using Webshop.Models.Entities.Clients.Customers;

namespace Webshop.Models.Entities.Orders;

public class Order : IEntityWithSerial, IHasCode, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [MaxLength(32)] public string? Code { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Total { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    [MaxLength(512)]
    public string? NormalizedContent { get; set; }

    public ICollection<OrderLine>? OrderLines { get; set; }
}
