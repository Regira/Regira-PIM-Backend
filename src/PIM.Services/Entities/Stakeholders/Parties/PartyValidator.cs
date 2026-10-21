using Microsoft.EntityFrameworkCore;
using PIM.Core.Constants;
using PIM.Data;
using PIM.Models.Stakeholders.ContactData;
using PIM.Models.Stakeholders.Parties;
using PIM.Services.Entities.Stakeholders.ContactData;
using PIM.Services.Entities.Validation;
using Regira.Entities.Validators.Abstractions;

namespace PIM.Services.Entities.Stakeholders.Parties;

/// <summary>
/// A party's contact data must be what its type says, and its relationships must keep the party hierarchy a tree:
/// no relationship to itself, and none of its new parents may already be below it. The relationships are saved with the
/// party, so their own rules — their dates and contact data — are checked here too.
/// </summary>
public class PartyValidator(PimDbContext dbContext, ContactDataChecker contactDataChecker) : EntityValidatorBase<Party>
{
    public override async Task Validate(IEntityValidatorContext<Party> context, CancellationToken token = default)
    {
        var item = context.Item;
        if (context.Operation == EntityWriteOperation.Remove)
        {
            return;
        }

        var partyContactData = await Changed(item.ContactData, dbContext.Set<PartyContactDetails>(), token);
        foreach (var (message, value) in contactDataChecker.Check(partyContactData))
        {
            context.AddError(nameof(Party.ContactData), message, new { value });
        }
        await CheckRelationships(context, nameof(Party.ParentRelationships), item.ParentRelationships, token);
        await CheckRelationships(context, nameof(Party.ChildRelationships), item.ChildRelationships, token);

        // null = the save leaves those relationships as they are
        if (item.ParentRelationships == null && item.ChildRelationships == null)
        {
            return;
        }

        var storedParents = await dbContext.Set<PartyRelationship>().Where(r => r.ChildId == item.Id).Select(r => r.ParentId).ToListAsync(token);
        var storedChildren = await dbContext.Set<PartyRelationship>().Where(r => r.ParentId == item.Id).Select(r => r.ChildId).ToListAsync(token);
        var parents = item.ParentRelationships != null ? dbContext.Kept(item.ParentRelationships).Select(r => r.ParentId).ToList() : storedParents;
        var children = item.ChildRelationships != null ? dbContext.Kept(item.ChildRelationships).Select(r => r.ChildId).ToList() : storedChildren;
        // the links as stored: this save adds none that could close a cycle
        if (parents.ToHashSet().SetEquals(storedParents) && children.ToHashSet().SetEquals(storedChildren))
        {
            return;
        }

        if (item.Id != 0 && (parents.Contains(item.Id) || children.Contains(item.Id)))
        {
            var key = parents.Contains(item.Id) ? nameof(Party.ParentRelationships) : nameof(Party.ChildRelationships);
            context.AddError(key, ValidationMessages.PartyRelationshipSelf);
            return;
        }

        var cycle = await HierarchyCycleFinder.Find(item.Id, children, parents, RelationsOf, token);
        if (cycle != null)
        {
            // Title is computed from the names, so the party itself is read
            var parent = await dbContext.Parties.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(p => p.Id == cycle.Value.Parent, token);
            context.AddError(nameof(Party.ParentRelationships), ValidationMessages.PartyRelationshipCycle, new { name = parent?.Title });
        }
    }

    async Task CheckRelationships(IEntityValidatorContext<Party> context, string key, ICollection<PartyRelationship>? relationships, CancellationToken token)
    {
        if (relationships == null)
        {
            return;
        }
        var kept = dbContext.Kept(relationships).ToList();
        if (kept.Any(StartEndDateValidator.IsEndBeforeStart))
        {
            context.AddError(key, ValidationMessages.EndBeforeStart);
        }
        var contactData = await Changed(kept.SelectMany(r => r.ContactData ?? []), dbContext.Set<PartyRelationshipContactDetails>(), token);
        foreach (var (message, value) in contactDataChecker.Check(contactData))
        {
            context.AddError(key, message, new { value });
        }
    }

    /// <summary>
    /// The contact details this save adds or changes. A stored detail sent back as it is stays as it is — one stored
    /// before these rules existed included — so it never blocks an edit of something else.
    /// </summary>
    static async Task<IList<IContactDetails>> Changed<TDetails>(IEnumerable<TDetails>? items, IQueryable<TDetails> stored, CancellationToken token)
        where TDetails : class, IContactDetails
    {
        var list = items?.ToList() ?? [];
        var ids = list.Where(c => c.Id > 0).Select(c => c.Id).ToList();
        var storedValues = ids.Count == 0
            ? []
            : await stored.Where(c => ids.Contains(c.Id)).Select(c => new { c.Id, c.Value, c.DataType }).ToDictionaryAsync(c => c.Id, token);
        return list
            .Where(c => !storedValues.TryGetValue(c.Id, out var s) || s.Value != c.Value || s.DataType != c.DataType)
            .Cast<IContactDetails>()
            .ToList();
    }

    async Task<IEnumerable<(int Parent, int Child)>> RelationsOf(IReadOnlyCollection<int> partyIds, CancellationToken token)
    {
        var edges = await dbContext.Set<PartyRelationship>()
            .Where(r => partyIds.Contains(r.ParentId))
            .Select(r => new { r.ParentId, r.ChildId })
            .ToListAsync(token);
        return edges.Select(e => (e.ParentId, e.ChildId));
    }
}
