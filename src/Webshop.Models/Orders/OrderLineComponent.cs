using Regira.Entities.Models.Abstractions;
using Webshop.Models.Catalog.Articles;

namespace Webshop.Models.Orders;

public class OrderLineComponent : IEntityWithSerial
{
    public int Id { get; set; }
    public int OrderLineId { get; set; }
    public int ArticleId { get; set; }
    /// <summary>
    /// Price per unit of the article at the time of the order. This is stored here to keep a record of the price even if it changes in the future.
    /// </summary>
    public decimal Price { get; set; }
    public decimal Quantity { get; set; } = 1;

    public Article? Article { get; set; }
    public OrderLine? OrderLine { get; set; }
}
public class OrderLineComponentAddition : OrderLineComponent;
public class OrderLineComponentOmission : OrderLineComponent;
