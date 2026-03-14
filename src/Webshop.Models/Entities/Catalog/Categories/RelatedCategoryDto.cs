namespace Webshop.Models.Entities.Catalog.Categories;

public class RelatedCategoryDto
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int ParentId { get; set; }
}

public class ParentCategoryDto : RelatedCategoryDto
{
    public CategoryCoreDto Parent { get; set; } = null!;
}

public class ChildCategoryDto : RelatedCategoryDto
{
    public CategoryCoreDto Child { get; set; } = null!;
}
