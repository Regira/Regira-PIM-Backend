using System.ComponentModel.DataAnnotations;

namespace PIM.Models.Stakeholders.Parties;

public class OrganizationInputDto : PartyInputDto
{
    [Required, MaxLength(256)]
    public string Name { get; set; } = null!;
    [MaxLength(32)]
    public string? LegalEntity { get; set; }
}
