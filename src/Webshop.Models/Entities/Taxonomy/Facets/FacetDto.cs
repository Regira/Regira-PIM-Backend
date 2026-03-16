namespace Webshop.Models.Entities.Taxonomy.Facets;

public class FacetCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class FacetDto : FacetCoreDto
{
    public ICollection<ParentFacetDto>? ParentEntities { get; set; }
    public ICollection<ChildFacetDto>? ChildEntities { get; set; }
    public int? ProductCount { get; set; }
}
