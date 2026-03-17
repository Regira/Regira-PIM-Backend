using Regira.Entities.Models;

namespace Webshop.Identity.Models;

public class WebshopUserSearchObject : SearchObject<string>
{
    public string? Username { get; set; }
}
