using PIM.Models.Stakeholders.Parties;
using Regira.Entities.Services.Abstractions;

namespace PIM.Services.Entities.Stakeholders.Abstractions;

public interface IPartyRepository : IPartyService, IEntityRepository<Party, PartySearchObject, PartySortBy, PartyIncludes>;
