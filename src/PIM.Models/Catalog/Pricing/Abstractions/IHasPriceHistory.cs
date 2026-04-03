namespace PIM.Models.Catalog.Pricing.Abstractions;

public interface IHasPricePeriod
{
    public ICollection<IPricePeriod>? Prices { get; set; }
}

public interface IHasPricePeriod<T>
    where T : IPricePeriod
{
    public decimal? Price { get; set; }
    public ICollection<T>? Prices { get; set; }
}