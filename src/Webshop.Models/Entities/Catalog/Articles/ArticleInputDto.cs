using System.ComponentModel.DataAnnotations;
using Webshop.Models.Entities.Catalog.Pricing;

namespace Webshop.Models.Entities.Catalog.Articles;

public class ArticleInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    public int? UnitTypeId { get; set; }
    public bool AllowAdditions { get; set; } = true;
    public ICollection<ArticleCategoryInputDto>? Categories { get; set; }
    public ICollection<ArticleComponentInputDto>? Components { get; set; }
    public ICollection<ArticleAllowedComponentAdditionInputDto>? AllowedComponentAdditions { get; set; }
    public ICollection<ArticleSupplierInputDto>? Suppliers { get; set; }
    public ICollection<PriceHistoryInputDto>? Prices { get; set; }
}

public class ArticleCategoryInputDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int CategoryId { get; set; }
}

public class ArticleComponentInputDto
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public int ChildId { get; set; }
    public decimal Quantity { get; set; }
    public bool IsOmittable { get; set; }
}

public class ArticleAllowedComponentAdditionInputDto
{
    public int Id { get; set; }
    public int AssemblyId { get; set; }
    public int ComponentId { get; set; }
}

public class ArticleSupplierInputDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int SupplierId { get; set; }
}
