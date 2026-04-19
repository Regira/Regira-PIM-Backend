using Regira.Entities.Models.Abstractions;

namespace PIM.Models.Catalog.Products;

public class ProductUnitTypeConversion : IEntityWithSerial
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int SourceUnitTypeId { get; set; }
    /// <summary>
    /// Default 1
    /// </summary>
    public decimal SourceQuantity { get; set; }
    /// <summary>
    /// Should be UOM
    /// </summary>
    public int TargetUnitTypeId { get; set; }
    public decimal TargetQuantity { get; set; }
}