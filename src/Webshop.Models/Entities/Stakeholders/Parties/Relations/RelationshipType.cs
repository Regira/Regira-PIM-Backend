using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Webshop.Models.Entities.Stakeholders.Parties.Relations;

public class RelationshipType : IEntityWithSerial, IHasCode, IHasTitle, IHasDescription, IHasNormalizedContent
{
    public int Id { get; set; }
    [MaxLength(16)]
    public string? Code { get; set; }
    [Required, MaxLength(64)]
    public string? Title { get; set; }
    [MaxLength(512)]
    public string? Description { get; set; }
    [MaxLength(128), Normalized(SourceProperties = [nameof(Title), nameof(Code)])]
    public string? NormalizedContent { get; set; }
}