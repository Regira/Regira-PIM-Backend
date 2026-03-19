using Regira.Entities.Models.Abstractions;
using Webshop.Models.Taxonomy.Facets;

namespace Webshop.Models.Taxonomy.FacetGroups;

public class FacetGroupLink : IEntityWithSerial
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public int ChildId { get; set; }

    public FacetGroup? Parent { get; set; }
    public Facet? Child { get; set; }
}