using Regira.Entities.Models.Abstractions;
using Webshop.Models.Entities.Catalog.Parts;

namespace Webshop.Models.Entities.Orders;

public class OrderLinePart : IEntityWithSerial
{
    public int Id { get; set; }
    public int OrderLineId { get; set; }
    public int PartId { get; set; }
    /// <summary>
    /// Price per unit of the part at the time of the order. This is stored here to keep a record of the price even if it changes in the future.
    /// </summary>
    public decimal Price { get; set; }
    public decimal Quantity { get; set; } = 1;

    public Part? Part { get; set; }
    public OrderLine? OrderLine { get; set; }
}
public class OrderLinePartAddition : OrderLinePart;
public class OrderLinePartOmission : OrderLinePart;