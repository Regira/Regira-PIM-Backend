using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Webshop.Models.Entities.Catalog.Parts;
using Webshop.Services.Entities.Catalog.Pricing;

namespace Webshop.Services.Entities.Catalog.Parts;

public interface IPartValidateManager: IEntityService<Part, PartSearchObject, PartSortBy, PartIncludes>
{
    void Validate(Part item);
}

public class PartValidateManager(IEntityRepository<Part, PartSearchObject, PartSortBy, PartIncludes> service)
    : EntityWrappingServiceBase<Part, PartSearchObject, PartSortBy, PartIncludes>(service), IPartValidateManager
{
    public override Task Add(Part item)
    {
        Validate(item);
        return base.Add(item);
    }
    public override Task<Part?> Modify(Part item)
    {
        Validate(item);
        return base.Modify(item);
    }
    public override Task Save(Part item)
    {
        Validate(item);
        return base.Save(item);
    }
    public void Validate(Part item)
    {
        var errors = new Dictionary<string, string>();

        // Pricing
        var priceErrors = item.Validate<Part, PartPriceHistory>();
        foreach (var error in priceErrors)
        {
            errors.Add(error.Key, error.Value);
        }

        if (errors.Any())
        {
            throw new EntityInputException<Part>("Validation failed")
            {
                InputErrors = errors,
                Item = item
            };
        }
    }
}