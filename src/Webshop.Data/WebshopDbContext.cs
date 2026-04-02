using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;
using Webshop.Core.Constants;
using Webshop.Models.Catalog.Allergens;
using Webshop.Models.Catalog.Articles;
using Webshop.Models.Catalog.UnitTypes;
using Webshop.Models.Orders;
using Webshop.Models.Stakeholders.Identity;
using Webshop.Models.Stakeholders.Parties;
using Webshop.Models.Stakeholders.Parties.Relations;
using Webshop.Models.Taxonomy.Facets;
using Webshop.Models.Taxonomy.FacetGroups;

namespace Webshop.Data;

public class WebshopDbContext(DbContextOptions<WebshopDbContext> options) : DbContext(options)
{
    public DbSet<Party> Parties { get; set; }
    public DbSet<PartyUser> PartyUsers { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Person> Persons { get; set; }
    public DbSet<RelationshipType> RelationshipTypes { get; set; }
    public DbSet<Facet> Facets { get; set; } = null!;
    public DbSet<FacetGroup> FacetGroups { get; set; } = null!;
    public DbSet<UnitType> UnitTypes { get; set; } = null!;
    public DbSet<Article> Articles { get; set; } = null!;
    public DbSet<Allergen> Allergens { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;

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

        // Articles
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasMany(e => e.Prices).WithOne(ph => ph.Article).HasForeignKey(ph => ph.ObjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.UnitType).WithMany().OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ArticleFacet>(e =>
        {
            e.HasIndex(ac => new { ac.ArticleId, ac.FacetId }).IsUnique();
            e.HasOne(ac => ac.Article).WithMany(a => a.Facets).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ac => ac.Facet).WithMany().OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ArticleComponent>(e =>
        {
            e.HasIndex(ac => new { ac.AssemblyId, ac.ComponentId }).IsUnique();
            e.HasOne(ac => ac.Assembly).WithMany(a => a.Components).HasForeignKey(ac => ac.AssemblyId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ac => ac.Component).WithMany(a => a.Assemblies).HasForeignKey(ac => ac.ComponentId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ArticleAllowedComponentAddition>(e =>
        {
            e.HasIndex(ac => new { ac.AssemblyId, ac.ComponentId }).IsUnique();
            e.HasOne(ac => ac.Assembly).WithMany(a => a.AllowedComponentAdditions).HasForeignKey(ac => ac.AssemblyId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ac => ac.Component).WithMany().HasForeignKey(ac => ac.ComponentId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ArticleSupplier>(e =>
        {
            e.HasIndex(s => new { s.ArticleId, s.SupplierId }).IsUnique();
            e.HasOne(s => s.Article).WithMany(a => a.Suppliers).OnDelete(DeleteBehavior.Cascade);
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

            entity.HasMany(e => e.ContactData).WithOne().OnDelete(DeleteBehavior.Cascade);
        });

        // Orders
        modelBuilder.Entity<Order>(e =>
        {
            e.HasIndex(o => o.Code).IsUnique();
            e.HasOne(o => o.Customer).WithMany().OnDelete(DeleteBehavior.Restrict);
            e.HasMany(o => o.OrderLines).WithOne(ol => ol.Order).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderLine>(e =>
        {
            e.HasOne(ol => ol.Article).WithMany().OnDelete(DeleteBehavior.Restrict);
            e.HasMany(ol => ol.ComponentAdditions).WithOne().OnDelete(DeleteBehavior.Cascade);
            e.HasMany(ol => ol.ComponentOmissions).WithOne().OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderLineComponentAddition>(entity =>
        {
            entity.HasOne(olp => olp.Article).WithMany().OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(olp => olp.OrderLine).WithMany(ol => ol.ComponentAdditions).HasForeignKey(olp => olp.OrderLineId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<OrderLineComponentOmission>(entity =>
        {
            entity.HasOne(olp => olp.Article).WithMany().OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(olp => olp.OrderLine).WithMany(ol => ol.ComponentOmissions).HasForeignKey(olp => olp.OrderLineId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
