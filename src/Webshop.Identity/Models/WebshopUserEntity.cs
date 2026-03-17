using Microsoft.AspNetCore.Identity;
using Regira.Entities.Models.Abstractions;

namespace Webshop.Identity.Models;

/// <summary>
/// Simpler POCO implementation of <see cref="WebshopIdentityUser"/> without the <see cref="IdentityUser"/> overhead
/// </summary>
public class WebshopUserEntity : IEntity<string>
{
    public string Id { get; set; } = null!;

    public string? Email { get; set; }
    public string? UserName { get; set; }

    public string? GivenName { get; set; }
    public string? LastName { get; set; }

    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }

    public ICollection<IdentityUserClaimEntity>? UserClaims { get; set; }
}