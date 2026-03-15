using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Webshop.Models.Entities.Catalog.Pricing.Abstractions;
using Webshop.Models.Entities.Catalog.UnitTypes;

namespace Webshop.Models.Entities.Catalog.Articles;

public class Article : IEntityWithSerial, IHasDescription, IHasTimestamps, IHasPriceHistory, IHasPriceHistory<ArticlePriceHistory>, IHasNormalizedTitle, IHasNormalizedContent, IArchivable
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
    public UnitType? UnitType { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsArchived { get; set; }

    public ICollection<ArticleCategory>? Categories { get; set; }
    /// <summary>
    /// The collection of assemblies associated with this article.
    /// </summary>
    /// <remarks>
    /// Each assembly represents a relationship between this article and its components, 
    /// specifying the quantity and other details of the parts that make up the assembly.
    /// </remarks>
    public ICollection<ArticleComponent>? Assemblies { get; set; } // parents
    /// <summary>
    /// The collection of components associated with this article.
    /// </summary>
    /// <remarks>
    /// Components represent the individual parts that make up this article. Each component is defined as an 
    /// <see cref="ArticleComponent"/> and may include details such as quantity and whether it is omittable.
    /// </remarks>
    public ICollection<ArticleComponent>? Components { get; set; } // children
    public ICollection<ArticleAllowedComponentAddition>? AllowedComponentAdditions { get; set; }
    public ICollection<ArticleSupplier>? Suppliers { get; set; }
    public ICollection<ArticlePriceHistory>? Prices { get; set; }
    ICollection<IPriceHistory>? IHasPriceHistory.Prices
    {
        get => Prices?.Cast<IPriceHistory>().ToList();
        set => Prices = value?.Cast<ArticlePriceHistory>().ToList();
    }

    /// <summary> Gets or sets the current price of the product, calculated from the price history. </summary>
    [NotMapped]
    public decimal? Price { get; set; }
}