using Regira.Entities.Models;

namespace Webshop.Models.Stakeholders.Parties;

public class PartySearchObject : SearchObject
{
    public string? PartyType { get; set; }
    public string? Code { get; set; }
    public string? Query { get; set; }
    public ICollection<int>? RelationshipId { get; set; }
}
