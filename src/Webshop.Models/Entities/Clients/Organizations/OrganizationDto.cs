namespace Webshop.Models.Entities.Clients.Organizations;

public class OrganizationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
