using Regira.Entities.Models.Abstractions;

namespace PIM.Models.Classification.Categories;

public class RelatedCategory : IEntityWithSerial
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int ParentId { get; set; }

    public Category? Child { get; set; }
    public Category? Parent { get; set; }
}
