namespace PIM.Models.Catalog.Products;

[Flags]
public enum ProductIncludes
{
    None = 0,
    Facets = 1 << 1,
    Components = 1 << 2,
    Suppliers = 1 << 3,
    All = Facets | Components | Suppliers
}
