namespace Webshop.Models.Entities.Catalog.Pricing;

public class PriceHistoryDto
{
    public int Id { get; set; }
    public int ObjectId { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}