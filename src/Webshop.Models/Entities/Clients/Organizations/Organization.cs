using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Webshop.Models.Entities.Clients.Customers;

namespace Webshop.Models.Entities.Clients.Organizations;

public class Organization : IEntityWithSerial, IHasTimestamps, IHasTitle
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(256)] public string? Address { get; set; }
    [MaxLength(64)] public string? Phone { get; set; }
    [MaxLength(256)] public string? Email { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<CustomerOrganization>? Customers { get; set; }
}
