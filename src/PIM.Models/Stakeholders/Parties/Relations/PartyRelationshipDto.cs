using PIM.Models.Stakeholders.ContactData;

namespace PIM.Models.Stakeholders.Parties.Relations;

public class PartyRelationshipDto
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int ParentId { get; set; }
    public int RelationshipTypeId { get; set; }
    public int SortOrder { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public RelationshipTypeDto? RelationshipType { get; set; }
    public ICollection<ContactDetailsDto>? ContactData { get; set; }
}

public class PartyParentDto : PartyRelationshipDto
{
    public PartyCoreDto? Parent { get; set; }
}
public class PartyChildDto : PartyRelationshipDto
{
    public PartyCoreDto? Child { get; set; }
}