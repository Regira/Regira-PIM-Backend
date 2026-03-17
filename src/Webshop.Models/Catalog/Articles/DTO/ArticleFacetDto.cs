using Webshop.Models.Taxonomy.Facets;

namespace Webshop.Models.Catalog.Articles.DTO;

public class ArticleFacetDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int FacetId { get; set; }
    public FacetCoreDto? Facet { get; set; }
}
