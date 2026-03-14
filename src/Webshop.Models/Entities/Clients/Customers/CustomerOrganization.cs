using Regira.Entities.Models.Abstractions;
using Webshop.Models.Entities.Clients.Organizations;

namespace Webshop.Models.Entities.Clients.Customers;

public class CustomerOrganization : IEntityWithSerial
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int OrganizationId { get; set; }
    
    public Customer? Customer { get; set; }
    public Organization? Organization { get; set; }
}