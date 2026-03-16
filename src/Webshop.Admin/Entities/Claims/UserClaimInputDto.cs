using System.ComponentModel.DataAnnotations;

namespace Webshop.Admin.Entities.Claims;

public class UserClaimInputDto
{
    public int Id { get; set; }
    [MaxLength(256)]
    public string ClaimType { get; set; } = null!;
    [MaxLength(512)]
    public string? ClaimValue { get; set; }
}