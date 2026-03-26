namespace Webshop.Models.Catalog.Articles;

[Flags]
public enum ArticleIncludes
{
    None = 0,
    Facets = 1 << 0,
    Price = 1 << 1,
    PricePeriod = 1 << 2,
    Assemblies = 1 << 3,
    Components = 1 << 4,
    AllowedComponentAdditions = 1 << 5,
    Suppliers = 1 << 6,
    All = Facets | Price | PricePeriod | Assemblies | Components | AllowedComponentAdditions | Suppliers
}
