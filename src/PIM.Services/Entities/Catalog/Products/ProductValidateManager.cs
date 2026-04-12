using PIM.Data;
using PIM.Models.Catalog.Products;
using PIM.Services.Entities.Catalog.Abstractions;
using PIM.Services.Entities.Catalog.Pricing;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Regira.TreeList;

namespace PIM.Services.Entities.Catalog.Products;

public class ProductValidateManager(IProductRepository service)
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
        var priceErrors = item.Validate<Product, ProductPricePeriod>();
        foreach (var error in priceErrors)
        {
            errors.Add(error.Key, error.Value);
        }

        // Components
        if (item.Components?.Any(ac => ac.Quantity <= 0) == true)
        {
            errors.Add("Components", "Product components must have a quantity greater than zero.");
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


    public Task<TreeList<ProductTreeItem>> GetAncestors(IList<int> ids, int maxLevel = 9)
        => service.GetAncestors(ids, maxLevel);
    public Task<TreeList<ProductTreeItem>> GetOffspring(IList<int> ids, int maxLevel = 9)
        => service.GetOffspring(ids, maxLevel);
    public Task<TreeList<ProductTreeItem>> GetFamily(IList<int> ids, int maxLevel = 9)
        => service.GetFamily(ids, maxLevel);
}