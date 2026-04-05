using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using System.ComponentModel.DataAnnotations;
using PIM.Models.Taxonomy.FacetGroupFacets;

namespace PIM.Models.Taxonomy.Facets;

/// <summary>
/// Represents a facet in the taxonomy, which can be used to categorize and organize items.
/// Facets can have parent and child relationships with other facets, allowing for a hierarchical structure. 
/// They can also be associated with facet groups to further organize them within the taxonomy.
/// </summary>
public class Facet : IEntityWithSerial, IHasCode, IHasTimestamps, IHasNormalizedTitle, IHasDescription, IHasNormalizedContent, IArchivable
{
    public int Id { get; set; }
    [MaxLength(32)]
    public string? Code { get; set; }
    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;
    [MaxLength(1024)]
    public string? Description { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsArchived { get; set; }

    [MaxLength(128), Normalized(SourceProperties = [nameof(Title), nameof(Code)])]
    public string? NormalizedTitle { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Code), nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }

    public ICollection<FacetLink>? ParentEntities { get; set; }
    public ICollection<FacetLink>? ChildEntities { get; set; }

    public ICollection<FacetParentGroup>? FacetParentGroups { get; set; }
    public ICollection<FacetChildGroup>? FacetChildGroups { get; set; }
}
