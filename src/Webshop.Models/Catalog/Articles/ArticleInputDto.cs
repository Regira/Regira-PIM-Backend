using System.ComponentModel.DataAnnotations;
using Webshop.Models.Catalog.Pricing;

namespace Webshop.Models.Catalog.Articles;

public class ArticleInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    public int? UnitTypeId { get; set; }
    public bool AllowAdditions { get; set; } = true;
    public ICollection<ArticleFacetInputDto>? Facets { get; set; }
    public ICollection<ArticleComponentInputDto>? Components { get; set; }
    public ICollection<ArticleAllowedComponentAdditionInputDto>? AllowedComponentAdditions { get; set; }
    public ICollection<ArticleSupplierInputDto>? Suppliers { get; set; }
    public ICollection<PricePeriodInputDto>? Prices { get; set; }
}

public class ArticleFacetInputDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int FacetId { get; set; }
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
