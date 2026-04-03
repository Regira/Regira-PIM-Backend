using PIM.Models.Stakeholders.Parties;
using Regira.Entities.Models.Abstractions;

namespace PIM.Models.Stakeholders.Identity;

public class PartyUser : IEntityWithSerial
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public int PartyId { get; set; }

    public Party? Party { get; set; }
}