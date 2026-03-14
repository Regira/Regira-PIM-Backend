using System.ComponentModel.DataAnnotations;

namespace Webshop.Models.Entities.Clients.Addresses;

public class AddressInputDto
{
    public int Id { get; set; }
    [MaxLength(64)]
    public string? Title { get; set; }

    [MaxLength(128)]
    public string? Street { get; set; }
    [MaxLength(16)]
    public string? Number { get; set; }
    [MaxLength(16)]
    public string? Box { get; set; }
    [MaxLength(16)]
    public string? PostBox { get; set; }
    [MaxLength(16)]
    public string? PostalCode { get; set; }
    [MaxLength(128)]
    public string? City { get; set; }
    [StringLength(2)]
    public string? CountryCode { get; set; }
    [MaxLength(512)]
    public string? Description { get; set; }

}