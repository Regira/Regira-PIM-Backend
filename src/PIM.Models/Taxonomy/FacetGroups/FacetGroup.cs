using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using System.ComponentModel.DataAnnotations;
using PIM.Models.Taxonomy.FacetGroupFacets;

namespace PIM.Models.Taxonomy.FacetGroups;

/// <summary>
/// Represents a group of facets that can be used to categorize and filter products in a taxonomy. A facet group can have multiple child facets and can also be a child of another facet group, allowing for a hierarchical structure of facets.
/// Facets can be linked to a Product, and the facet groups can be used to organize these facets.
/// e.g. a "Color" facet group can have child facets like "Red", "Blue", "Green", etc.
/// </summary>
public class FacetGroup : IEntityWithSerial, IHasCode, IHasNormalizedTitle, IHasTimestamps, IHasNormalizedContent, IArchivable
{
    public int Id { get; set; }
    [MaxLength(32)]
    public string? Code { get; set; }
    [Required, MaxLength(64)]
    public string? Title { get; set; }
    [MaxLength(1024)]
    public string? Description { get; set; }

    public bool AllowMultiSelect { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsArchived { get; set; }

    [MaxLength(128), Normalized(SourceProperties = [nameof(Title), nameof(Code)])]
    public string? NormalizedTitle { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Code), nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }

    public ICollection<FacetChildGroup>? ParentFacets { get; set; }
    public ICollection<FacetParentGroup>? ChildFacets { get; set; }
}