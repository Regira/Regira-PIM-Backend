using PIM.Models.Catalog.Pricing.Abstractions;
using System.Linq.Expressions;

namespace PIM.Models.Catalog.Pricing.Utilities;

public static class PricePeriodUtility
{
    /// <summary>
    /// A price is active if StartDate is null or StartDate &lt;= priceDate.
    /// </summary>
    /// <typeparam name="TPrice"></typeparam>
    /// <param name="priceDate"></param>
    /// <returns></returns>
    public static Expression<Func<TPrice, bool>> IsActiveOn<TPrice>(DateTime priceDate)
        where TPrice : IPricePeriod
        => p => p.StartDate == null || p.StartDate <= priceDate;
}
