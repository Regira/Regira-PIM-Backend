using System.ComponentModel.DataAnnotations;
using Webshop.Models.Entities.Catalog.Pricing;

namespace Webshop.Models.Entities.Catalog.Products;

public class ProductInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    public int? UnitTypeId { get; set; }
    public ICollection<ProductCategoryInputDto>? Categories { get; set; }
    public ICollection<ProductPartInputDto>? Parts { get; set; }
    public ICollection<ProductAllergenInputDto>? Allergens { get; set; }
    public ICollection<PriceHistoryInputDto>? Prices { get; set; }
}

public class ProductCategoryInputDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
}

public class ProductPartInputDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int PartId { get; set; }
    public decimal Quantity { get; set; }
}

public class ProductAllergenInputDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int AllergenId { get; set; }
}
