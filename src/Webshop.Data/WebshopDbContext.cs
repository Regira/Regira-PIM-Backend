using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;
using Webshop.Models.Entities.Catalog.Allergens;
using Webshop.Models.Entities.Catalog.Categories;
using Webshop.Models.Entities.Catalog.Parts;
using Webshop.Models.Entities.Catalog.Products;
using Webshop.Models.Entities.Catalog.UnitTypes;
using Webshop.Models.Entities.Orders;
using Webshop.Models.Entities.Stakeholders.Parties;
using Webshop.Models.Entities.Stakeholders.Parties.Relations;

namespace Webshop.Data;

public partial class WebshopDbContext(DbContextOptions<WebshopDbContext> options) : DbContext(options)
{
    public DbSet<Party> Parties { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Person> Persons { get; set; }
    public DbSet<RelationshipType> RelationshipTypes { get; set; }
    public DbSet<Allergen> Allergens { get; set; } = null!;
    public DbSet<UnitType> UnitTypes { get; set; } = null!;
    public DbSet<Part> Parts { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention();

        // Catalog
        modelBuilder.Entity<RelatedCategory>(e =>
        {
            e.HasOne(c => c.Parent).WithMany(x => x.ChildEntities).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Child).WithMany(x => x.ParentEntities).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasMany(e => e.Prices).WithOne(ph => ph.Product).HasForeignKey(ph => ph.ObjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.UnitType).WithMany().OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasMany(e => e.Prices).WithOne(ph => ph.Part).HasForeignKey(ph => ph.ObjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.UnitType).WithMany().OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ProductCategory>(e =>
        {
            e.HasIndex(pc => new { pc.ProductId, pc.CategoryId }).IsUnique();
            e.HasOne(pc => pc.Product).WithMany(p => p.Categories).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(pc => pc.Category).WithMany(c => c.Products).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ProductPart>(e =>
        {
            e.HasIndex(pp => new { pp.ProductId, pp.PartId }).IsUnique();
            e.HasOne(pp => pp.Product).WithMany(p => p.Parts).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(pp => pp.Part).WithMany().OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ProductAllergen>(e =>
        {
            e.HasIndex(pa => new { pa.ProductId, pa.AllergenId }).IsUnique();
            e.HasOne(pa => pa.Product).WithMany(p => p.Allergens).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(pa => pa.Allergen).WithMany().OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ProductAllowedPartAddition>(e =>
        {
            e.HasIndex(pa => new { pa.ProductId, pa.PartId }).IsUnique();
            e.HasOne(pa => pa.Product).WithMany(p => p.AllowedPartAdditions).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(pa => pa.Part).WithMany().OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Party>(entity =>
        {
            entity.HasDiscriminator(p => p.PartyType)
                .HasValue<Person>("PERSON")
                .HasValue<Organization>("ORGANIZATION");
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

        modelBuilder.Entity<Order>(e =>
        {
            e.HasIndex(o => o.Code).IsUnique();
            e.HasOne(o => o.Customer).WithMany().OnDelete(DeleteBehavior.Restrict);
            e.HasMany(o => o.OrderLines).WithOne(ol => ol.Order).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderLine>(e =>
        {
            e.HasOne(ol => ol.Product).WithMany().OnDelete(DeleteBehavior.Restrict);
            e.HasMany(ol => ol.PartAdditions).WithOne().OnDelete(DeleteBehavior.Cascade);
            e.HasMany(ol => ol.PartOmissions).WithOne().OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderLinePartAddition>(entity =>
        {
            entity.HasOne(olp => olp.Part).WithMany().OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(olp => olp.OrderLine).WithMany(ol => ol.PartAdditions).HasForeignKey(olp => olp.OrderLineId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<OrderLinePartOmission>(entity =>
        {
            entity.HasOne(olp => olp.Part).WithMany().OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(olp => olp.OrderLine).WithMany(ol => ol.PartOmissions).HasForeignKey(olp => olp.OrderLineId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
