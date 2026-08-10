using Microsoft.EntityFrameworkCore;
using PIM.Core.Constants;
using PIM.Models.Catalog.Products;
using PIM.Models.Catalog.UnitTypes;
using PIM.Models.Stakeholders.Identity;
using PIM.Models.Stakeholders.Parties;
using PIM.Models.Taxonomy.FacetGroupFacets;
using PIM.Models.Taxonomy.FacetGroups;
using PIM.Models.Taxonomy.Facets;
using Regira.DAL.EFcore.Extensions;

namespace PIM.Data;

public partial class PimDbContext(DbContextOptions<PimDbContext> options) : DbContext(options)
{
    public DbSet<Party> Parties { get; set; }
    public DbSet<PartyUser> PartyUsers { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Person> Persons { get; set; }
    public DbSet<RelationshipType> RelationshipTypes { get; set; }
    public DbSet<Facet> Facets { get; set; } = null!;
    public DbSet<FacetGroup> FacetGroups { get; set; } = null!;
    public DbSet<UnitType> UnitTypes { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention();

        // Filtered and covering indexes are SQL Server features; on SQLite the same HasIndex calls still
        // produce plain indexes, which is why the extras below are applied conditionally rather than skipped.
        var isSqlServer = Database.IsSqlServer();

        // Facets
        modelBuilder.Entity<Facet>(entity =>
        {
            entity.HasMany(e => e.ParentEntities).WithOne(e => e.Child).HasForeignKey(e => e.ChildId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.ChildEntities).WithOne(e => e.Parent).HasForeignKey(e => e.ParentId).OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.FacetParentGroups).WithOne(e => e.Facet).HasForeignKey(e => e.FacetId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.FacetChildGroups).WithOne(e => e.Facet).HasForeignKey(e => e.FacetId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Facet>(entity =>
        {
            entity.HasIndex(e => e.Title).HasDatabaseName("IX_Facets_Title");
            entity.HasIndex(e => e.Code).HasDatabaseName("IX_Facets_Code");
        });
        modelBuilder.Entity<FacetLink>(entity =>
        {
            entity.HasIndex(x => new { x.ParentId, x.ChildId }).IsUnique();
            // The ancestor walk travels ChildId → ParentId, so the reverse index carries the parent too.
            var byChild = entity.HasIndex(x => x.ChildId);
            if (isSqlServer)
                byChild.IncludeProperties(x => x.ParentId);
        });

        // FacetGroups
        modelBuilder.Entity<FacetGroup>(entity =>
        {
            entity.HasMany(e => e.ParentFacets).WithOne(e => e.FacetGroup).HasForeignKey(e => e.FacetGroupId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.ChildFacets).WithOne(e => e.FacetGroup).HasForeignKey(e => e.FacetGroupId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<FacetParentGroup>(e =>
        {
            e.HasIndex(x => new { x.FacetGroupId, x.FacetId }).IsUnique();
        });
        modelBuilder.Entity<FacetChildGroup>(e =>
        {
            e.HasIndex(x => new { x.FacetGroupId, x.FacetId }).IsUnique();
        });

        // Products
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasOne(e => e.UnitType).WithMany().OnDelete(DeleteBehavior.Restrict);

            // Every list query sorts by Title and carries the archived filter, so without this each page
            // sorts the whole table. Filtered to the rows those queries can actually return, and covering
            // the columns a product list projects.
            var byTitle = entity.HasIndex(e => e.Title).HasDatabaseName("IX_Products_Title_NotArchived");
            var byCreated = entity.HasIndex(e => e.Created).HasDatabaseName("IX_Products_Created_NotArchived");

            if (isSqlServer)
            {
                byTitle
                    .IncludeProperties(e => new { e.Description, e.UnitTypeId, e.DefaultQuantity, e.Created })
                    .HasFilter("[IsArchived] = 0");
                byCreated.HasFilter("[IsArchived] = 0");

                // The tree functions test "is this row's product archived?" for every edge they walk.
                // Archived products are a small minority, so an index over just those is tiny and turns
                // that anti-join into a seek.
                entity.HasIndex(e => e.Id)
                    .HasDatabaseName("IX_Products_Archived")
                    .HasFilter("[IsArchived] = 1");
            }
        });
        modelBuilder.Entity<ProductFacet>(e =>
        {
            e.HasIndex(ac => new { ac.ProductId, ac.FacetId }).IsUnique();

            // Declared rather than left to the foreign-key convention, so that answering
            // "which products carry this facet?" never leaves the index.
            var byFacet = e.HasIndex(ac => ac.FacetId);
            if (isSqlServer)
                byFacet.IncludeProperties(ac => ac.ProductId);

            e.HasOne(ac => ac.Product).WithMany(a => a.Facets).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ac => ac.Facet).WithMany().OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ProductComponent>(e =>
        {
            // Both directions of the component graph are walked — offspring by AssemblyId, ancestors by
            // ComponentId — so both carry the payload columns. Without them every matched edge costs a
            // key lookup, which is what makes a deep tree query expensive.
            var byAssembly = e.HasIndex(ac => new { ac.AssemblyId, ac.ComponentId }).IsUnique();
            var byComponent = e.HasIndex(ac => ac.ComponentId);
            if (isSqlServer)
            {
                byAssembly.IncludeProperties(ac => new { ac.Quantity, ac.IsOmittable, ac.SortOrder });
                byComponent.IncludeProperties(ac => new { ac.AssemblyId, ac.Quantity, ac.IsOmittable, ac.SortOrder });
            }

            e.HasOne(ac => ac.Assembly).WithMany(a => a.Components).HasForeignKey(ac => ac.AssemblyId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ac => ac.Component).WithMany(a => a.Assemblies).HasForeignKey(ac => ac.ComponentId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ProductSupplier>(e =>
        {
            e.HasIndex(s => new { s.ProductId, s.SupplierId }).IsUnique();
            // PartyQueryFilter matches on supplier and product together (ProductIdSupplied), so the
            // supplier-side index carries the product as a key column rather than only the FK.
            e.HasIndex(s => new { s.SupplierId, s.ProductId });

            e.HasOne(s => s.Product).WithMany(a => a.Suppliers).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.Supplier).WithMany().OnDelete(DeleteBehavior.Restrict);
        });

        // Stakeholders
        modelBuilder.Entity<PartyUser>(entity =>
        {
            entity.HasOne(pu => pu.Party).WithOne().HasForeignKey<PartyUser>(pu => pu.PartyId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Party>(entity =>
        {
            entity.HasDiscriminator(p => p.PartyType)
                .HasValue<Person>(PartyTypes.Person)
                .HasValue<Organization>(PartyTypes.Organization);
            entity.HasMany(e => e.ContactData).WithOne().OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Addresses).WithOne().HasForeignKey(a => a.PartyId).OnDelete(DeleteBehavior.Cascade);

            // A party list filters on the discriminator and sorts by NormalizedTitle by default
            // (PartySortingQueryBuilder), which is one index rather than a filter plus a sort.
            var byTypeAndTitle = entity.HasIndex(e => new { e.PartyType, e.NormalizedTitle })
                .HasDatabaseName("IX_Parties_PartyType_NormalizedTitle");
            if (isSqlServer)
                byTypeAndTitle.HasFilter("[IsArchived] = 0");

            entity.HasIndex(e => e.Code).HasDatabaseName("IX_Parties_Code");
            entity.HasIndex(e => e.Created).HasDatabaseName("IX_Parties_Created");
        });

        modelBuilder.Entity<PartyRelationship>(entity =>
        {
            entity
                .HasOne(r => r.Parent)
                .WithMany(p => p.ChildRelationships)
                .HasForeignKey(r => r.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(r => r.Child)
                .WithMany(p => p.ParentRelationships)
                .HasForeignKey(r => r.ChildId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasIndex(r => new { r.ParentId, r.ChildId, r.RelationshipTypeId })
                .IsUnique();

            // Mirrors ProductComponent: the ancestor walk travels ChildId → ParentId.
            var byChild = entity.HasIndex(r => r.ChildId);
            if (isSqlServer)
                byChild.IncludeProperties(r => new { r.ParentId, r.RelationshipTypeId });

            entity.HasMany(e => e.ContactData).WithOne().HasForeignKey(c => c.PartyRelationshipId).OnDelete(DeleteBehavior.Cascade);
        });

        ConfigureFunctions(modelBuilder);
    }

    partial void ConfigureFunctions(ModelBuilder modelBuilder);
}
