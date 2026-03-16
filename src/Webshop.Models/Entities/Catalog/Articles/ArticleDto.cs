using Webshop.Models.Entities.Catalog.Pricing;
using Webshop.Models.Entities.Catalog.UnitTypes;
using Webshop.Models.Entities.Classification.Categories;

namespace Webshop.Models.Entities.Catalog.Articles;

public class ArticleCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? UnitTypeId { get; set; }
    public UnitTypeDto? UnitType { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class ArticleDto : ArticleCoreDto
{
    public bool AllowAdditions { get; set; }
    public ICollection<ArticleCategoryDto>? Categories { get; set; }
    public ICollection<ArticleComponentDto>? Components { get; set; }
    public ICollection<ArticleAllowedComponentAdditionDto>? AllowedComponentAdditions { get; set; }
    public ICollection<ArticleSupplierDto>? Suppliers { get; set; }
    public ICollection<PricePeriodDto>? Prices { get; set; }
}

public class ArticleCategoryDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int CategoryId { get; set; }
    public CategoryCoreDto? Category { get; set; }
}

public class ArticleComponentDto
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public int ChildId { get; set; }
    public decimal Quantity { get; set; }
    public bool IsOmittable { get; set; }
    public ArticleCoreDto? Component { get; set; }
}

public class ArticleAllowedComponentAdditionDto
{
    public int Id { get; set; }
    public int AssemblyId { get; set; }
    public int ComponentId { get; set; }
    public ArticleCoreDto? Component { get; set; }
}

public class ArticleSupplierDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int SupplierId { get; set; }
    public ArticleSupplierPartyDto? Supplier { get; set; }
}

public class ArticleSupplierPartyDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
}
