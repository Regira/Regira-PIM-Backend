using Webshop.Models.Catalog.Articles.DTO;
using Webshop.Models.Stakeholders.Parties;

namespace Webshop.Models.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public int CustomerId { get; set; }
    public PartyDto? Customer { get; set; }
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
    public int ArticleId { get; set; }
    public ArticleCoreDto? Article { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
    public int SortOrder { get; set; }
    public ICollection<OrderLineComponentDto>? PartAdditions { get; set; }
    public ICollection<OrderLineComponentDto>? PartOmissions { get; set; }
}

public class OrderLineComponentDto
{
    public int Id { get; set; }
    public int OrderLineId { get; set; }
    public int ArticleId { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public ArticleCoreDto? Article { get; set; }
}
