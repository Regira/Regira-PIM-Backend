using FluentValidation;
using PIM.Models.Catalog.Products;
using PIM.Services.Entities.Abstractions;
using PIM.Services.Entities.Catalog.Abstractions;
using Regira.TreeList;

namespace PIM.Services.Entities.Catalog.Products;

public class ProductManager(IProductRepository service, IValidator<Product> validator)
    : PimEntityManager<Product, ProductSearchObject, ProductSortBy, ProductIncludes>(service, validator), IProductService
{
    public Task<TreeList<ProductTreeItem>> GetAncestors(IList<int> ids, int maxLevel = 9)
        => service.GetAncestors(ids, maxLevel);
    public Task<TreeList<ProductTreeItem>> GetOffspring(IList<int> ids, int maxLevel = 9)
        => service.GetOffspring(ids, maxLevel);
    public Task<TreeList<ProductTreeItem>> GetFamily(IList<int> ids, int maxLevel = 9)
        => service.GetFamily(ids, maxLevel);
}
