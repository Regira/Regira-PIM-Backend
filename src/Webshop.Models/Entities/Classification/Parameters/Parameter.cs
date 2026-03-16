using Regira.Entities.Models.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Webshop.Models.Entities.Classification.Parameters;

public class Parameter : IEntityWithSerial, IHasCode, IHasNormalizedTitle, IHasNormalizedContent
{
    public int Id { get; set; }
    [MaxLength(16)]
    public string? Code { get; set; }
    [MaxLength(64)]
    public string? Title { get; set; }
    [MaxLength(128)]
    public string? NormalizedTitle { get; set; }
    [MaxLength(256)]
    public string? NormalizedContent { get; set; }

    public ICollection<CategoryParameter>? ParentCategories { get; set; }
    public ICollection<ParameterCategory>? ChildCategories { get; set; }
}