using Regira.Entities.Models;

namespace Webshop.Admin.Entities;

public class WebshopUserSearchObject : SearchObject<string>
{
    public string? Username { get; set; }
}
