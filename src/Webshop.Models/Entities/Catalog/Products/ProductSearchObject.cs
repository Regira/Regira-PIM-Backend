using Regira.Entities.Models;

namespace Webshop.Models.Entities.Catalog.Products;

public class ProductSearchObject : SearchObject
{
    public ICollection<int>? CategoryId { get; set; }
    public ICollection<int>? PartId { get; set; }
    public ICollection<int>? AllergenId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    /// <summary>
    /// The date for which the price filters should be applied. If not specified, the current date will be used.
    /// </summary>
    public DateTime? PriceDate { get; set; }
    public bool? HasPrice { get; set; }
}
