using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models.Taxonomy.FacetGroupFacets;
using PIM.Models.Taxonomy.Facets;

namespace PIM.Services.Entities.Taxonomy;

/// <summary>
/// The stored edges of the facet hierarchy, as the facet tree functions walk them: a facet is the parent of the facets
/// its <see cref="FacetLink"/>s name and of the groups its <see cref="FacetChildGroup"/>s name; a group is the parent of
/// the facets its <see cref="FacetParentGroup"/>s name.
/// </summary>
public static class FacetHierarchy
{
    public static async Task<IEnumerable<(FacetNode Parent, FacetNode Child)>> ChildrenOf(PimDbContext dbContext, IReadOnlyCollection<FacetNode> nodes, CancellationToken token)
    {
        var facetIds = nodes.Where(n => !n.IsGroup).Select(n => n.Id).ToList();
        var groupIds = nodes.Where(n => n.IsGroup).Select(n => n.Id).ToList();
        var edges = new List<(FacetNode, FacetNode)>();
        if (facetIds.Count > 0)
        {
            var facetLinks = await dbContext.Set<FacetLink>()
                .Where(l => facetIds.Contains(l.ParentId))
                .Select(l => new { l.ParentId, l.ChildId })
                .ToListAsync(token);
            edges.AddRange(facetLinks.Select(l => (FacetNode.Facet(l.ParentId), FacetNode.Facet(l.ChildId))));
            var childGroups = await dbContext.Set<FacetChildGroup>()
                .Where(g => facetIds.Contains(g.FacetId))
                .Select(g => new { g.FacetId, g.FacetGroupId })
                .ToListAsync(token);
            edges.AddRange(childGroups.Select(g => (FacetNode.Facet(g.FacetId), FacetNode.Group(g.FacetGroupId))));
        }
        if (groupIds.Count > 0)
        {
            var childFacets = await dbContext.Set<FacetParentGroup>()
                .Where(g => groupIds.Contains(g.FacetGroupId))
                .Select(g => new { g.FacetGroupId, g.FacetId })
                .ToListAsync(token);
            edges.AddRange(childFacets.Select(g => (FacetNode.Group(g.FacetGroupId), FacetNode.Facet(g.FacetId))));
        }
        return edges;
    }

    public static Task<string?> TitleOf(PimDbContext dbContext, FacetNode node, CancellationToken token)
        => node.IsGroup
            ? dbContext.FacetGroups.IgnoreQueryFilters().Where(g => g.Id == node.Id).Select(g => g.Title).FirstOrDefaultAsync(token)
            : dbContext.Facets.IgnoreQueryFilters().Where(f => f.Id == node.Id).Select(f => (string?)f.Title).FirstOrDefaultAsync(token);
}
