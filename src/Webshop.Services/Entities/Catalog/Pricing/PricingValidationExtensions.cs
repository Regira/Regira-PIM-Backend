using Webshop.Models.Entities.Catalog.Pricing.Abstractions;

namespace Webshop.Services.Entities.Catalog.Pricing;

public static class PricingValidationExtensions
{
    public static IDictionary<string, string> Validate<TEntity, TPrice>(this TEntity item)
        where TPrice : class, IPricePeriod
        where TEntity : class, IHasPricePeriod<TPrice>
    {
        var priceList = item.Prices?.ToList();
        if (priceList == null || priceList.Count < 2)
            return new Dictionary<string, string>();

        var errors = new List<string>();

        if (priceList.Count(p => !p.StartDate.HasValue) > 1)
        {
            errors.Add("Prices must not have more than one entry without a start date.");
        }
        if (priceList.Count(p => !p.EndDate.HasValue) > 1)
        {
            errors.Add("Prices must not have more than one entry without an end date.");
        }

        if (priceList.Any(p1 => priceList.Any(p2 =>
                p1 != p2
                && (
                    (p1.StartDate >= p2.StartDate || !p2.StartDate.HasValue) && (p1.StartDate < p2.EndDate || !p2.EndDate.HasValue)
                    || (p1.EndDate > p2.StartDate || !p2.StartDate.HasValue) && (p1.EndDate <= p2.EndDate || !p2.EndDate.HasValue)
                )
            )))
        {
            errors.Add("Prices must not have overlapping date ranges.");
        }

        return errors.Any() ? new Dictionary<string, string> { ["Prices"] = string.Join(Environment.NewLine, errors) } : new Dictionary<string, string>();
    }
}