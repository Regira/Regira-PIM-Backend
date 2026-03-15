using Regira.Entities.Models;

namespace Webshop.Models.Entities.Catalog.Products;

public class ProductSearchObject : SearchObject
{
    /// <summary>
    /// Filters products that belong to ANY of the specified category IDs.
    /// </summary>
    public ICollection<int>? CategoryId { get; set; }
    /// <summary>
    /// Filters products that contain ANY of the specified part IDs.
    /// </summary>
    public ICollection<int>? PartId { get; set; }
    /// <summary>
    /// Filters products that contain ALL of the specified part IDs.
    /// </summary>
    public ICollection<int>? AllPartId { get; set; }
    /// <summary>
    /// Filters products that contain ANY of the specified allergen IDs.
    /// </summary>
    public ICollection<int>? AllergenId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    /// <summary>
    /// The date for which the price filters should be applied. If not specified, the current date will be used.
    /// </summary>
    public DateTime? PriceDate { get; set; }
    public bool? HasPrice { get; set; }
}
