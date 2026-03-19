using Regira.Entities.Models;

namespace Webshop.Models.Taxonomy.Facets;

public class FacetSearchObject : SearchObject
{
    public ICollection<int>? ParentId { get; set; }
    public ICollection<int>? ChildId { get; set; }
    public ICollection<int>? FacetGroupId { get; set; }
    public bool? IsRoot { get; set; }
}
