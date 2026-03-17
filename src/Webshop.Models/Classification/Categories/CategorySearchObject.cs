using Regira.Entities.Models;

namespace Webshop.Models.Classification.Categories;

public class CategorySearchObject : SearchObject
{
    public ICollection<int>? ParentId { get; set; }
    public ICollection<int>? ChildId { get; set; }
    public bool? IsRoot { get; set; }
}
