namespace PIM.Models.Catalog.Products.DTO;

public class ProductComponentDto
{
    public int Id { get; set; }
    public int AssemblyId { get; set; }
    public int ComponentId { get; set; }
    public decimal Quantity { get; set; }
    public bool IsOmittable { get; set; }
    public ProductCoreDto? Component { get; set; }
}