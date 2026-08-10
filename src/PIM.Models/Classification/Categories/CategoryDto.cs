namespace PIM.Models.Classification.Categories;

public class CategoryCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    // IArchivable: DELETE soft-deletes, so the flag must round-trip or the row cannot be restored.
    public bool IsArchived { get; set; }
}

public class CategoryDto : CategoryCoreDto
{
    public ICollection<ParentCategoryDto>? ParentEntities { get; set; }
    public ICollection<ChildCategoryDto>? ChildEntities { get; set; }
    public int? ProductCount { get; set; }
}
