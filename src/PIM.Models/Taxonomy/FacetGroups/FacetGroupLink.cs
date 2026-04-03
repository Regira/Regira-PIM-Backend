using PIM.Models.Taxonomy.Facets;
using Regira.Entities.Models.Abstractions;

namespace PIM.Models.Taxonomy.FacetGroups;

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