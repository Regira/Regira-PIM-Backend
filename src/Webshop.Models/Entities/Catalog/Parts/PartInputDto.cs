using System.ComponentModel.DataAnnotations;
using Webshop.Models.Entities.Catalog.Pricing;

namespace Webshop.Models.Entities.Catalog.Parts;

public class PartInputDto
{
    public int Id { get; set; }
    [MaxLength(32)]
    public string? Code { get; set; }
    [Required, MaxLength(64)] 
    public string Title { get; set; } = null!;
    [MaxLength(512)] 
    public string? Description { get; set; }
    public int? UnitTypeId { get; set; }

    public ICollection<PriceHistoryInputDto>? Prices { get; set; }
}
