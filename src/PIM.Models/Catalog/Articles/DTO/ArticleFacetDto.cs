using PIM.Models.Taxonomy.Facets;

namespace PIM.Models.Catalog.Articles.DTO;

public class ArticleFacetDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int FacetId { get; set; }
    public FacetCoreDto? Facet { get; set; }
}
