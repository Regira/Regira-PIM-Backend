namespace Webshop.Models.Entities.Catalog.Articles.DTO;

public class ArticleCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
}