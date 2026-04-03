using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace PIM.Models.Catalog.UnitTypes;

public class UnitType : IEntityWithSerial, IHasCode, IHasTitle, IHasNormalizedContent, IHasTimestamps
{
    public int Id { get; set; }
    [Required, MaxLength(16)]
    public string? Code { get; set; }
    [Required, MaxLength(64)]
    public string? Title { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    [MaxLength(128), Normalized(SourceProperties = [nameof(Title), nameof(Code)])]
    public string? NormalizedContent { get; set; }
}
