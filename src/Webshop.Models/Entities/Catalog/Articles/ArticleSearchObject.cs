using Regira.Entities.Models;

namespace Webshop.Models.Entities.Catalog.Articles;

public class ArticleSearchObject : SearchObject
{
    /// <summary>
    /// Filters articles that belong to ANY of the specified category IDs.
    /// </summary>
    public ICollection<int>? CategoryId { get; set; }
    /// <summary>
    /// Filters articles that contain ANY of the specified component IDs.
    /// </summary>
    public ICollection<int>? ComponentId { get; set; }
    /// <summary>
    /// Filters articles that contain ALL of the specified component IDs.
    /// </summary>
    public ICollection<int>? AllComponentId { get; set; }
    /// <summary>
    /// Filters articles by supplier IDs.
    /// </summary>
    public ICollection<int>? SupplierId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    /// <summary>
    /// The date for which the price filters should be applied. If not specified, the current date will be used.
    /// </summary>
    public DateTime? PriceDate { get; set; }
    public bool? HasPrice { get; set; }
}
