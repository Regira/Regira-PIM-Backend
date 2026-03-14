using Webshop.Models.Entities.Clients.Addresses;
using Webshop.Models.Entities.Clients.ContactData;
using Webshop.Models.Entities.Clients.Organizations;

namespace Webshop.Models.Entities.Clients.Customers;

public class CustomerDto
{
    public int Id { get; set; }
    public string GivenName { get; set; } = null!;
    public string? FamilyName { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<CustomerOrganizationDto>? Organizations { get; set; }
    public ICollection<CustomerContactDataDto>? ContactData { get; set; }
    public ICollection<AddressDto>? Addresses { get; set; }
}

public class CustomerOrganizationDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int OrganizationId { get; set; }
    public OrganizationDto? Organization { get; set; }
}
