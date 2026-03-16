using Webshop.Admin.Entities.Claims;

namespace Webshop.Admin.Entities;

public class WebshopUserDto
{
    public string Id { get; set; } = null!;

    public string? GivenName { get; set; }
    public string? LastName { get; set; }

    public string? Email { get; set; }
    public string? UserName { get; set; }

    public ICollection<UserClaimDto>? Claims { get; set; }
}
