using Regira.Entities.Models;

namespace PIM.Identity.Models;

public class PimUserSearchObject : SearchObject<string>
{
    public string? UserName { get; set; }
}
