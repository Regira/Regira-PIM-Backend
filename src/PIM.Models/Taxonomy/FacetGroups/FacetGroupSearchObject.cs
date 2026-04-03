using Regira.Entities.Models;

namespace PIM.Models.Taxonomy.FacetGroups;

public class FacetGroupSearchObject : SearchObject
{
    public bool? IsRoot { get; set; }
    public ICollection<int>? ParentFacetId { get; set; }
    public ICollection<int>? ChildFacetId { get; set; }
    public ICollection<int>? FacetId { get; set; }
}
