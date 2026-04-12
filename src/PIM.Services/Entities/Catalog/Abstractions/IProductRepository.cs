using PIM.Models.Catalog.Products;
using Regira.Entities.Services.Abstractions;

namespace PIM.Services.Entities.Catalog.Abstractions;

public interface IProductRepository : IProductService, IEntityRepository<Product, ProductSearchObject, ProductSortBy, ProductIncludes>;