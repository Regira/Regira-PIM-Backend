using Regira.Entities.Models.Abstractions;

namespace Webshop.Models.Catalog.Articles;

public class ArticleComponent : IEntityWithSerial
{
    public int Id { get; set; }
    public int AssemblyId { get; set; }
    public int ComponentId { get; set; }
    public decimal Quantity { get; set; }
    /// <summary>
    /// Indicates whether this component can be omitted from the article without affecting its core functionality.
    /// </summary>
    public bool IsOmittable { get; set; }

    public Article? Assembly { get; set; }
    public Article? Component { get; set; }
}