using Regira.Normalizing;
using System.ComponentModel.DataAnnotations;

namespace Webshop.Models.Entities.Stakeholders.Parties;

public class Organization() : Party("ORGANIZATION")
{
    [Required, MaxLength(256)]
    public string Name { get; set; } = null!;
    [MaxLength(32)]
    public string? LegalEntity { get; set; }
    [Normalized(SourceProperty = nameof(Name))]
    public override string? NormalizedTitle { get; set; }

    public override string Title => Name;
}