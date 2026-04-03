using Regira.Entities.Models.Abstractions;

namespace PIM.Models.Catalog.Pricing.Abstractions;

public interface IPricePeriod : IEntityWithSerial, IHasObjectId<int>, IHasStartEndDate
{
    public decimal Price { get; set; }
}