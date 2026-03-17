using System.ComponentModel.DataAnnotations;
using Regira.Normalizing;

namespace Webshop.Models.Stakeholders.ContactData;

public class ContactDetailsInputDto
{
    public int Id { get; set; }
    [MaxLength(64)]
    public string? Title { get; set; }
    [Required]
    [MaxLength(256)]
    public string Value { get; set; } = null!;
    [Normalized(SourceProperty = nameof(Value))]
    public ContactDataTypes DataType { get; set; }
    [MaxLength(512)]
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}