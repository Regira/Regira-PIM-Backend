namespace Webshop.Models.Entities.Catalog.Pricing.Abstractions;

public interface IHasPriceHistory
{
    public ICollection<IPriceHistory>? Prices { get; set; }
}

public interface IHasPriceHistory<T>
    where T : IPriceHistory
{
    public decimal? Price { get; set; }
    public ICollection<T>? Prices { get; set; }
}