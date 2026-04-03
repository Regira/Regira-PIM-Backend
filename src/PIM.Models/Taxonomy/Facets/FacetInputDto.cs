using System.ComponentModel.DataAnnotations;
using PIM.Models.Taxonomy.FacetGroups;

namespace PIM.Models.Taxonomy.Facets;

public class FacetInputDto
{
    public int Id { get; set; }
    [MaxLength(32)]
    public string? Code { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    public ICollection<RelatedFacetInputDto>? ParentEntities { get; set; }
    public ICollection<RelatedFacetInputDto>? ChildEntities { get; set; }
    public ICollection<FacetGroupLinkInputDto>? FacetGroups { get; set; }
}
