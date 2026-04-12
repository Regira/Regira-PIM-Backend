using Regira.Entities.Models;

namespace PIM.Models.Catalog.Products;

public class ProductSearchObject : SearchObject
{
    public string? Title { get; set; }

    /// <summary>
    /// Filters products that belong to ANY of the specified facet IDs.
    /// </summary>
    public ICollection<int>? FacetId { get; set; }

    public bool? IsRoot { get; set; }
    public bool? IsComponent { get; set; }
    public bool? IsAssembly { get; set; }

    /// <summary>
    /// Filters products that belong to ANY of the specified assembly IDs.
    /// </summary>
    public ICollection<int>? AssemblyId { get; set; }
    /// <summary>
    /// Filters products that contain ANY of the specified component IDs.
    /// </summary>    
    public ICollection<int>? ComponentId { get; set; }
    /// <summary>
    /// Filters products that contain ALL of the specified component IDs.
    /// </summary>
    public ICollection<int>? AllComponentId { get; set; }

    public ICollection<int>? AncestorId { get; set; }
    public ICollection<int>? OffspringId { get; set; }

    /// <summary>
    /// Filters products by supplier IDs.
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
