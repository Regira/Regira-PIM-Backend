using Regira.Entities.Models;

namespace Webshop.Models.Entities.Taxonomy.Facets;

public class FacetSearchObject : SearchObject
{
    public ICollection<int>? ParentId { get; set; }
    public ICollection<int>? ChildId { get; set; }
    public bool? IsRoot { get; set; }
}
