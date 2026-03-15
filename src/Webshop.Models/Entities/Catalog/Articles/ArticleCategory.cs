using Regira.Entities.Models.Abstractions;
using Webshop.Models.Entities.Catalog.Categories;

namespace Webshop.Models.Entities.Catalog.Articles;

public class ArticleCategory : IEntityWithSerial
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int CategoryId { get; set; }

    public Article? Article { get; set; }
    public Category? Category { get; set; }
}