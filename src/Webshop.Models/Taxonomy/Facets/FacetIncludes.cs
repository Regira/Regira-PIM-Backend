namespace Webshop.Models.Taxonomy.Facets;

[Flags]
public enum FacetIncludes
{
    Default = 0,
    Parents = 1 << 0,
    Children = 1 << 1,
    All = Parents | Children
}
