using Regira.Entities.Models.Abstractions;
using Webshop.Models.Entities.Catalog.Allergens;

namespace Webshop.Models.Entities.Catalog.Products;

public class ProductAllergen : IEntityWithSerial
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int AllergenId { get; set; }

    public Product? Product { get; set; }
    public Allergen? Allergen { get; set; }
}
