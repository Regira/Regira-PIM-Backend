using Regira.Entities.Models.Abstractions;
using Webshop.Models.Entities.Catalog.Categories;

namespace Webshop.Models.Entities.Catalog.Products;

public class ProductCategory : IEntityWithSerial
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int CategoryId { get; set; }

    public Product? Product { get; set; }
    public Category? Category { get; set; }
}
