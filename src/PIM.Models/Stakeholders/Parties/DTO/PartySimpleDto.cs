using PIM.Models.Stakeholders.Addresses;
using PIM.Models.Stakeholders.ContactData;
using PIM.Models.Stakeholders.Parties.Abstractions;

namespace PIM.Models.Stakeholders.Parties.DTO;

public class PartySimpleDto : IPartyCore
{
    public int Id { get; set; }
    public string PartyType { get; set; } = null!;
    public string? Code { get; set; }

    //// Organization
    //public string? Name { get; set; }
    //public string? LegalEntity { get; set; }
    //// Person
    //public string? Salutation { get; set; }
    //public string? GivenName { get; set; }
    //public string? MiddleName { get; set; }
    //public string? FamilyName { get; set; }

    public string? Title { get; set; }
    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<ContactDetailsDto>? ContactData { get; set; }
    public ICollection<AddressDto>? Addresses { get; set; }
}