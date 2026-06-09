using Regira.Entities.Models;

namespace PIM.Models.Taxonomy.Facets;

public record FacetSearchObject : SearchObject
{
    public ICollection<int>? ParentId { get; set; }
    public ICollection<int>? ChildId { get; set; }
    public ICollection<int>? ParentGroupId { get; set; }
    public ICollection<int>? ChildGroupId { get; set; }

    public bool? IsRoot { get; set; }
    public bool? IsParent { get; set; }
    public bool? IsChild { get; set; }

    public ICollection<int>? AncestorId { get; set; }
    public ICollection<int>? OffspringId { get; set; }
    public ICollection<int>? RootId { get; set; }

    public ICollection<int>? AncestorGroupId { get; set; }
    public ICollection<int>? OffspringGroupId { get; set; }
    public ICollection<int>? RootGroupId { get; set; }
}
