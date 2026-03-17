namespace Webshop.Models.Catalog.Articles.DTO;

public class ArticleSupplierDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int SupplierId { get; set; }
    public PartySupplierDto? Supplier { get; set; }
}