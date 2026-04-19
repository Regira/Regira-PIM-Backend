namespace PIM.Models.Catalog.UnitTypes;

public class UnitTypeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public bool IsUom { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
