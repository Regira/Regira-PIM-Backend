namespace Webshop.Models.Entities.Catalog.Categories;

public class RelatedCategoryInputDto
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int ParentId { get; set; }
}
