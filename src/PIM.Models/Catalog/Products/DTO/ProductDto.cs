namespace PIM.Models.Catalog.Products.DTO;

public class ProductDto : ProductCoreDto
{
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    // IArchivable: DELETE soft-deletes, so the flag must round-trip or the row cannot be restored.
    public bool IsArchived { get; set; }

    public ICollection<ProductFacetDto>? Facets { get; set; }
    public ICollection<ProductComponentDto>? Components { get; set; }
    public ICollection<ProductSupplierDto>? Suppliers { get; set; }
}