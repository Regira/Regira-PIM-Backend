using Webshop.Models.Taxonomy.Facets;

namespace Webshop.Models.Taxonomy.FacetGroups;

public class FacetGroupLinkDto
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public int ChildId { get; set; }
    public FacetCoreDto? Child { get; set; }
}

public class FacetGroupLinkParentDto
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public int ChildId { get; set; }
    public FacetGroupCoreDto? Parent { get; set; }
}
