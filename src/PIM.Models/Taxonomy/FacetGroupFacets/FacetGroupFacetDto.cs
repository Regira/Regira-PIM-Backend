using PIM.Models.Taxonomy.FacetGroups;
using PIM.Models.Taxonomy.Facets;

namespace PIM.Models.Taxonomy.FacetGroupFacets;

public class FacetGroupFacetDto
{
    public int Id { get; set; }
    public int FacetGroupId { get; set; }
    public int FacetId { get; set; }
    public FacetCoreDto? Facet { get; set; }
    public FacetGroupCoreDto? FacetGroup { get; set; }
}