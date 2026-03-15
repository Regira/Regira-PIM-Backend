using Regira.Entities.Models.Abstractions;

namespace Webshop.Models.Entities.Catalog.Articles;

public class ArticleAllowedComponentAddition : IEntityWithSerial
{
    public int Id { get; set; }
    public int AssemblyId { get; set; }
    public int ComponentId { get; set; }
    
    public Article? Assembly { get; set; }
    public Article? Component { get; set; }
}