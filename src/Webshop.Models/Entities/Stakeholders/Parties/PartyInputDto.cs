using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Webshop.Models.Entities.Stakeholders.Addresses;
using Webshop.Models.Entities.Stakeholders.ContactData;

namespace Webshop.Models.Entities.Stakeholders.Parties;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "partyType")]
[JsonDerivedType(typeof(PersonInputDto), "PERSON")]
[JsonDerivedType(typeof(OrganizationInputDto), "ORGANIZATION")]
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