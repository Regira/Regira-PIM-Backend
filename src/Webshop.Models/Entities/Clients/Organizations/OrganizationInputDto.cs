using System.ComponentModel.DataAnnotations;

namespace Webshop.Models.Entities.Clients.Organizations;

public class OrganizationInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(256)] public string? Address { get; set; }
    [MaxLength(64)] public string? Phone { get; set; }
    [MaxLength(256)] public string? Email { get; set; }
}
