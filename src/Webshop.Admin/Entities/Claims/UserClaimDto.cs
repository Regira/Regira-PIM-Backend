namespace Webshop.Admin.Entities.Claims;

public class UserClaimDto
{
    public int Id { get; set; }
    public string ClaimType { get; set; } = null!;
    public string? ClaimValue { get; set; }
}