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

        // Facets
        modelBuilder.Entity<Facet>(entity =>
        {
            entity.HasMany(e => e.ParentEntities).WithOne(e => e.Child).HasForeignKey(e => e.ChildId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.ChildEntities).WithOne(e => e.Parent).HasForeignKey(e => e.ParentId).OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.FacetParentGroups).WithOne(e => e.Facet).HasForeignKey(e => e.FacetId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.FacetChildGroups).WithOne(e => e.Facet).HasForeignKey(e => e.FacetId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<FacetLink>(entity =>
        {
            entity.HasIndex(x => new { x.ParentId, x.ChildId }).IsUnique();
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
        });
        modelBuilder.Entity<ProductFacet>(e =>
        {
            e.HasIndex(ac => new { ac.ProductId, ac.FacetId }).IsUnique();
            e.HasOne(ac => ac.Product).WithMany(a => a.Facets).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ac => ac.Facet).WithMany().OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ProductComponent>(e =>
        {
            e.HasIndex(ac => new { ac.AssemblyId, ac.ComponentId }).IsUnique();
            e.HasOne(ac => ac.Assembly).WithMany(a => a.Components).HasForeignKey(ac => ac.AssemblyId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ac => ac.Component).WithMany(a => a.Assemblies).HasForeignKey(ac => ac.ComponentId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ProductSupplier>(e =>
        {
            e.HasIndex(s => new { s.ProductId, s.SupplierId }).IsUnique();
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

            entity.HasMany(e => e.ContactData).WithOne().HasForeignKey(c => c.PartyRelationshipId).OnDelete(DeleteBehavior.Cascade);
        });

        ConfigureFunctions(modelBuilder);
    }

    partial void ConfigureFunctions(ModelBuilder modelBuilder);
}
