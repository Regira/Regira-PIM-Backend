using PIM.Models.Taxonomy.Facets;
using Regira.Entities.Models.Abstractions;

namespace PIM.Models.Catalog.Products;

public class ProductFacet : IEntityWithSerial
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int FacetId { get; set; }

    public Product? Product { get; set; }
    public Facet? Facet { get; set; }
}
