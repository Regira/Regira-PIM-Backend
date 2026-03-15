using Regira.Entities.Models.Abstractions;
using Webshop.Models.Entities.Catalog.Parts;

namespace Webshop.Models.Entities.Catalog.Products;

public class ProductPart : IEntityWithSerial
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int PartId { get; set; }
    public decimal Quantity { get; set; }
    /// <summary>
    /// Indicates whether this part can be omitted from the product without affecting its core functionality.
    /// </summary>
    public bool IsOmittable { get; set; }

    public Product? Product { get; set; }
    public Part? Part { get; set; }
}
