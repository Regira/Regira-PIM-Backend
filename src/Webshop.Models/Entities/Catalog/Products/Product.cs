using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Webshop.Models.Entities.Catalog.Pricing.Abstractions;
using Webshop.Models.Entities.Catalog.UnitTypes;

namespace Webshop.Models.Entities.Catalog.Products;

public class Product : IEntityWithSerial, IHasDescription, IHasTimestamps, IHasPriceHistory, IHasPriceHistory<ProductPriceHistory>, IHasNormalizedTitle, IHasNormalizedContent, IArchivable
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
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsArchived { get; set; }
    public bool AllowAdditions { get; set; } = true;
    public int? UnitTypeId { get; set; }
    public UnitType? UnitType { get; set; }

    public ICollection<ProductCategory>? Categories { get; set; }
    public ICollection<ProductPart>? Parts { get; set; }
    public ICollection<ProductAllowedPartAddition>? AllowedPartAdditions { get; set; }
    public ICollection<ProductAllergen>? Allergens { get; set; }
    public ICollection<ProductPriceHistory>? Prices { get; set; }
    ICollection<IPriceHistory>? IHasPriceHistory.Prices
    {
        get => Prices?.Cast<IPriceHistory>().ToList();
        set => Prices = value?.Cast<ProductPriceHistory>().ToList();
    }

    /// <summary> Gets or sets the current price of the product, calculated from the price history. </summary>
    [NotMapped]
    public decimal? Price { get; set; }
}
