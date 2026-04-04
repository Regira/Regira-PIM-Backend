using Regira.Entities.Models.Abstractions;

namespace PIM.Models.Catalog.Products;

public class ProductAllowedComponentAddition : IEntityWithSerial
{
    public int Id { get; set; }
    public int AssemblyId { get; set; }
    public int ComponentId { get; set; }
    
    public Product? Assembly { get; set; }
    public Product? Component { get; set; }
}