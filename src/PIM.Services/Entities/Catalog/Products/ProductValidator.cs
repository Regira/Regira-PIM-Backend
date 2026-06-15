using FluentValidation;
using PIM.Models.Catalog.Products;

namespace PIM.Services.Entities.Catalog.Products;

public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(p => p.Components)
            .Must(components => components == null || components.All(c => c.Quantity > 0))
            .WithName("Components")
            .WithMessage("Product components must have a quantity greater than zero.");
    }
}
