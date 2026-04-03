using System.ComponentModel.DataAnnotations;
using PIM.Identity.Models.Claims;

namespace PIM.Identity.Models;

public class PimUserInputDto
{
    public string? Id { get; set; }

    [MaxLength(64)]
    public string? GivenName { get; set; }
    [MaxLength(64)]
    public string? LastName { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }
    [MaxLength(256)]
    public string? UserName { get; set; }

    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }

    public ICollection<UserClaimInputDto>? Claims { get; set; }
}
