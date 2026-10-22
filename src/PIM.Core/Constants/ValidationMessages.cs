namespace PIM.Core.Constants;

/// <summary>
/// Messages the entity validators refuse a write with. Each is a translation key the manager app pairs with its own
/// texts (public/data/translations.json); a <c>{name}</c> in a translation is filled from the error's args.
/// </summary>
public static class ValidationMessages
{
    public const string ComponentQuantity = "product.componentQuantity";
    public const string ComponentCycle = "product.componentCycle";
    public const string FacetHierarchyCycle = "facet.hierarchyCycle";
    public const string PartyRelationshipSelf = "party.relationshipSelf";
    public const string PartyRelationshipCycle = "party.relationshipCycle";
    public const string EndBeforeStart = "dates.endBeforeStart";
    public const string InvalidPhone = "contactData.invalidPhone";
    public const string InvalidEmail = "contactData.invalidEmail";
    public const string UnitTypeCodeTaken = "unitType.codeTaken";
    public const string UnitTypeInUse = "unitType.inUse";
    public const string RelationshipTypeInUse = "relationshipType.inUse";
}
