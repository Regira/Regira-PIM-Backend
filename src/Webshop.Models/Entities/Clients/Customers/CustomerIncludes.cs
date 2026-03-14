namespace Webshop.Models.Entities.Clients.Customers;

[Flags]
public enum CustomerIncludes
{
    None = 0,
    Organizations = 1 << 0,
    ContactData = 1 << 1,
    Addresses = 1 << 2,
    All = Organizations | ContactData | Addresses
}