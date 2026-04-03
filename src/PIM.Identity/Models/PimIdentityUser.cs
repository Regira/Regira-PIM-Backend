using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PIM.Identity.Models;

public class PimIdentityUser : IdentityUser
{
    [PersonalData]
    [MaxLength(64)]
    public string? GivenName { get; set; }
    [PersonalData]
    [MaxLength(64)]
    public string? LastName { get; set; }

    public ICollection<IdentityUserClaim<string>>? UserClaims { get; set; }
}