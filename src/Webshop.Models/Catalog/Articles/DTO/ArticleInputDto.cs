using System.ComponentModel.DataAnnotations;
using Webshop.Models.Catalog.Pricing;

namespace Webshop.Models.Catalog.Articles.DTO;

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