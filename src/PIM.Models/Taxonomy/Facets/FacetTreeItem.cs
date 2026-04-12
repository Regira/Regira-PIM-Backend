namespace PIM.Models.Taxonomy.Facets;

public class FacetTreeItem
{
    public int ParentId { get; set; }
    public string ParentType { get; set; } = null!;
    public int ChildId { get; set; }
    public string ChildType { get; set; } = null!;
    public int Level { get; set; }
    public int RootId { get; set; }
    public string RootType { get; set; } = null!;
}