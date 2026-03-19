namespace Webshop.Models.Taxonomy.FacetGroups;

[Flags]
public enum FacetGroupIncludes
{
    Default = 0,
    Facets = 1 << 0,
    All = Facets
}
