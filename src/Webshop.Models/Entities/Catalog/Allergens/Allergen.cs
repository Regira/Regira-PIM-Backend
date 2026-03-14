using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Webshop.Models.Entities.Catalog.Allergens;

public class Allergen : IEntityWithSerial, IHasTimestamps, IHasCode, IHasDescription, IHasNormalizedTitle, IHasNormalizedContent
{
    public int Id { get; set; }
    [MaxLength(32)]
    public string? Code { get; set; }
    [Required, MaxLength(64)]
    public string? Title { get; set; } = null!;
    [MaxLength(512)]
    public string? Description { get; set; }
    [MaxLength(64), Normalized(SourceProperties = [nameof(Title), nameof(Code)])]
    public string? NormalizedTitle { get; set; }
    [MaxLength(512), Normalized(SourceProperties = [nameof(Title), nameof(Code), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
