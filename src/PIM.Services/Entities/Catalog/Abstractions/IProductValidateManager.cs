using PIM.Models.Catalog.Products;

namespace PIM.Services.Entities.Catalog.Abstractions;

public interface IProductValidateManager : IProductService
{
    void Validate(Product item);
}