using Regira.Entities.Models;

namespace Webshop.Identity.Entities;

public class WebshopUserSearchObject : SearchObject<string>
{
    public string? Username { get; set; }
}
