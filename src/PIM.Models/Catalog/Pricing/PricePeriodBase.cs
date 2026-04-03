using System.ComponentModel.DataAnnotations;
using PIM.Models.Catalog.Pricing.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace PIM.Models.Catalog.Pricing;

public abstract class PricePeriodBase : IPricePeriod, IHasDescription, IHasTimestamps
{
    public int Id { get; set; }
    public int ObjectId { get; set; }
    public decimal Price { get; set; }
    [MaxLength(512)]
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}