using Microsoft.AspNetCore.Identity;
using Regira.Entities.Models.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Webshop.Identity.Models;

public class WebshopUser : IdentityUser, IEntity<string>
{
    [PersonalData]
    [MaxLength(64)]
    public string? GivenName { get; set; }
    [PersonalData]
    [MaxLength(64)]
    public string? LastName { get; set; }

    [NotMapped]
    public string? CurrentPassword { get; set; }
    [NotMapped]
    public string? NewPassword { get; set; }

    public ICollection<IdentityUserClaim<string>>? UserClaims { get; set; }
}