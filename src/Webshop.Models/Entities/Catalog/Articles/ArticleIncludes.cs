namespace Webshop.Models.Entities.Catalog.Articles;

[Flags]
public enum ArticleIncludes
{
    None = 0,
    Categories = 1 << 0,
    Price = 1 << 1,
    PricePeriod = 1 << 2,
    Components = 1 << 3,
    AllowedComponentAdditions = 1 << 4,
    Suppliers = 1 << 5,
    All = Categories | Price | PricePeriod | Components | AllowedComponentAdditions | Suppliers
}
