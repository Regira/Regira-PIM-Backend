namespace PIM.Models.Catalog.Pricing;

public class PricePeriodDto
{
    public int Id { get; set; }
    public int ObjectId { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    //public DateTime? EndDate { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}