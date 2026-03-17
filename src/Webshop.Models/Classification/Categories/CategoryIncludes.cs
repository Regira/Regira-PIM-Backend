namespace Webshop.Models.Classification.Categories;

[Flags]
public enum CategoryIncludes
{
    Default = 0,
    Parents = 1 << 0,
    Children = 1 << 1,
    All = Parents | Children
}
