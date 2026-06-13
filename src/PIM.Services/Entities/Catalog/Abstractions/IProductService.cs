using PIM.Models.Catalog.Products;
using Regira.Entities.Services.Abstractions;
using Regira.TreeList;

namespace PIM.Services.Entities.Catalog.Abstractions;

public interface IProductService : IEntityService<Product, ProductSearchObject, ProductSortBy, ProductIncludes>
{
    Task<TreeList<ProductTreeItem>> GetAncestors(IList<int> ids, int maxLevel = 9);
    Task<TreeList<ProductTreeItem>> GetOffspring(IList<int> ids, int maxLevel = 9);
    Task<TreeList<ProductTreeItem>> GetFamily(IList<int> ids, int maxLevel = 9);
}