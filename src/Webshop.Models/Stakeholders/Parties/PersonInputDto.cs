using System.ComponentModel.DataAnnotations;

namespace Webshop.Models.Stakeholders.Parties;

public class PersonInputDto : PartyInputDto
{
    [MaxLength(32)]
    public string? Salutation { get; set; }
    [Required, MaxLength(128)]
    public string? GivenName { get; set; }
    [MaxLength(128)]
    public string? MiddleName { get; set; }
    [Required, MaxLength(128)]
    public string? FamilyName { get; set; }
}
