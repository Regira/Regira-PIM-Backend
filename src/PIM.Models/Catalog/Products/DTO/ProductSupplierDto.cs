using PIM.Models.Stakeholders.Parties;

namespace PIM.Models.Catalog.Products.DTO;

public class ProductSupplierDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int SupplierId { get; set; }
    public PartyDto? Supplier { get; set; }
}