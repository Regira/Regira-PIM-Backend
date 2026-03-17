using Regira.Entities.Models.Abstractions;
using Webshop.Models.Taxonomy.Facets;

namespace Webshop.Models.Catalog.Articles;

public class ArticleFacet : IEntityWithSerial
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int FacetId { get; set; }

    public Article? Article { get; set; }
    public Facet? Facet { get; set; }
}
