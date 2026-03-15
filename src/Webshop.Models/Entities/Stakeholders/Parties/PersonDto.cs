namespace Webshop.Models.Entities.Stakeholders.Parties;

public class PersonDto : PartyDto
{
    public string? Salutation { get; set; }
    public string? GivenName { get; set; }
    public string? MiddleName { get; set; }
    public string? FamilyName { get; set; }
}