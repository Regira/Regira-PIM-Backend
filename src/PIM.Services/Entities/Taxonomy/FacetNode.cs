namespace PIM.Services.Entities.Taxonomy;

/// <summary>A node of the facet hierarchy, in which facets and facet groups alternate.</summary>
public readonly record struct FacetNode(bool IsGroup, int Id)
{
    public static FacetNode Facet(int id) => new(false, id);
    public static FacetNode Group(int id) => new(true, id);
}
