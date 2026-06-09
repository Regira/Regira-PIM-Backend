using Regira.Entities.Models;

namespace PIM.Identity.Models;

public record PimUserSearchObject : SearchObject<string>
{
    public string? UserName { get; set; }
}
