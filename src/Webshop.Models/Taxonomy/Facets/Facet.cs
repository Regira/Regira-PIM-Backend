using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using System.ComponentModel.DataAnnotations;

namespace Webshop.Models.Taxonomy.Facets;

public class Facet : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent, IArchivable
{
    public int Id { get; set; }
    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;
    [MaxLength(1024)]
    public string? Description { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsArchived { get; set; }

    public ICollection<RelatedFacet>? ParentEntities { get; set; }
    public ICollection<RelatedFacet>? ChildEntities { get; set; }
}
