using Regira.Entities.Models;

namespace PIM.Models.Classification.Categories;

public record CategorySearchObject : SearchObject
{
    public ICollection<int>? ParentId { get; set; }
    public ICollection<int>? ChildId { get; set; }
    public bool? IsRoot { get; set; }
}
