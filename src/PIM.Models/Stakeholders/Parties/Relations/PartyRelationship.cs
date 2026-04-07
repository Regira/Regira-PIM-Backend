using PIM.Models.Stakeholders.ContactData;
using Regira.Entities.Models.Abstractions;

namespace PIM.Models.Stakeholders.Parties.Relations;

public class PartyRelationship : IEntityWithSerial, IHasStartEndDate, ISortable, IHasContactData, IHasContactData<PartyRelationshipContactDetails>
{
    public int Id { get; set; }

    public int ChildId { get; set; }
    public int ParentId { get; set; }
    public int RelationshipTypeId { get; set; }

    // when did this relationship start/end?
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public Party? Parent { get; set; }
    public Party? Child { get; set; }
    public RelationshipType? RelationshipType { get; set; }

    public ICollection<PartyRelationshipContactDetails>? ContactData { get; set; }
    public int SortOrder { get; set; }
    ICollection<IContactDetails>? IHasContactData.ContactData
    {
        get => ContactData?.Cast<IContactDetails>().ToList();
        set => ContactData = value?.Cast<PartyRelationshipContactDetails>().ToList();
    }
}