using Regira.Entities.Models.Abstractions;

namespace PIM.Models.Catalog.Products;

public class ProductComponent : IEntityWithSerial, ISortable
{
    public int Id { get; set; }
    public int AssemblyId { get; set; }
    public int ComponentId { get; set; }
    public decimal Quantity { get; set; }
    /// <summary>
    /// Indicates whether this component can be omitted from the product without affecting its core functionality.
    /// </summary>
    public bool IsOmittable { get; set; }
    public int SortOrder { get; set; }

    public Product? Assembly { get; set; }
    public Product? Component { get; set; }
}