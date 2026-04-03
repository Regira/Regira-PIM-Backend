using PIM.Models.Taxonomy.Facets;

namespace PIM.Models.Taxonomy.FacetGroups;

public class FacetGroupLinkDto
{
    public int Id { get; set; }
    public int FacetGroupId { get; set; }
    public int FacetId { get; set; }
    public FacetCoreDto? Facet { get; set; }
}

public class FacetGroupLinkParentDto
{
    public int Id { get; set; }
    public int FacetGroupId { get; set; }
    public int FacetId { get; set; }
    public FacetGroupCoreDto? FacetGroup { get; set; }
}
