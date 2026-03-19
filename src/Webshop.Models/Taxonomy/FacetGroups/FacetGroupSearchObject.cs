using Regira.Entities.Models;

namespace Webshop.Models.Taxonomy.FacetGroups;

public class FacetGroupSearchObject : SearchObject
{
    public ICollection<int>? FacetId { get; set; }
}
