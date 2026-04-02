using Regira.Entities.Models.Abstractions;
using Webshop.Models.Taxonomy.Facets;

namespace Webshop.Models.Taxonomy.FacetGroups;

public abstract class FacetGroupLink : IEntityWithSerial
{
    public int Id { get; set; }
    public int FacetGroupId { get; set; }
    public int FacetId { get; set; }

    public FacetGroup? FacetGroup { get; set; }
    public Facet? Facet { get; set; }
}
public class FacetParentGroup : FacetGroupLink;
public class FacetChildGroup : FacetGroupLink;