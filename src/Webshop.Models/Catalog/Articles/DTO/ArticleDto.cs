using Webshop.Models.Catalog.Pricing;
using Webshop.Models.Catalog.UnitTypes;

namespace Webshop.Models.Catalog.Articles.DTO;

public class ArticleDto : ArticleCoreDto
{
    public decimal? Price { get; set; }
    public int? UnitTypeId { get; set; }
    public UnitTypeDto? UnitType { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public bool AllowAdditions { get; set; }

    public ICollection<ArticleFacetDto>? Facets { get; set; }
    public ICollection<ArticleComponentDto>? Components { get; set; }
    public ICollection<ArticleAllowedComponentAdditionDto>? AllowedComponentAdditions { get; set; }
    public ICollection<ArticleSupplierDto>? Suppliers { get; set; }
    public ICollection<PricePeriodDto>? Prices { get; set; }
}