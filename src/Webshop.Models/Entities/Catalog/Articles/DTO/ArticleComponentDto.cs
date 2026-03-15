namespace Webshop.Models.Entities.Catalog.Articles.DTO;

public class ArticleComponentDto
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public int ChildId { get; set; }
    public decimal Quantity { get; set; }
    public bool IsOmittable { get; set; }
    public ArticleCoreDto? Component { get; set; }
}