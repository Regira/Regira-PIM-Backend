using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Webshop.Core.Constants;
using Webshop.Models.Stakeholders.Addresses;
using Webshop.Models.Stakeholders.ContactData;

namespace Webshop.Models.Stakeholders.Parties;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "partyType")]
[JsonDerivedType(typeof(PersonInputDto), PartyTypes.Person)]
[JsonDerivedType(typeof(OrganizationInputDto), PartyTypes.Organization)]
public class PartyInputDto
{
    public int Id { get; set; }

    [MaxLength(16)]
    public string? Code { get; set; }

    [MaxLength(2048)]
    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public ICollection<ContactDetailsInputDto>? ContactData { get; set; }
    public ICollection<AddressInputDto>? Addresses { get; set; }
}