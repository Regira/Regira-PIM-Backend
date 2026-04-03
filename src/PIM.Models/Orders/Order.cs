using System.ComponentModel.DataAnnotations;
using PIM.Models.Stakeholders.Parties;
using Regira.Entities.Models.Abstractions;

namespace PIM.Models.Orders;

public class Order : IEntityWithSerial, IHasCode, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [MaxLength(32)] public string? Code { get; set; }
    public int CustomerId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Total { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    [MaxLength(512)]
    public string? NormalizedContent { get; set; }

    public ICollection<OrderLine>? OrderLines { get; set; }
    public Party? Customer { get; set; }
}
