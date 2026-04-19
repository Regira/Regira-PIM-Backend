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
    /// <summary>
    /// Is Unit of Measure, indicating that this unit type is used for measuring quantities, dimensions, or other attributes of products.<br />
    /// False when the unit type is categorized as a Countable item, such as Piece, Set, etc., which are used for counting discrete items rather than measuring continuous attributes.
    /// </summary>
    public bool IsUom { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    [MaxLength(128), Normalized(SourceProperties = [nameof(Title), nameof(Code)])]
    public string? NormalizedContent { get; set; }
}
