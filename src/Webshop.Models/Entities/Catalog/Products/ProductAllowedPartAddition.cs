using Regira.Entities.Models.Abstractions;
using Webshop.Models.Entities.Catalog.Parts;

namespace Webshop.Models.Entities.Catalog.Products;

public class ProductAllowedPartAddition : IEntityWithSerial
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int PartId { get; set; }
    public Product? Product { get; set; }
    public Part? Part { get; set; }
}
