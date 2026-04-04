using PIM.Models.Catalog.Pricing;

namespace PIM.Models.Catalog.Products.DTO;

public class ProductDto : ProductCoreDto
{
    public decimal? Price { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public bool AllowAdditions { get; set; }

    public ICollection<ProductFacetDto>? Facets { get; set; }
    public ICollection<ProductComponentDto>? Components { get; set; }
    public ICollection<ProductAllowedComponentAdditionDto>? AllowedComponentAdditions { get; set; }
    public ICollection<ProductSupplierDto>? Suppliers { get; set; }
    public ICollection<PricePeriodDto>? Prices { get; set; }
}