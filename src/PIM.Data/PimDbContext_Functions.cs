using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PIM.Models.Catalog.Products;
using PIM.Models.Stakeholders.Parties;
using PIM.Models.Taxonomy.Facets;

namespace PIM.Data;

public partial class PimDbContext
{
    #region Mapped DbFunctions
    // Facets + Groups
    protected IQueryable<FacetTreeItem> GetFacetOffspring(string? facetIds, string? groupIds, int maxLevel)
        => FromExpression(() => GetFacetOffspring(facetIds, groupIds, maxLevel));
    protected IQueryable<FacetTreeItem> GetFacetAncestors(string? facetIds, string? groupIds, int maxLevel)
        => FromExpression(() => GetFacetAncestors(facetIds, groupIds, maxLevel));
    protected IQueryable<FacetTreeItem> GetFacetFamily(string? facetIds, string? groupIds, int maxLevel)
        => FromExpression(() => GetFacetFamily(facetIds, groupIds, maxLevel));

    // Parties
    protected IQueryable<PartyTreeItem> GetPartyOffspring(string? ids, int maxLevel)
        => FromExpression(() => GetPartyOffspring(ids, maxLevel));
    protected IQueryable<PartyTreeItem> GetPartyAncestors(string? ids, int maxLevel)
        => FromExpression(() => GetPartyAncestors(ids, maxLevel));
    protected IQueryable<PartyTreeItem> GetPartyFamily(string? ids, int maxLevel)
        => FromExpression(() => GetPartyFamily(ids, maxLevel));

    // Products
    protected IQueryable<ProductTreeItem> GetProductOffspring(string? ids, int maxLevel)
        => FromExpression(() => GetProductOffspring(ids, maxLevel));
    protected IQueryable<ProductTreeItem> GetProductAncestors(string? ids, int maxLevel)
        => FromExpression(() => GetProductAncestors(ids, maxLevel));
    protected IQueryable<ProductTreeItem> GetProductFamily(string? ids, int maxLevel)
        => FromExpression(() => GetProductFamily(ids, maxLevel));
    #endregion

    #region Convenience overloads
    public IQueryable<PartyTreeItem> GetPartyOffspring(IEnumerable<int>? ids, int maxLevel = 9)
        => GetPartyOffspring(ToJsonArray(ids), maxLevel);
    public IQueryable<PartyTreeItem> GetPartyAncestors(IEnumerable<int>? ids, int maxLevel = 9)
        => GetPartyAncestors(ToJsonArray(ids), maxLevel);
    public IQueryable<PartyTreeItem> GetPartyFamily(IEnumerable<int>? ids, int maxLevel = 9)
        => GetPartyFamily(ToJsonArray(ids), maxLevel);

    public IQueryable<ProductTreeItem> GetProductOffspring(IEnumerable<int>? ids = null, int maxLevel = 9)
        => GetProductOffspring(ToJsonArray(ids), maxLevel);
    public IQueryable<ProductTreeItem> GetProductAncestors(IEnumerable<int>? ids, int maxLevel = 9)
        => GetProductAncestors(ToJsonArray(ids), maxLevel);
    public IQueryable<ProductTreeItem> GetProductFamily(IEnumerable<int>? ids, int maxLevel = 9)
        => GetProductFamily(ToJsonArray(ids), maxLevel);

    public IQueryable<FacetTreeItem> GetFacetOffspring(IEnumerable<int>? facetIds, IEnumerable<int>? groupIds = null, int maxLevel = 9)
        => GetFacetOffspring(ToJsonArray(facetIds), ToJsonArray(groupIds), maxLevel);
    public IQueryable<FacetTreeItem> GetFacetAncestors(IEnumerable<int>? facetIds, IEnumerable<int>? groupIds = null, int maxLevel = 9)
        => GetFacetAncestors(ToJsonArray(facetIds), ToJsonArray(groupIds), maxLevel);
    public IQueryable<FacetTreeItem> GetFacetFamily(IEnumerable<int>? facetIds, IEnumerable<int>? groupIds = null, int maxLevel = 9)
        => GetFacetFamily(ToJsonArray(facetIds), ToJsonArray(groupIds), maxLevel);
    #endregion


    partial void ConfigureFunctions(ModelBuilder modelBuilder)
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var schema = "dbo";

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
        modelBuilder.HasDbFunction(typeof(PimDbContext).GetMethod(nameof(GetPartyOffspring), flags, [typeof(string), typeof(int)])!)
            .HasSchema(schema);
        modelBuilder.HasDbFunction(typeof(PimDbContext).GetMethod(nameof(GetPartyAncestors), flags, [typeof(string), typeof(int)])!)
            .HasSchema(schema);
        modelBuilder.HasDbFunction(typeof(PimDbContext).GetMethod(nameof(GetPartyFamily), flags, [typeof(string), typeof(int)])!)
            .HasSchema(schema);
        // Product Tree
        modelBuilder.Entity<ProductTreeItem>().HasNoKey().ToTable((string?)null);
        modelBuilder.HasDbFunction(typeof(PimDbContext).GetMethod(nameof(GetProductOffspring), flags, [typeof(string), typeof(int)])!)
            .HasSchema(schema);
        modelBuilder.HasDbFunction(typeof(PimDbContext).GetMethod(nameof(GetProductAncestors), flags, [typeof(string), typeof(int)])!)
            .HasSchema(schema);
        modelBuilder.HasDbFunction(typeof(PimDbContext).GetMethod(nameof(GetProductFamily), flags, [typeof(string), typeof(int)])!)
            .HasSchema(schema);
        // Facet Tree
        modelBuilder.Entity<FacetTreeItem>().HasNoKey().ToTable((string?)null);
        modelBuilder.HasDbFunction(typeof(PimDbContext).GetMethod(nameof(GetFacetOffspring), flags, [typeof(string), typeof(string), typeof(int)])!)
            .HasSchema(schema);
        modelBuilder.HasDbFunction(typeof(PimDbContext).GetMethod(nameof(GetFacetAncestors), flags, [typeof(string), typeof(string), typeof(int)])!)
            .HasSchema(schema);
        modelBuilder.HasDbFunction(typeof(PimDbContext).GetMethod(nameof(GetFacetFamily), flags, [typeof(string), typeof(string), typeof(int)])!)
            .HasSchema(schema);
    }

    private static string? ToJsonArray(IEnumerable<int>? ids)
    {
        if (ids == null) return null;
        var list = ids as IList<int> ?? ids.ToList();
        return list.Count == 0 ? null : $"[{string.Join(",", list)}]";
    }
}