using Regira.Entities.Models;

namespace Webshop.Models.Entities.Orders;

public class OrderSearchObject : SearchObject
{
    public string? Code { get; set; }
    public ICollection<int>? CustomerId { get; set; }
    public ICollection<int>? ProductId { get; set; }
    public bool? HasAdditions { get; set; }
    public bool? HasOmissions { get; set; }
    public ICollection<OrderStatus>? Status { get; set; }
    public DateTime? MinScheduledDate { get; set; }
    public DateTime? MaxScheduledDate { get; set; }
    public decimal? MinTotal { get; set; }
    public decimal? MaxTotal { get; set; }
}
