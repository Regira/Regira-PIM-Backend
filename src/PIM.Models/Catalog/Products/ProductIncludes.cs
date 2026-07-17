namespace PIM.Models.Catalog.Products;

[Flags]
public enum ProductIncludes
{
    None = 0,
    Facets = 1 << 0,
    Components = 1 << 1,
    Suppliers = 1 << 2,
    All = Facets | Components | Suppliers
}
