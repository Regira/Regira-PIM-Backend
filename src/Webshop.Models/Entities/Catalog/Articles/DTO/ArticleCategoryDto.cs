using Webshop.Models.Entities.Classification.Categories;

namespace Webshop.Models.Entities.Catalog.Articles.DTO;

public class ArticleCategoryDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int CategoryId { get; set; }
    public CategoryCoreDto? Category { get; set; }
}