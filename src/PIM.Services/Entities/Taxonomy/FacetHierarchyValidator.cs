using Microsoft.EntityFrameworkCore;
using PIM.Core.Constants;
using PIM.Data;
using PIM.Models.Taxonomy.FacetGroupFacets;
using PIM.Models.Taxonomy.Facets;
using PIM.Services.Entities.Validation;
using Regira.Entities.Validators.Abstractions;

namespace PIM.Services.Entities.Taxonomy;

/// <summary>
/// A facet's parents and children keep the hierarchy a tree: none of its new parents may already be below it, and it
/// cannot be its own parent or child.
/// </summary>
public class FacetHierarchyValidator(PimDbContext dbContext) : EntityValidatorBase<Facet>
{
    public override async Task Validate(IEntityValidatorContext<Facet> context, CancellationToken token = default)
    {
        var item = context.Item;
        // null = the save leaves those links as they are
        if (context.Operation == EntityWriteOperation.Remove
            || (item.ParentEntities == null && item.ChildEntities == null && item.FacetParentGroups == null))
        {
            return;
        }

        var storedParentFacets = await dbContext.Set<FacetLink>().Where(l => l.ChildId == item.Id).Select(l => l.ParentId).ToListAsync(token);
        var storedParentGroups = await dbContext.Set<FacetParentGroup>().Where(g => g.FacetId == item.Id).Select(g => g.FacetGroupId).ToListAsync(token);
        var storedChildFacets = await dbContext.Set<FacetLink>().Where(l => l.ParentId == item.Id).Select(l => l.ChildId).ToListAsync(token);
        var parentFacets = item.ParentEntities != null ? dbContext.Kept(item.ParentEntities).Select(l => l.ParentId).ToList() : storedParentFacets;
        var parentGroups = item.FacetParentGroups != null ? dbContext.Kept(item.FacetParentGroups).Select(g => g.FacetGroupId).ToList() : storedParentGroups;
        var childFacets = item.ChildEntities != null ? dbContext.Kept(item.ChildEntities).Select(l => l.ChildId).ToList() : storedChildFacets;
        // the links as stored: this save adds none that could close a cycle
        if (parentFacets.ToHashSet().SetEquals(storedParentFacets) && parentGroups.ToHashSet().SetEquals(storedParentGroups)
            && childFacets.ToHashSet().SetEquals(storedChildFacets))
        {
            return;
        }
        // its child groups are saved through the groups
        var childGroups = await dbContext.Set<FacetChildGroup>().Where(g => g.FacetId == item.Id).Select(g => g.FacetGroupId).ToListAsync(token);

        if (item.Id != 0 && (parentFacets.Contains(item.Id) || childFacets.Contains(item.Id)))
        {
            var key = parentFacets.Contains(item.Id) ? nameof(Facet.ParentEntities) : nameof(Facet.ChildEntities);
            context.AddError(key, ValidationMessages.FacetHierarchyCycle, new { title = item.Title });
            return;
        }

        var cycle = await HierarchyCycleFinder.Find(FacetNode.Facet(item.Id),
            childFacets.Select(FacetNode.Facet).Concat(childGroups.Select(FacetNode.Group)),
            parentFacets.Select(FacetNode.Facet).Concat(parentGroups.Select(FacetNode.Group)),
            (nodes, t) => FacetHierarchy.ChildrenOf(dbContext, nodes, t), token);
        if (cycle != null)
        {
            var parent = cycle.Value.Parent;
            var key = parent.IsGroup ? nameof(Facet.FacetParentGroups) : nameof(Facet.ParentEntities);
            context.AddError(key, ValidationMessages.FacetHierarchyCycle, new { title = await FacetHierarchy.TitleOf(dbContext, parent, token) });
        }
    }
}
