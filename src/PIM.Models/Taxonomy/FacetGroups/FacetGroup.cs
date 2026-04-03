using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace PIM.Models.Taxonomy.FacetGroups;

public class FacetGroup : IEntityWithSerial, IHasCode, IHasNormalizedTitle, IHasTimestamps, IHasNormalizedContent, IArchivable
{
    public int Id { get; set; }
    [MaxLength(32)]
    public string? Code { get; set; }
    [Required, MaxLength(64)]
    public string? Title { get; set; }
    [MaxLength(128), Normalized(SourceProperties = [nameof(Title), nameof(Code)])]
    public string? NormalizedTitle { get; set; }
    [MaxLength(1024)]
    public string? Description { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Code), nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsArchived { get; set; }

    public ICollection<FacetChildGroup>? ParentFacets { get; set; }
    public ICollection<FacetParentGroup>? ChildFacets { get; set; }
}