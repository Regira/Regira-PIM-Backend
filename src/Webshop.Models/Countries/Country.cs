using Regira.Entities.Models.Abstractions;

namespace Webshop.Models.Countries;

public class Country : /*IEntity<string>,*/ IHasCode, IHasNormalizedTitle, IHasNormalizedContent, IHasDefault<string>
{
    public string Id { get; set; } = null!;
    public string? Title { get; set; }
    public string? Code
    {
        get => Id;
        set => Id = value!;
    }
    public bool IsDefault { get; set; }

    public string? NormalizedTitle { get; set; }
    public string? NormalizedContent { get; set; }
}