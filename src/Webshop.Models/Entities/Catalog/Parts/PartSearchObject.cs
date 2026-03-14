using Regira.Entities.Models;

namespace Webshop.Models.Entities.Catalog.Parts;

public class PartSearchObject : SearchObject
{
    public string? Code { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
