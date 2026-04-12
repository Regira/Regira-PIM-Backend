using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models.Stakeholders.Parties;
using PIM.Services.Entities.Stakeholders.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Services.Abstractions;
using Regira.TreeList;

namespace PIM.Services.Entities.Stakeholders.Parties;

public class PartyRepository(
    PimDbContext dbContext,
    IEntityReadService<Party, int, PartySearchObject, PartySortBy, PartyIncludes> readService,
    IEntityWriteService<Party, int> writeService)
    : EntityRepository<Party, PartySearchObject, PartySortBy, PartyIncludes>(readService, writeService), IPartyRepository
{
    public async Task<TreeList<PartyTreeItem>> GetAncestors(IList<int> ids, int maxLevel = 9)
    {
        var items = await dbContext.GetPartyAncestors(ids, maxLevel).ToListAsync();
        return items.ToTreeList(x => items.FindAll(p => p.ChildId == x.ParentId));
    }
    public async Task<TreeList<PartyTreeItem>> GetOffspring(IList<int> ids, int maxLevel = 9)
    {
        var items = await dbContext.GetPartyOffspring(ids, maxLevel).ToListAsync();
        return items.ToTreeList(x => items.FindAll(p => p.ParentId == x.ChildId));
    }
    public async Task<TreeList<PartyTreeItem>> GetFamily(IList<int> ids, int maxLevel = 9)
    {
        var items = await dbContext.GetPartyFamily(ids, maxLevel).ToListAsync();
        return items.ToTreeList(x => items.FindAll(p => p.ChildId == x.ParentId));
    }
}
