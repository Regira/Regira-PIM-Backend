using System.ComponentModel.DataAnnotations;
using Webshop.Admin.Entities.Claims;

namespace Webshop.Admin.Entities;

public class WebshopUserInputDto
{
    public string? Id { get; set; }

    [MaxLength(64)]
    public string? GivenName { get; set; }
    [MaxLength(64)]
    public string? LastName { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }

    public ICollection<UserClaimInputDto>? Claims { get; set; }
}
