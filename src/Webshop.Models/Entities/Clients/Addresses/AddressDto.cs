namespace Webshop.Models.Entities.Clients.Addresses;

public class AddressDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Street { get; set; }
    public string? Number { get; set; }
    public string? Box { get; set; }
    public string? PostBox { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? CountryCode { get; set; }
    public string? Description { get; set; }
}