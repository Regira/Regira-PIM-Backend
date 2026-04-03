using System.ComponentModel.DataAnnotations;

namespace PIM.Models.Classification.Categories;

public class CategoryInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    public ICollection<RelatedCategoryInputDto>? ParentEntities { get; set; }
    public ICollection<RelatedCategoryInputDto>? ChildEntities { get; set; }
}
