namespace Webshop.Models.Entities.Catalog.Products;

[Flags]
public enum ProductIncludes
{
    None = 0,
    Categories = 1 << 0,
    Price = 1 << 1,
    PriceHistory = 1 << 2,
    Parts = 1 << 3,
    Allergens = 1 << 4,
    AllowedPartAdditions = 1 << 5,
    All = Categories | Price | PriceHistory | Parts | Allergens | AllowedPartAdditions
}