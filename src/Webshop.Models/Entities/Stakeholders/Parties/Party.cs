using Regira.Entities.Models.Abstractions;
using System.ComponentModel.DataAnnotations;
using Webshop.Models.Entities.Stakeholders.ContactData;
using Webshop.Models.Entities.Stakeholders.Parties.Relations;

namespace Webshop.Models.Entities.Stakeholders.Parties;

public abstract class Party(string partyType) : IEntityWithSerial, IHasCode, IHasDescription, IHasTimestamps, IArchivable, IHasContactData, IHasContactData<PartyContactDetails>, IHasStartEndDate, IHasNormalizedTitle, IHasNormalizedContent
{
    public int Id { get; set; }
    [MaxLength(16)]
    public string PartyType { get; init; } = partyType;
    [MaxLength(16)]
    public string? Code { get; set; }

    // Title -> must be abstract!
    public abstract string? Title { get; }
    [MaxLength(256)]
    public abstract string? NormalizedTitle { get; set; }
    [MaxLength(2048)]
    public string? Description { get; set; }
    [MaxLength(2048)]
    public virtual string? NormalizedContent { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime? LastModified { get; set; }
    public bool IsArchived { get; set; }

    // Contact data
    public ICollection<PartyContactDetails>? ContactData { get; set; }
    ICollection<IContactDetails>? IHasContactData.ContactData
    {
        get => ContactData?.Cast<IContactDetails>().ToList();
        set => ContactData = value?.Cast<PartyContactDetails>().ToList();
    }
    // Addresses
    public ICollection<PartyAddress>? Addresses { get; set; }
    // Relationships
    public ICollection<PartyRelationship>? ChildRelationships { get; set; }
    public ICollection<PartyRelationship>? ParentRelationships { get; set; }
}