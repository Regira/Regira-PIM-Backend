namespace PIM.Models.Stakeholders.Parties;

[Flags]
public enum PartyIncludes
{
    None = 0,
    ContactData = 1 << 0,
    Addresses = 1 << 1,
    Parents = 1 << 2,
    Children = 1 << 3,
    Relationships = Parents | Children,
    All = ContactData | Addresses | Relationships
}
