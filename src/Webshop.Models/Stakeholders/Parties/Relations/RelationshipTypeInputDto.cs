using System.ComponentModel.DataAnnotations;

namespace Webshop.Models.Stakeholders.Parties.Relations;

public class RelationshipTypeInputDto
{
    public int Id { get; set; }
    [MaxLength(16)]
    public string? Code { get; set; }
    [Required, MaxLength(64)]
    public string? Title { get; set; }
    [MaxLength(512)]
    public string? Description { get; set; }
}
