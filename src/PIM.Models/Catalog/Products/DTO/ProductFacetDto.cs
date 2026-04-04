using PIM.Models.Taxonomy.Facets;

namespace PIM.Models.Catalog.Products.DTO;

public class ProductFacetDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int FacetId { get; set; }
    public FacetCoreDto? Facet { get; set; }
}
