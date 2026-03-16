using Microsoft.AspNetCore.Identity;
using Regira.Entities.Models.Abstractions;
using Webshop.Identity.Models;

namespace Webshop.Identity.Entities;

/// <summary>
/// Simpler POCO implementation of <see cref="WebshopUser"/> without the <see cref="IdentityUser"/> overhead
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