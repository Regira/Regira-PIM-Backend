namespace PIM.Services.Entities.Validation;

/// <summary>
/// Finds the cycle a save would close in a hierarchy (products and their components, facets and facet groups, parties
/// and their relationships). The saved item's own edges are the ones the save replaces, so the walk starts from its new
/// children, follows the <b>stored</b> edges of every other node, and reports the first of its new parents it reaches:
/// that parent would end up below the item it is about to be the parent of.
/// </summary>
public static class HierarchyCycleFinder
{
    /// <param name="self">The saved item; never expanded, since its stored edges are about to be replaced.</param>
    /// <param name="children">The item's children after the save — the walk starts here.</param>
    /// <param name="parents">The item's parents after the save — reaching one closes a cycle.</param>
    /// <param name="childrenOf">The stored (parent, child) edges leaving the given nodes.</param>
    /// <returns>The child the walk started from and the parent it reached, or <c>null</c> when the save closes no cycle.</returns>
    public static async Task<(TNode Child, TNode Parent)?> Find<TNode>(TNode self, IEnumerable<TNode> children, IEnumerable<TNode> parents,
        Func<IReadOnlyCollection<TNode>, CancellationToken, Task<IEnumerable<(TNode Parent, TNode Child)>>> childrenOf, CancellationToken token = default)
        where TNode : notnull
    {
        var targets = parents.ToHashSet();
        // every node reached, mapped to the child of the saved item its path started from
        var origin = new Dictionary<TNode, TNode>();
        var frontier = new List<TNode>();
        foreach (var child in children)
        {
            if (origin.TryAdd(child, child))
            {
                frontier.Add(child);
            }
        }

        while (frontier.Count > 0)
        {
            foreach (var node in frontier)
            {
                if (targets.Contains(node))
                {
                    return (origin[node], node);
                }
            }

            var expandable = frontier.Where(n => !n.Equals(self)).ToList();
            var next = new List<TNode>();
            if (expandable.Count > 0)
            {
                foreach (var (parent, child) in await childrenOf(expandable, token))
                {
                    if (origin.TryAdd(child, origin[parent]))
                    {
                        next.Add(child);
                    }
                }
            }
            frontier = next;
        }

        return null;
    }
}
