namespace Webshop.Models.Entities.Stakeholders.Parties;

[Flags]
public enum PartyIncludes
{
    None = 0,
    ContactData = 1 << 0,
    Addresses = 1 << 1,
    Relationships = 1 << 2,
    All = ContactData | Addresses | Relationships
}
