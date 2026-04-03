using Regira.Entities.Models;

namespace PIM.Identity.Models;

public class PimUserSearchObject : SearchObject<string>
{
    public string? Username { get; set; }
}
