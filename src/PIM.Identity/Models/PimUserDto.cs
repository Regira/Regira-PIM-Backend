using PIM.Identity.Models.Claims;

namespace PIM.Identity.Models;

public class PimUserDto
{
    public string Id { get; set; } = null!;

    public string? GivenName { get; set; }
    public string? LastName { get; set; }

    public string? Email { get; set; }
    public string? UserName { get; set; }

    public ICollection<UserClaimDto>? Claims { get; set; }
}
