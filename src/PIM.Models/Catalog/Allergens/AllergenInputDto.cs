using System.ComponentModel.DataAnnotations;

namespace PIM.Models.Catalog.Allergens;

public class AllergenInputDto
{
    public int Id { get; set; }
    [MaxLength(32)]
    public string? Code { get; set; }
    [Required, MaxLength(64)] 
    public string Title { get; set; } = null!;
    [MaxLength(512)] 
    public string? Description { get; set; }
}
