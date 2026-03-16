using Regira.Entities.Models.Abstractions;

namespace Webshop.Admin.Entities;

public class IdentityUserClaimEntity : IEntityWithSerial
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public string? ClaimType { get; set; }
    public string? ClaimValue { get; set; }
}