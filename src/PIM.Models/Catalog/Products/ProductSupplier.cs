using PIM.Models.Stakeholders.Parties;
using Regira.Entities.Models.Abstractions;

namespace PIM.Models.Catalog.Products;

public class ProductSupplier : IEntityWithSerial
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int SupplierId { get; set; }

    public Product? Product { get; set; }
    public Party? Supplier { get; set; }
}