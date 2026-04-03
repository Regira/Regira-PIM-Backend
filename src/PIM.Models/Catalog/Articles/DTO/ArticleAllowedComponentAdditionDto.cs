namespace PIM.Models.Catalog.Articles.DTO;

public class ArticleAllowedComponentAdditionDto
{
    public int Id { get; set; }
    public int AssemblyId { get; set; }
    public int ComponentId { get; set; }
    public ArticleCoreDto? Component { get; set; }
}