namespace Webshop.Identity.Models;

public class UserClaimDto
{
    public int Id { get; set; }
    public string ClaimType { get; set; } = null!;
    public string? ClaimValue { get; set; }
}