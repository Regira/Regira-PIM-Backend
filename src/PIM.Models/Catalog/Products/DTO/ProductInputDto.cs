using System.ComponentModel.DataAnnotations;

namespace PIM.Models.Catalog.Products.DTO;

public class ProductInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    // Must stay on the input DTO: it is the only way a payload can clear the archived flag (restore).
    public bool IsArchived { get; set; }
    // Send back the token that was read: a stale one answers 409 Conflict
    public Guid ConcurrencyToken { get; set; }
    public int? UnitTypeId { get; set; }
    public decimal? DefaultQuantity { get; set; }
    public bool AllowAdditions { get; set; } = true;
    public ICollection<ProductFacetInputDto>? Facets { get; set; }
    public ICollection<ProductComponentInputDto>? Components { get; set; }
    public ICollection<ProductSupplierInputDto>? Suppliers { get; set; }
}