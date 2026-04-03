using System.ComponentModel.DataAnnotations;

namespace PIM.Identity.Models.Claims;

public class UserClaimInputDto
{
    public int Id { get; set; }
    [MaxLength(256)]
    public string ClaimType { get; set; } = null!;
    [MaxLength(512)]
    public string? ClaimValue { get; set; }
}