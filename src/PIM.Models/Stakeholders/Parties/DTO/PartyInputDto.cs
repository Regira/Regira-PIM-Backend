using PIM.Core.Constants;
using PIM.Models.Stakeholders.Addresses;
using PIM.Models.Stakeholders.ContactData;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PIM.Models.Stakeholders.Parties.DTO;

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
    public ICollection<PartyRelationshipInputDto>? ParentRelationships { get; set; }
    public ICollection<PartyRelationshipInputDto>? ChildRelationships { get; set; }
}