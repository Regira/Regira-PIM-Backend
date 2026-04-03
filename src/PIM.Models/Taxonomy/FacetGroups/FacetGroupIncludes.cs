namespace PIM.Models.Taxonomy.FacetGroups;

[Flags]
public enum FacetGroupIncludes
{
    Default = 0,
    ParentFacets = 1 << 0,
    ChildFacets = 1 << 1,
    Facets = ParentFacets | ChildFacets,
    All = Facets
}
