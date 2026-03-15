using System.Linq.Expressions;
using Webshop.Models.Entities.Catalog.Pricing.Abstractions;

namespace Webshop.Models.Entities.Catalog.Pricing.Utilities;

public static class PriceHistoryUtility
{
    /// <summary>
    /// A price is active if the priceDate is within the StartDate and EndDate (inclusive).
    /// </summary>
    /// <typeparam name="TPrice"></typeparam>
    /// <param name="priceDate"></param>
    /// <returns></returns>
    public static Expression<Func<TPrice, bool>> IsActiveOn<TPrice>(DateTime priceDate)
        where TPrice : IPriceHistory
        => p => (p.StartDate == null || p.StartDate <= priceDate)
                && (p.EndDate == null || p.EndDate >= priceDate);
}
