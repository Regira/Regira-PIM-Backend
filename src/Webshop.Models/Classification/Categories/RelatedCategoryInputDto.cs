namespace Webshop.Models.Classification.Categories;

public class RelatedCategoryInputDto
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int ParentId { get; set; }
}
