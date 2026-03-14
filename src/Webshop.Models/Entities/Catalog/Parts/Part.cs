using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Webshop.Models.Entities.Catalog.Pricing.Abstractions;
using Webshop.Models.Entities.Catalog.UnitTypes;

namespace Webshop.Models.Entities.Catalog.Parts;

public class Part : IEntityWithSerial, IHasCode, IHasTitle, IHasDescription, IHasTimestamps, IHasPriceHistory, IHasPriceHistory<PartPriceHistory>, IHasNormalizedContent, IArchivable
{
    public int Id { get; set; }
    [MaxLength(32)]
    public string? Code { get; set; }
    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;
    [MaxLength(512)]
    public string? Description { get; set; }
    [MaxLength(512), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsArchived { get; set; }
    public int? UnitTypeId { get; set; }
    public UnitType? UnitType { get; set; }

    public ICollection<PartPriceHistory>? Prices { get; set; }
    ICollection<IPriceHistory>? IHasPriceHistory.Prices
    {
        get => Prices?.Cast<IPriceHistory>().ToList();
        set => Prices = value?.Cast<PartPriceHistory>().ToList();
    }

    /// <summary> Gets or sets the current price of the product, calculated from the price history. </summary>
    [NotMapped]
    public decimal? Price { get; set; }
}
