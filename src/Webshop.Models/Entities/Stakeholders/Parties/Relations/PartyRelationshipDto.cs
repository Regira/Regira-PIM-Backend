using Webshop.Models.Entities.Stakeholders.ContactData;

namespace Webshop.Models.Entities.Stakeholders.Parties.Relations;

public class PartyRelationshipDto
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int ParentId { get; set; }
    public int RelationshipTypeId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public RelationshipTypeDto? RelationshipType { get; set; }
    public ICollection<ContactDetailsDto>? ContactData { get; set; }
}
