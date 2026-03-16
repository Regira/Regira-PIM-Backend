using Regira.Entities.Models.Abstractions;

namespace Webshop.Models.Entities.Taxonomy.Facets;

public class RelatedFacet : IEntityWithSerial
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int ParentId { get; set; }

    public Facet? Child { get; set; }
    public Facet? Parent { get; set; }
}
