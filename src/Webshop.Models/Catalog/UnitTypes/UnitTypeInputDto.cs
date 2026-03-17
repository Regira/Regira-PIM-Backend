using System.ComponentModel.DataAnnotations;

namespace Webshop.Models.Catalog.UnitTypes;

public class UnitTypeInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(16)]
    public string Code { get; set; } = null!;
    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;
}
