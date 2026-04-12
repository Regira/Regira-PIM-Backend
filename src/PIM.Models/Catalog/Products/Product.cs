using PIM.Models.Catalog.Pricing.Abstractions;
using PIM.Models.Catalog.UnitTypes;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM.Models.Catalog.Products;

/// <summary>
/// A product can be any producible item. 
/// They can be categorized using Facets. The facets are usually more general than products and in plural form, while a Product usually represents a single item.
/// </summary>
public class Product : IEntityWithSerial, IHasDescription, IHasTimestamps, IHasPricePeriod, IHasPricePeriod<ProductPricePeriod>, IHasNormalizedTitle, IHasNormalizedContent, IArchivable
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Title { get; set; } = null!;
    [MaxLength(128), Normalized(SourceProperties = [nameof(Title)])]
    public string? NormalizedTitle { get; set; }
    [MaxLength(1024)]
    public string? Description { get; set; }
    [MaxLength(2048), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }

    public bool AllowAdditions { get; set; } = true;
    public int? UnitTypeId { get; set; }
    public decimal? DefaultQuantity { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsArchived { get; set; }

    public UnitType? UnitType { get; set; }
    public ICollection<ProductFacet>? Facets { get; set; }
    /// <summary>
    /// The collection of assemblies associated with this product.
    /// </summary>
    /// <remarks>
    /// Each assembly represents a relationship between this product and its components, 
    /// specifying the quantity and other details of the parts that make up the assembly.
    /// </remarks>
    public ICollection<ProductComponent>? Assemblies { get; set; } // parents
    /// <summary>
    /// The collection of components associated with this product.
    /// </summary>
    /// <remarks>
    /// Components represent the individual parts that make up this product. Each component is defined as an 
    /// <see cref="ProductComponent"/> and may include details such as quantity and whether it is omittable.
    /// </remarks>
    public ICollection<ProductComponent>? Components { get; set; } // children
    public ICollection<ProductAllowedComponentAddition>? AllowedComponentAdditions { get; set; }
    public ICollection<ProductSupplier>? Suppliers { get; set; }
    public ICollection<ProductPricePeriod>? Prices { get; set; }
    ICollection<IPricePeriod>? IHasPricePeriod.Prices
    {
        get => Prices?.Cast<IPricePeriod>().ToList();
        set => Prices = value?.Cast<ProductPricePeriod>().ToList();
    }

    /// <summary> Gets or sets the current price of the product, calculated from the price history. </summary>
    [NotMapped]
    public decimal? Price { get; set; }
}