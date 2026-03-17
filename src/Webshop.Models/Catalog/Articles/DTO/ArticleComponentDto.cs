namespace Webshop.Models.Catalog.Articles.DTO;

public class ArticleComponentDto
{
    public int Id { get; set; }
    public int AssemblyId { get; set; }
    public int ComponentId { get; set; }
    public decimal Quantity { get; set; }
    public bool IsOmittable { get; set; }
    public ArticleCoreDto? Component { get; set; }
}