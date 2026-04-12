using PIM.Models.Stakeholders.Parties;
using Regira.Entities.Services.Abstractions;
using Regira.TreeList;

namespace PIM.Services.Entities.Stakeholders.Abstractions;

public interface IPartyService : IEntityService<Party, PartySearchObject, PartySortBy, PartyIncludes>
{
    Task<TreeList<PartyTreeItem>> GetAncestors(IList<int> ids, int maxLevel = 9);
    Task<TreeList<PartyTreeItem>> GetOffspring(IList<int> ids, int maxLevel = 9);
    Task<TreeList<PartyTreeItem>> GetFamily(IList<int> ids, int maxLevel = 9);
}
