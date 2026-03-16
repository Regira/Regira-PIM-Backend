using Regira.Entities.Models.Abstractions;
using Webshop.Models.Entities.Classification.Categories;

namespace Webshop.Models.Entities.Classification.Parameters;

public class ParameterCategory : IEntityWithSerial
{
    public int Id { get; set; }
    public int ParameterId { get; set; }
    public int CategoryId { get; set; }

    public Parameter Parameter { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
public class CategoryParameter : IEntityWithSerial
{
    public int Id { get; set; }
    public int ParameterId { get; set; }
    public int CategoryId { get; set; }

    public Parameter Parameter { get; set; } = null!;
    public Category Category { get; set; } = null!;
}