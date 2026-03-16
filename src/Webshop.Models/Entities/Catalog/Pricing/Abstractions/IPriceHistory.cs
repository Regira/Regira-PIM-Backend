using Regira.Entities.Models.Abstractions;

namespace Webshop.Models.Entities.Catalog.Pricing.Abstractions;

public interface IPricePeriod : IEntityWithSerial, IHasObjectId<int>, IHasStartEndDate
{
    public decimal Price { get; set; }
}