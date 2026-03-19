namespace Webshop.Models.Taxonomy.FacetGroups;

public class FacetGroupCoreDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class FacetGroupDto : FacetGroupCoreDto
{
    public ICollection<FacetGroupLinkDto>? Facets { get; set; }
}
