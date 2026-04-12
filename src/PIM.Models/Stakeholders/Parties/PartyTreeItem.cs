namespace PIM.Models.Stakeholders.Parties;

public class PartyTreeItem
{
    public int ParentId { get; set; }
    public int ChildId { get; set; }
    public int RelationshipTypeId { get; set; }
    public int Level { get; set; }
    public int RootId { get; set; }

    //public Party? Parent { get; set; }
    //public Party? Child { get; set; }
    //public Party? Root { get; set; }
    //public RelationshipType? RelationshipType { get; set; } 
}