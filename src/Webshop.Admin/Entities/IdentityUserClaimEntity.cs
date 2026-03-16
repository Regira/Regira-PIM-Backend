using Regira.Entities.Models.Abstractions;

namespace Webshop.Admin.Entities;

public class IdentityUserClaimEntity : IEntity<string>
{
    public string Id { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string? ClaimType { get; set; }
    public string? ClaimValue { get; set; }
}