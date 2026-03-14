using Regira.Entities.Models.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Webshop.Models.Entities.Catalog.UnitTypes;

public class UnitType : IEntityWithSerial, IHasCode, IHasTitle, IHasTimestamps
{
    public int Id { get; set; }
    [Required, MaxLength(16)]
    public string? Code { get; set; }
    [Required, MaxLength(64)]
    public string? Title { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
