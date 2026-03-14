using Regira.Entities.Models;

namespace Webshop.Models.Entities.Clients.Customers;

public class CustomerSearchObject : SearchObject
{
    public ICollection<int>? OrganizationId { get; set; }
}
