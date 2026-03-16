using Webshop.Models.Entities.Taxonomy.Facets;

namespace Webshop.Models.Entities.Catalog.Articles.DTO;

public class ArticleFacetDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int FacetId { get; set; }
    public FacetCoreDto? Facet { get; set; }
}
