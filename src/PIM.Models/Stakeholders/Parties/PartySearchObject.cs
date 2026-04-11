using Regira.Entities.Models;

namespace PIM.Models.Stakeholders.Parties;

public class PartySearchObject : SearchObject
{
    public string? PartyType { get; set; }
    public string? Code { get; set; }
    public string? Query { get; set; }
    public ICollection<int>? RelationshipId { get; set; }
    public ICollection<int>? ProductIdSupplied { get; set; }

    public bool? IsRoot { get; set; }
    public ICollection<int>? AncestorId { get; set; }
    public ICollection<int>? OffspringId { get; set; }
    public ICollection<int>? RootId { get; set; }
}
