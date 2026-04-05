using PIM.Models.Taxonomy.FacetGroupFacets;

namespace PIM.Models.Taxonomy.Facets;

public class FacetCoreDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class FacetDto : FacetCoreDto
{
    public ICollection<ParentFacetDto>? ParentEntities { get; set; }
    public ICollection<ChildFacetDto>? ChildEntities { get; set; }

    public ICollection<FacetGroupFacetDto>? FacetParentGroups { get; set; }
    public ICollection<FacetGroupFacetDto>? FacetChildGroups { get; set; }
    public int? ProductCount { get; set; }
}
