namespace Webshop.Models.Entities.Catalog.Categories;

public class CategoryCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class CategoryDto : CategoryCoreDto
{
    public ICollection<ParentCategoryDto>? ParentEntities { get; set; }
    public ICollection<ChildCategoryDto>? ChildEntities { get; set; }
    public int? ProductCount { get; set; }
}
