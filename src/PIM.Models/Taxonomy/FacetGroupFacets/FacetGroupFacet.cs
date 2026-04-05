using PIM.Models.Taxonomy.FacetGroups;
using PIM.Models.Taxonomy.Facets;
using Regira.Entities.Models.Abstractions;

namespace PIM.Models.Taxonomy.FacetGroupFacets;

public abstract class FacetGroupFacet : IEntityWithSerial
{
    public int Id { get; set; }
    public int FacetGroupId { get; set; }
    public int FacetId { get; set; }

    public FacetGroup? FacetGroup { get; set; }
    public Facet? Facet { get; set; }
}

public class FacetParentGroup : FacetGroupFacet;
public class FacetChildGroup : FacetGroupFacet;