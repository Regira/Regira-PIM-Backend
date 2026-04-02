using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using System.ComponentModel.DataAnnotations;
using Webshop.Models.Taxonomy.FacetGroups;

namespace Webshop.Models.Taxonomy.Facets;

public class Facet : IEntityWithSerial, IHasCode, IHasTimestamps, IHasNormalizedTitle, IHasDescription, IHasNormalizedContent, IArchivable
{
    public int Id { get; set; }
    [MaxLength(32)]
    public string? Code { get; set; }
    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;
    [MaxLength(128), Normalized(SourceProperties = [nameof(Title), nameof(Code)])]
    public string? NormalizedTitle { get; set; }
    [MaxLength(1024)]
    public string? Description { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Code), nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsArchived { get; set; }

    public ICollection<FacetLink>? ParentEntities { get; set; }
    public ICollection<FacetLink>? ChildEntities { get; set; }

    public ICollection<FacetParentGroup>? FacetParentGroups { get; set; }
    public ICollection<FacetChildGroup>? FacetChildGroups { get; set; }
}
