using Microsoft.EntityFrameworkCore;
using PIM.Models.Catalog.Pricing.Abstractions;
using PIM.Services.Entities.Extensions;

namespace PIM.Services.Entities.Catalog.Pricing;

public static class PricingFilterExtensions
{
    /// <summary>
    /// When multiple active prices exist, the one with the latest EndDate (or StartDate if EndDate is null) should be selected.
    /// </summary>
    /// <typeparam name="TPrice"></typeparam>
    /// <param name="prices"></param>
    /// <returns></returns>
    static IQueryable<TPrice> OrderActivePrices<TPrice>(this IQueryable<TPrice> prices)
        where TPrice : IPricePeriod
        => prices
            .OrderByDescending(ph => (ph.EndDate ?? ph.StartDate) == null) // nulls first
            .ThenByDescending(ph => ph.EndDate ?? ph.StartDate);
    /// <inheritdoc cref="OrderActivePrices{TPrice}(IQueryable{TPrice})"/>
    static IEnumerable<TPrice> OrderActivePrices<TPrice>(this IEnumerable<TPrice> prices)
        where TPrice : IPricePeriod
        => prices
            .AsQueryable()
            .OrderActivePrices();

    /// <summary>
    /// Finds the active price for the given priceDate. If multiple active prices exist, the one with the latest EndDate (or StartDate if EndDate is null) will be returned.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="priceDate"></param>
    /// <returns></returns>
    public static IPricePeriod? FindActivePrice(this IHasPricePeriod item, DateTime priceDate)
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
        where TPrice : IPricePeriod
        where THasPrices : IHasPricePeriod<TPrice>
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
        where TPrice : IPricePeriod
        where THasPrices : IHasPricePeriod<TPrice>
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
        where TPrice : IPricePeriod
        => prices.AsQueryable().FilterIsActiveOn(priceDate);
    /// <summary>
    /// Filters the given prices to only include those active for the given priceDate. The resulting query will not be ordered, so if multiple active prices exist, no specific one will be prioritized.
    /// </summary>
    /// <typeparam name="TPrice"></typeparam>
    /// <param name="prices"></param>
    /// <param name="priceDate"></param>
    /// <returns></returns>
    public static IQueryable<TPrice> FilterActivePrices<TPrice>(this IQueryable<TPrice> prices, DateTime priceDate)
        where TPrice : IPricePeriod
        => prices.FilterIsActiveOn(priceDate);
}