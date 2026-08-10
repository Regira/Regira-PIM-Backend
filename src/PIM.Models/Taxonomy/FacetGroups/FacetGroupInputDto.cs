using System.ComponentModel.DataAnnotations;
using PIM.Models.Taxonomy.FacetGroupFacets;

namespace PIM.Models.Taxonomy.FacetGroups;

public class FacetGroupInputDto
{
    public int Id { get; set; }
    [MaxLength(32)] public string? Code { get; set; }
    [Required, MaxLength(64)] public string? Title { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    // Must stay on the input DTO: it is the only way a payload can clear the archived flag (restore).
    public bool IsArchived { get; set; }
    public ICollection<FacetGroupFacetInputDto>? ParentFacets { get; set; }
    public ICollection<FacetGroupFacetInputDto>? ChildFacets { get; set; }
}
