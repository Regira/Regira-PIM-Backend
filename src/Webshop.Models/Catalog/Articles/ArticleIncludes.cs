namespace Webshop.Models.Catalog.Articles;

[Flags]
public enum ArticleIncludes
{
    None = 0,
    Price = 1 << 0,
    PricePeriod = 1 << 1,
    Facets = 1 << 2,
    Components = 1 << 3,
    AllowedComponentAdditions = 1 << 4,
    Suppliers = 1 << 5,
    All = Facets | Price | PricePeriod | Components | AllowedComponentAdditions | Suppliers
}
