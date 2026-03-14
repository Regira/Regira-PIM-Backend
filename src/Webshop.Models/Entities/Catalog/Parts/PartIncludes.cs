namespace Webshop.Models.Entities.Catalog.Parts;

[Flags]
public enum PartIncludes
{
    None = 0,
    Price = 1 << 0,
    PriceHistory = 1 << 1,
}
