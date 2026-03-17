using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Webshop.Identity.Models;

public class WebshopIdentityUser : IdentityUser
{
    [PersonalData]
    [MaxLength(64)]
    public string? GivenName { get; set; }
    [PersonalData]
    [MaxLength(64)]
    public string? LastName { get; set; }

    public ICollection<IdentityUserClaim<string>>? UserClaims { get; set; }
}