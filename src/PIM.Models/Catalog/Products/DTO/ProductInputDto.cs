using System.ComponentModel.DataAnnotations;
using PIM.Models.Catalog.Pricing;

namespace PIM.Models.Catalog.Products.DTO;

public class ProductInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    public int? UnitTypeId { get; set; }
    public decimal? DefaultQuantity { get; set; }
    public bool AllowAdditions { get; set; } = true;
    public ICollection<ProductFacetInputDto>? Facets { get; set; }
    public ICollection<ProductComponentInputDto>? Components { get; set; }
    public ICollection<ProductAllowedComponentAdditionInputDto>? AllowedComponentAdditions { get; set; }
    public ICollection<ProductSupplierInputDto>? Suppliers { get; set; }
    public ICollection<PricePeriodInputDto>? Prices { get; set; }
}