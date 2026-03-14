using System.ComponentModel.DataAnnotations;
using Webshop.Models.Entities.Clients.Addresses;
using Webshop.Models.Entities.Clients.ContactData;

namespace Webshop.Models.Entities.Clients.Customers;

public class CustomerInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(256)]
    public string GivenName { get; set; } = null!;
    [Required, MaxLength(256)]
    public string? FamilyName { get; set; }
    public ICollection<CustomerOrganizationInputDto>? Organizations { get; set; }
    public ICollection<CustomerContactDataInputDto>? ContactData { get; set; }
    public ICollection<AddressInputDto>? Addresses { get; set; }
}

public class CustomerOrganizationInputDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int OrganizationId { get; set; }
}
