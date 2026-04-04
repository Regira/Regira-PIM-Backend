namespace PIM.Models.Catalog.Products.DTO;

public class ProductAllowedComponentAdditionDto
{
    public int Id { get; set; }
    public int AssemblyId { get; set; }
    public int ComponentId { get; set; }
    public ProductCoreDto? Component { get; set; }
}