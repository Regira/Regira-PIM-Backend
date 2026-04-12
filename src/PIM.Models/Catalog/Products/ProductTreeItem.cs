namespace PIM.Models.Catalog.Products;

public class ProductTreeItem
{
    public int ParentId { get; set; }
    public int ChildId { get; set; }
    public int Level { get; set; }
    public int RootId { get; set; }
}