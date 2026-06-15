using FluentValidation;
using PIM.Models.Stakeholders.Parties;
using PIM.Services.Entities.Abstractions;
using PIM.Services.Entities.Stakeholders.Abstractions;
using Regira.TreeList;

namespace PIM.Services.Entities.Stakeholders.Parties;

public class PartyManager(IPartyRepository service, IValidator<Party>? validator)
    : PimEntityManager<Party, PartySearchObject, PartySortBy, PartyIncludes>(service, validator), IPartyService
{
    public Task<TreeList<PartyTreeItem>> GetAncestors(IList<int> ids, int maxLevel = 9)
        => service.GetAncestors(ids, maxLevel);
    public Task<TreeList<PartyTreeItem>> GetOffspring(IList<int> ids, int maxLevel = 9)
        => service.GetOffspring(ids, maxLevel);
    public Task<TreeList<PartyTreeItem>> GetFamily(IList<int> ids, int maxLevel = 9)
        => service.GetFamily(ids, maxLevel);
}