using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PIM.Models.Stakeholders.Parties;

namespace PIM.Data;


public class ProductRelationResult
{
    public int ParentId { get; set; }
    public int ChildId { get; set; }
    public int Level { get; set; }
    public int RootId { get; set; }
}

public class FacetRelationResult
{
    public int ParentId { get; set; }
    public string ParentType { get; set; } = null!;
    public int ChildId { get; set; }
    public string ChildType { get; set; } = null!;
    public int Level { get; set; }
    public int RootId { get; set; }
    public string RootType { get; set; } = null!;
}

public partial class PimDbContext
{
    #region Mapped DbFunctions
    // Facets + Groups
    protected IQueryable<FacetRelationResult> GetFacetOffspring(string? facetIds, string? groupIds, int maxLevel)
        => FromExpression(() => GetFacetOffspring(facetIds, groupIds, maxLevel));
    protected IQueryable<FacetRelationResult> GetFacetAncestors(string? facetIds, string? groupIds, int maxLevel)
        => FromExpression(() => GetFacetAncestors(facetIds, groupIds, maxLevel));
    protected IQueryable<FacetRelationResult> GetFacetFamily(string? facetIds, string? groupIds, int maxLevel)
        => FromExpression(() => GetFacetFamily(facetIds, groupIds, maxLevel));

    // Parties
    protected IQueryable<PartyTreeItem> GetPartyOffspring(string? ids, int maxLevel)
        => FromExpression(() => GetPartyOffspring(ids, maxLevel));
    protected IQueryable<PartyTreeItem> GetPartyAncestors(string? ids, int maxLevel)
        => FromExpression(() => GetPartyAncestors(ids, maxLevel));
    protected IQueryable<PartyTreeItem> GetPartyFamily(string? ids, int maxLevel)
        => FromExpression(() => GetPartyFamily(ids, maxLevel));

    // Products
    protected IQueryable<ProductRelationResult> GetProductOffspring(string? ids, int maxLevel)
        => FromExpression(() => GetProductOffspring(ids, maxLevel));
    protected IQueryable<ProductRelationResult> GetProductAncestors(string? ids, int maxLevel)
        => FromExpression(() => GetProductAncestors(ids, maxLevel));
    protected IQueryable<ProductRelationResult> GetProductFamily(string? ids, int maxLevel)
        => FromExpression(() => GetProductFamily(ids, maxLevel));
    #endregion

    #region Convenience overloads
    public IQueryable<PartyTreeItem> GetPartyOffspring(IEnumerable<int>? ids, int maxLevel = 9)
        => GetPartyOffspring(ToJsonArray(ids), maxLevel);
    public IQueryable<PartyTreeItem> GetPartyAncestors(IEnumerable<int>? ids, int maxLevel = 9)
        => GetPartyAncestors(ToJsonArray(ids), maxLevel);
    public IQueryable<PartyTreeItem> GetPartyFamily(IEnumerable<int>? ids, int maxLevel = 9)
        => GetPartyFamily(ToJsonArray(ids), maxLevel);

    public IQueryable<ProductRelationResult> GetProductOffspring(IEnumerable<int>? ids, int maxLevel = 9)
        => GetProductOffspring(ToJsonArray(ids), maxLevel);
    public IQueryable<ProductRelationResult> GetProductAncestors(IEnumerable<int>? ids, int maxLevel = 9)
        => GetProductAncestors(ToJsonArray(ids), maxLevel);
    public IQueryable<ProductRelationResult> GetProductFamily(IEnumerable<int>? ids, int maxLevel = 9)
        => GetProductFamily(ToJsonArray(ids), maxLevel);

    public IQueryable<FacetRelationResult> GetFacetOffspring(IEnumerable<int>? facetIds, IEnumerable<int>? groupIds = null, int maxLevel = 9)
        => GetFacetOffspring(ToJsonArray(facetIds), ToJsonArray(groupIds), maxLevel);
    public IQueryable<FacetRelationResult> GetFacetAncestors(IEnumerable<int>? facetIds, IEnumerable<int>? groupIds = null, int maxLevel = 9)
        => GetFacetAncestors(ToJsonArray(facetIds), ToJsonArray(groupIds), maxLevel);
    public IQueryable<FacetRelationResult> GetFacetFamily(IEnumerable<int>? facetIds, IEnumerable<int>? groupIds = null, int maxLevel = 9)
        => GetFacetFamily(ToJsonArray(facetIds), ToJsonArray(groupIds), maxLevel);
    #endregion


    partial void ConfigureFunctions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PartyTreeItem>(entity =>
        {
            entity.HasNoKey();
            entity.HasOne(x => x.Parent)
                .WithMany();
            entity.HasOne(x => x.Child)
                .WithMany();
            entity.HasOne(x => x.Root)
                .WithMany();
        });
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        modelBuilder.HasDbFunction(typeof(PimDbContext).GetMethod(nameof(GetPartyOffspring), flags, [typeof(string), typeof(int)])!)
            .HasSchema("dbo");
        modelBuilder.HasDbFunction(typeof(PimDbContext).GetMethod(nameof(GetPartyAncestors), flags, [typeof(string), typeof(int)])!)
            .HasSchema("dbo");
        modelBuilder.HasDbFunction(typeof(PimDbContext).GetMethod(nameof(GetPartyFamily), flags, [typeof(string), typeof(int)])!)
            .HasSchema("dbo");

        modelBuilder.Entity<ProductRelationResult>().HasNoKey();
        modelBuilder.HasDbFunction(typeof(PimDbContext).GetMethod(nameof(GetProductOffspring), flags, [typeof(string), typeof(int)])!)
            .HasSchema("dbo");
        modelBuilder.HasDbFunction(typeof(PimDbContext).GetMethod(nameof(GetProductAncestors), flags, [typeof(string), typeof(int)])!)
            .HasSchema("dbo");
        modelBuilder.HasDbFunction(typeof(PimDbContext).GetMethod(nameof(GetProductFamily), flags, [typeof(string), typeof(int)])!)
            .HasSchema("dbo");

        modelBuilder.Entity<FacetRelationResult>().HasNoKey();
        modelBuilder.HasDbFunction(typeof(PimDbContext).GetMethod(nameof(GetFacetOffspring), flags, [typeof(string), typeof(string), typeof(int)])!)
            .HasSchema("dbo");
        modelBuilder.HasDbFunction(typeof(PimDbContext).GetMethod(nameof(GetFacetAncestors), flags, [typeof(string), typeof(string), typeof(int)])!)
            .HasSchema("dbo");
        modelBuilder.HasDbFunction(typeof(PimDbContext).GetMethod(nameof(GetFacetFamily), flags, [typeof(string), typeof(string), typeof(int)])!)
            .HasSchema("dbo");
    }

    private static string? ToJsonArray(IEnumerable<int>? ids)
    {
        if (ids == null) return null;
        var list = ids as IList<int> ?? ids.ToList();
        return list.Count == 0 ? null : $"[{string.Join(",", list)}]";
    }
}