using System.ComponentModel.DataAnnotations;

namespace PIM.Models.Catalog.Pricing;

public class PricePeriodInputDto
{
    public int Id { get; set; }
    public int ObjectId { get; set; }
    public decimal Price { get; set; }
    [MaxLength(512)]
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}