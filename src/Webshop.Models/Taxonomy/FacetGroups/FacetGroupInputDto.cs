using System.ComponentModel.DataAnnotations;

namespace Webshop.Models.Taxonomy.FacetGroups;

public class FacetGroupInputDto
{
    public int Id { get; set; }
    [MaxLength(32)] public string? Code { get; set; }
    [Required, MaxLength(64)] public string? Title { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    public ICollection<FacetGroupLinkInputDto>? ParentFacets { get; set; }
    public ICollection<FacetGroupLinkInputDto>? ChildFacets { get; set; }
}
