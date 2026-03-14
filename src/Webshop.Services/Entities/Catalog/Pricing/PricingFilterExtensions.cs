using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Webshop.Models.Entities.Catalog.Pricing.Abstractions;

namespace Webshop.Services.Entities.Catalog.Pricing;

public static class PricingFilterExtensions
{
    /// <summary>
    /// A price is active if the priceDate is within the StartDate and EndDate (inclusive).
    /// </summary>
    /// <typeparam name="TPrice"></typeparam>
    /// <param name="priceDate"></param>
    /// <returns></returns>
    static Expression<Func<TPrice, bool>> IsActiveOn<TPrice>(DateTime priceDate)
        where TPrice : IPriceHistory
        => p => (p.StartDate == null || p.StartDate <= priceDate)
                && (p.EndDate == null || p.EndDate >= priceDate);
    /// <summary>
    /// When multiple active prices exist, the one with the latest EndDate (or StartDate if EndDate is null) should be selected.
    /// </summary>
    /// <typeparam name="TPrice"></typeparam>
    /// <param name="prices"></param>
    /// <returns></returns>
    static IQueryable<TPrice> OrderActivePrices<TPrice>(this IQueryable<TPrice> prices)
        where TPrice : IPriceHistory
        => prices
            .OrderByDescending(ph => (ph.EndDate ?? ph.StartDate) == null) // nulls first
            .ThenByDescending(ph => ph.EndDate ?? ph.StartDate);
    /// <inheritdoc cref="OrderActivePrices{TPrice}(IQueryable{TPrice})"/>
    static IEnumerable<TPrice> OrderActivePrices<TPrice>(this IEnumerable<TPrice> prices)
        where TPrice : IPriceHistory
        => prices
            .AsQueryable()
            .OrderActivePrices();

    /// <summary>
    /// Finds the active price for the given priceDate. If multiple active prices exist, the one with the latest EndDate (or StartDate if EndDate is null) will be returned.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="priceDate"></param>
    /// <returns></returns>
    public static IPriceHistory? FindActivePrice(this IHasPriceHistory item, DateTime priceDate)
        => item.Prices?
            .FindActivePrices(priceDate)
            .OrderActivePrices()
            .FirstOrDefault();
    /// <summary>
    /// Filters the prices of all items in the query to only include active prices for the given priceDate, and orders them by recency (latest EndDate or StartDate). The resulting query will be a flat list of active prices across all items.
    /// </summary>
    /// <typeparam name="THasPrices"></typeparam>
    /// <typeparam name="TPrice"></typeparam>
    /// <param name="query"></param>
    /// <param name="priceDate"></param>
    /// <returns></returns>
    public static IQueryable<TPrice> FilterActivePrices<THasPrices, TPrice>(this IQueryable<THasPrices> query, DateTime priceDate)
        where TPrice : IPriceHistory
        where THasPrices : IHasPriceHistory<TPrice>
        => query
            .SelectMany(pp => pp.Prices!)
            .FilterActivePrices(priceDate)
            .OrderActivePrices();
    /// <summary>
    /// Finds the active price for each item in the query for the given priceDate, and returns a dictionary of item ID to active price. If multiple active prices exist for an item, the one with the latest EndDate (or StartDate if EndDate is null) will be selected.
    /// </summary>
    /// <typeparam name="THasPrices"></typeparam>
    /// <typeparam name="TPrice"></typeparam>
    /// <param name="query"></param>
    /// <param name="priceDate"></param>
    /// <returns></returns>
    public static Task<Dictionary<int, decimal>> GetActivePrices<THasPrices, TPrice>(this IQueryable<THasPrices> query, DateTime priceDate)
        where TPrice : IPriceHistory
        where THasPrices : IHasPriceHistory<TPrice>
        => query
            .FilterActivePrices<THasPrices, TPrice>(priceDate)
            .GroupBy(ph => ph.ObjectId)
            .ToDictionaryAsync(x => x.Key, x => x.OrderActivePrices().First().Price);

    /// <summary>
    /// Filters the given prices to only include those active for the given priceDate. The resulting enumerable will not be ordered, so if multiple active prices exist, no specific one will be prioritized.
    /// </summary>
    /// <typeparam name="TPrice"></typeparam>
    /// <param name="prices"></param>
    /// <param name="priceDate"></param>
    /// <returns></returns>
    public static IEnumerable<TPrice> FindActivePrices<TPrice>(this IEnumerable<TPrice> prices, DateTime priceDate)
        where TPrice : IPriceHistory
        => prices.Where(IsActiveOn<TPrice>(priceDate).Compile());
    /// <summary>
    /// Filters the given prices to only include those active for the given priceDate. The resulting query will not be ordered, so if multiple active prices exist, no specific one will be prioritized.
    /// </summary>
    /// <typeparam name="TPrice"></typeparam>
    /// <param name="prices"></param>
    /// <param name="priceDate"></param>
    /// <returns></returns>
    public static IQueryable<TPrice> FilterActivePrices<TPrice>(this IQueryable<TPrice> prices, DateTime priceDate)
        where TPrice : IPriceHistory
        => prices.Where(IsActiveOn<TPrice>(priceDate));
}