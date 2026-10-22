using Microsoft.EntityFrameworkCore;
using PIM.Core.Constants;
using PIM.Data;
using PIM.Models.Taxonomy.FacetGroupFacets;
using PIM.Models.Taxonomy.FacetGroups;
using PIM.Services.Entities.Validation;
using Regira.Entities.Validators.Abstractions;

namespace PIM.Services.Entities.Taxonomy;

/// <summary>
/// A facet group's parent and child facets keep the facet hierarchy a tree: none of its new parent facets may already
/// be below it.
/// </summary>
public class FacetGroupHierarchyValidator(PimDbContext dbContext) : EntityValidatorBase<FacetGroup>
{
    public override async Task Validate(IEntityValidatorContext<FacetGroup> context, CancellationToken token = default)
    {
        var item = context.Item;
        // null = the save leaves those links as they are
        if (context.Operation == EntityWriteOperation.Remove || (item.ParentFacets == null && item.ChildFacets == null))
        {
            return;
        }

        var storedParentFacets = await dbContext.Set<FacetChildGroup>().Where(g => g.FacetGroupId == item.Id).Select(g => g.FacetId).ToListAsync(token);
        var storedChildFacets = await dbContext.Set<FacetParentGroup>().Where(g => g.FacetGroupId == item.Id).Select(g => g.FacetId).ToListAsync(token);
        var parentFacets = item.ParentFacets != null ? dbContext.Kept(item.ParentFacets).Select(g => g.FacetId).ToList() : storedParentFacets;
        var childFacets = item.ChildFacets != null ? dbContext.Kept(item.ChildFacets).Select(g => g.FacetId).ToList() : storedChildFacets;
        // the links as stored: this save adds none that could close a cycle
        if (parentFacets.ToHashSet().SetEquals(storedParentFacets) && childFacets.ToHashSet().SetEquals(storedChildFacets))
        {
            return;
        }

        var cycle = await HierarchyCycleFinder.Find(FacetNode.Group(item.Id),
            childFacets.Select(FacetNode.Facet),
            parentFacets.Select(FacetNode.Facet),
            (nodes, t) => FacetHierarchy.ChildrenOf(dbContext, nodes, t), token);
        if (cycle != null)
        {
            context.AddError(nameof(FacetGroup.ParentFacets), ValidationMessages.FacetHierarchyCycle,
                new { title = await FacetHierarchy.TitleOf(dbContext, cycle.Value.Parent, token) });
        }
    }
}
