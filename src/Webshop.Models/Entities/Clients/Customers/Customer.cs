using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using System.ComponentModel.DataAnnotations;
using Webshop.Models.Entities.Clients.Addresses;
using Webshop.Models.Entities.Clients.ContactData;

namespace Webshop.Models.Entities.Clients.Customers;

public class Customer : IEntityWithSerial, IHasNormalizedTitle, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(256)] 
    public string GivenName { get; set; } = null!;
    [Required, MaxLength(256)] 
    public string? FamilyName { get; set; }
    public string? Title => $"{GivenName} {FamilyName}".TrimEnd();
    [MaxLength(512), Normalized(SourceProperties = [nameof(GivenName), nameof(FamilyName)])]
    public string? NormalizedTitle { get; set; }
    [MaxLength(1024)]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }


    public ICollection<CustomerAddress>? Addresses { get; set; }
    public ICollection<CustomerContactData>? ContactData { get; set; }
    public ICollection<CustomerOrganization>? Organizations { get; set; }
}
