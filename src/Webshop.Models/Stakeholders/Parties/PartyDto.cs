using System.Text.Json.Serialization;
using Webshop.Core.Constants;
using Webshop.Models.Stakeholders.Addresses;
using Webshop.Models.Stakeholders.ContactData;

namespace Webshop.Models.Stakeholders.Parties;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "partyType")]
[JsonDerivedType(typeof(PersonDto), PartyTypes.Person)]
[JsonDerivedType(typeof(OrganizationDto), PartyTypes.Organization)]
public abstract class PartyDto
{
    public int Id { get; set; }
    public string? Code { get; set; }

    public string? Title { get; set; }
    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<ContactDetailsDto>? ContactData { get; set; }
    public ICollection<AddressDto>? Addresses { get; set; }
}