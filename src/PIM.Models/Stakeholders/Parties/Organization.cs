using System.ComponentModel.DataAnnotations;
using PIM.Core.Constants;
using Regira.Normalizing;

namespace PIM.Models.Stakeholders.Parties;

public class Organization() : Party(PartyTypes.Organization)
{
    [Required, MaxLength(256)]
    public string Name { get; set; } = null!;
    [MaxLength(32)]
    public string? LegalEntity { get; set; }
    [Normalized(SourceProperty = nameof(Name))]
    public override string? NormalizedTitle { get; set; }

    public override string Title => Name;
}