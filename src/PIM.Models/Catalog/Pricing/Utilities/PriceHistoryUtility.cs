using System.Linq.Expressions;
using PIM.Models.Catalog.Pricing.Abstractions;

namespace PIM.Models.Catalog.Pricing.Utilities;

public static class PricePeriodUtility
{
    /// <summary>
    /// A price is active if the priceDate is within the StartDate and EndDate (inclusive).
    /// </summary>
    /// <typeparam name="TPrice"></typeparam>
    /// <param name="priceDate"></param>
    /// <returns></returns>
    public static Expression<Func<TPrice, bool>> IsActiveOn<TPrice>(DateTime priceDate)
        where TPrice : IPricePeriod
        => p => (p.StartDate == null || p.StartDate <= priceDate)
                && (p.EndDate == null || p.EndDate >= priceDate);
}
