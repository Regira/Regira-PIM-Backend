using PIM.Models.Taxonomy.Facets;
using Regira.Entities.Models.Abstractions;

namespace PIM.Models.Catalog.Articles;

public class ArticleFacet : IEntityWithSerial
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int FacetId { get; set; }

    public Article? Article { get; set; }
    public Facet? Facet { get; set; }
}
