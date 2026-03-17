namespace Webshop.Models.Taxonomy.Facets;

public class RelatedFacetDto
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int ParentId { get; set; }
}

public class ParentFacetDto : RelatedFacetDto
{
    public FacetCoreDto Parent { get; set; } = null!;
}

public class ChildFacetDto : RelatedFacetDto
{
    public FacetCoreDto Child { get; set; } = null!;
}
