using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Webshop.Models.Entities.Catalog.Products;
using Webshop.Services.Entities.Catalog.Pricing;

namespace Webshop.Services.Entities.Catalog.Products;

public interface IProductValidateManager : IEntityService<Product, ProductSearchObject, ProductSortBy, ProductIncludes>
{
    void Validate(Product item);
}

public class ProductValidateManager(IEntityRepository<Product, ProductSearchObject, ProductSortBy, ProductIncludes> service)
    : EntityWrappingServiceBase<Product, ProductSearchObject, ProductSortBy, ProductIncludes>(service), IProductValidateManager
{
    public override Task Add(Product item)
    {
        Validate(item);
        return base.Add(item);
    }
    public override Task<Product?> Modify(Product item)
    {
        Validate(item);
        return base.Modify(item);
    }
    public override Task Save(Product item)
    {
        Validate(item);
        return base.Save(item);
    }

    public void Validate(Product item)
    {
        var errors = new Dictionary<string, string>();

        // Pricing
        var priceErrors = item.Validate<Product, ProductPriceHistory>();
        foreach (var error in priceErrors)
        {
            errors.Add(error.Key, error.Value);
        }

        // Parts
        if (item.Parts?.Any(pp => pp.Quantity <= 0) == true)
        {
            errors.Add("Parts", "Product parts must have a quantity greater than zero.");
        }

        if (errors.Any())
        {
            throw new EntityInputException<Product>("Validation failed")
            {
                InputErrors = errors,
                Item = item
            };
        }
    }
}