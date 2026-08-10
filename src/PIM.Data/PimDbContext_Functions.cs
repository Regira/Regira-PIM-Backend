using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PIM.Models.Catalog.Products;
using PIM.Models.Stakeholders.Parties;
using PIM.Models.Taxonomy.Facets;

namespace PIM.Data;

public partial class PimDbContext
{
    // Each tree walk is backed by two SQL functions rather than one taking a nullable id list: a
    // "ByIds" form that seeks from the supplied roots, and an "All" form over every edge. See
    // ProductDbFunctions for why. The public overloads below hide the split — callers are unchanged.

    #region Mapped DbFunctions
    // Facets + Groups
    protected IQueryable<FacetTreeItem> GetFacetOffspringByIds(string? facetIds, string? groupIds, int maxLevel)
        => FromExpression(() => GetFacetOffspringByIds(facetIds, groupIds, maxLevel));
    protected IQueryable<FacetTreeItem> GetFacetOffspringAll(int maxLevel)
        => FromExpression(() => GetFacetOffspringAll(maxLevel));
    protected IQueryable<FacetTreeItem> GetFacetAncestorsByIds(string? facetIds, string? groupIds, int maxLevel)
        => FromExpression(() => GetFacetAncestorsByIds(facetIds, groupIds, maxLevel));
    protected IQueryable<FacetTreeItem> GetFacetAncestorsAll(int maxLevel)
        => FromExpression(() => GetFacetAncestorsAll(maxLevel));
    protected IQueryable<FacetTreeItem> GetFacetFamilyByIds(string? facetIds, string? groupIds, int maxLevel)
        => FromExpression(() => GetFacetFamilyByIds(facetIds, groupIds, maxLevel));
    protected IQueryable<FacetTreeItem> GetFacetFamilyAll(int maxLevel)
        => FromExpression(() => GetFacetFamilyAll(maxLevel));

    // Parties
    protected IQueryable<PartyTreeItem> GetPartyOffspringByIds(string ids, int maxLevel)
        => FromExpression(() => GetPartyOffspringByIds(ids, maxLevel));
    protected IQueryable<PartyTreeItem> GetPartyOffspringAll(int maxLevel)
        => FromExpression(() => GetPartyOffspringAll(maxLevel));
    protected IQueryable<PartyTreeItem> GetPartyAncestorsByIds(string ids, int maxLevel)
        => FromExpression(() => GetPartyAncestorsByIds(ids, maxLevel));
    protected IQueryable<PartyTreeItem> GetPartyAncestorsAll(int maxLevel)
        => FromExpression(() => GetPartyAncestorsAll(maxLevel));
    protected IQueryable<PartyTreeItem> GetPartyFamilyByIds(string ids, int maxLevel)
        => FromExpression(() => GetPartyFamilyByIds(ids, maxLevel));
    protected IQueryable<PartyTreeItem> GetPartyFamilyAll(int maxLevel)
        => FromExpression(() => GetPartyFamilyAll(maxLevel));

    // Products
    protected IQueryable<ProductTreeItem> GetProductOffspringByIds(string ids, int maxLevel)
        => FromExpression(() => GetProductOffspringByIds(ids, maxLevel));
    protected IQueryable<ProductTreeItem> GetProductOffspringAll(int maxLevel)
        => FromExpression(() => GetProductOffspringAll(maxLevel));
    protected IQueryable<ProductTreeItem> GetProductAncestorsByIds(string ids, int maxLevel)
        => FromExpression(() => GetProductAncestorsByIds(ids, maxLevel));
    protected IQueryable<ProductTreeItem> GetProductAncestorsAll(int maxLevel)
        => FromExpression(() => GetProductAncestorsAll(maxLevel));
    protected IQueryable<ProductTreeItem> GetProductFamilyByIds(string ids, int maxLevel)
        => FromExpression(() => GetProductFamilyByIds(ids, maxLevel));
    protected IQueryable<ProductTreeItem> GetProductFamilyAll(int maxLevel)
        => FromExpression(() => GetProductFamilyAll(maxLevel));
    #endregion

    #region Convenience overloads
    public IQueryable<PartyTreeItem> GetPartyOffspring(IEnumerable<int>? ids, int maxLevel = 9)
        => ToJsonArray(ids) is { } json ? GetPartyOffspringByIds(json, maxLevel) : GetPartyOffspringAll(maxLevel);
    public IQueryable<PartyTreeItem> GetPartyAncestors(IEnumerable<int>? ids, int maxLevel = 9)
        => ToJsonArray(ids) is { } json ? GetPartyAncestorsByIds(json, maxLevel) : GetPartyAncestorsAll(maxLevel);
    public IQueryable<PartyTreeItem> GetPartyFamily(IEnumerable<int>? ids, int maxLevel = 9)
        => ToJsonArray(ids) is { } json ? GetPartyFamilyByIds(json, maxLevel) : GetPartyFamilyAll(maxLevel);

    public IQueryable<ProductTreeItem> GetProductOffspring(IEnumerable<int>? ids = null, int maxLevel = 9)
        => ToJsonArray(ids) is { } json ? GetProductOffspringByIds(json, maxLevel) : GetProductOffspringAll(maxLevel);
    public IQueryable<ProductTreeItem> GetProductAncestors(IEnumerable<int>? ids, int maxLevel = 9)
        => ToJsonArray(ids) is { } json ? GetProductAncestorsByIds(json, maxLevel) : GetProductAncestorsAll(maxLevel);
    public IQueryable<ProductTreeItem> GetProductFamily(IEnumerable<int>? ids, int maxLevel = 9)
        => ToJsonArray(ids) is { } json ? GetProductFamilyByIds(json, maxLevel) : GetProductFamilyAll(maxLevel);

    public IQueryable<FacetTreeItem> GetFacetOffspring(IEnumerable<int>? facetIds, IEnumerable<int>? groupIds = null, int maxLevel = 9)
    {
        var (facets, groups) = FacetSeeds(facetIds, groupIds);
        return facets is null && groups is null
            ? GetFacetOffspringAll(maxLevel)
            : GetFacetOffspringByIds(facets, groups, maxLevel);
    }

    public IQueryable<FacetTreeItem> GetFacetAncestors(IEnumerable<int>? facetIds, IEnumerable<int>? groupIds = null, int maxLevel = 9)
    {
        var (facets, groups) = FacetSeeds(facetIds, groupIds);
        return facets is null && groups is null
            ? GetFacetAncestorsAll(maxLevel)
            : GetFacetAncestorsByIds(facets, groups, maxLevel);
    }

    public IQueryable<FacetTreeItem> GetFacetFamily(IEnumerable<int>? facetIds, IEnumerable<int>? groupIds = null, int maxLevel = 9)
    {
        var (facets, groups) = FacetSeeds(facetIds, groupIds);
        return facets is null && groups is null
            ? GetFacetFamilyAll(maxLevel)
            : GetFacetFamilyByIds(facets, groups, maxLevel);
    }
    #endregion


    partial void ConfigureFunctions(ModelBuilder modelBuilder)
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var schema = "dbo";

        void Map(string name, params Type[] parameterTypes)
            => modelBuilder
                .HasDbFunction(typeof(PimDbContext).GetMethod(name, flags, parameterTypes)!)
                .HasSchema(schema);

        // Party Tree
        modelBuilder.Entity<PartyTreeItem>(entity =>
        {
            entity.HasNoKey().ToTable((string?)null);
            entity.HasOne(x => x.Parent)
               .WithMany();
            entity.HasOne(x => x.Child)
               .WithMany();
            entity.HasOne(x => x.Root)
               .WithMany();
            entity.HasOne(x => x.RelationshipType)
                .WithMany();
        });
        Map(nameof(GetPartyOffspringByIds), typeof(string), typeof(int));
        Map(nameof(GetPartyOffspringAll), typeof(int));
        Map(nameof(GetPartyAncestorsByIds), typeof(string), typeof(int));
        Map(nameof(GetPartyAncestorsAll), typeof(int));
        Map(nameof(GetPartyFamilyByIds), typeof(string), typeof(int));
        Map(nameof(GetPartyFamilyAll), typeof(int));

        // Product Tree
        modelBuilder.Entity<ProductTreeItem>().HasNoKey().ToTable((string?)null);
        Map(nameof(GetProductOffspringByIds), typeof(string), typeof(int));
        Map(nameof(GetProductOffspringAll), typeof(int));
        Map(nameof(GetProductAncestorsByIds), typeof(string), typeof(int));
        Map(nameof(GetProductAncestorsAll), typeof(int));
        Map(nameof(GetProductFamilyByIds), typeof(string), typeof(int));
        Map(nameof(GetProductFamilyAll), typeof(int));

        // Facet Tree
        modelBuilder.Entity<FacetTreeItem>().HasNoKey().ToTable((string?)null);
        Map(nameof(GetFacetOffspringByIds), typeof(string), typeof(string), typeof(int));
        Map(nameof(GetFacetOffspringAll), typeof(int));
        Map(nameof(GetFacetAncestorsByIds), typeof(string), typeof(string), typeof(int));
        Map(nameof(GetFacetAncestorsAll), typeof(int));
        Map(nameof(GetFacetFamilyByIds), typeof(string), typeof(string), typeof(int));
        Map(nameof(GetFacetFamilyAll), typeof(int));
    }

    private static (string? FacetIds, string? GroupIds) FacetSeeds(IEnumerable<int>? facetIds, IEnumerable<int>? groupIds)
        => (ToJsonArray(facetIds), ToJsonArray(groupIds));

    private static string? ToJsonArray(IEnumerable<int>? ids)
    {
        if (ids == null) return null;
        var list = ids as IList<int> ?? ids.ToList();
        return list.Count == 0 ? null : $"[{string.Join(",", list)}]";
    }
}
