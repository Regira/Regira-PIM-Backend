using Webshop.Models.Entities.Catalog.Allergens;
using Webshop.Models.Entities.Catalog.Categories;
using Webshop.Models.Entities.Catalog.Parts;
using Webshop.Models.Entities.Catalog.Pricing;
using Webshop.Models.Entities.Catalog.UnitTypes;

namespace Webshop.Models.Entities.Catalog.Products;

public class ProductDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? UnitTypeId { get; set; }
    public UnitTypeDto? UnitType { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<ProductCategoryDto>? Categories { get; set; }
    public ICollection<ProductPartDto>? Parts { get; set; }
    public ICollection<ProductAllergenDto>? Allergens { get; set; }
    public ICollection<PriceHistoryDto>? Prices { get; set; }
}

public class ProductCategoryDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public CategoryCoreDto? Category { get; set; }
}

public class ProductPartDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int PartId { get; set; }
    public decimal Quantity { get; set; }
    public PartDto? Part { get; set; }
}

public class ProductAllergenDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int AllergenId { get; set; }
    public AllergenDto? Allergen { get; set; }
}
