using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;
using Webshop.Models.Entities.Catalog.Allergens;
using Webshop.Models.Entities.Catalog.Categories;
using Webshop.Models.Entities.Catalog.Parts;
using Webshop.Models.Entities.Catalog.Products;
using Webshop.Models.Entities.Catalog.UnitTypes;
using Webshop.Models.Entities.Clients.Customers;
using Webshop.Models.Entities.Clients.Organizations;
using Webshop.Models.Entities.Orders;

namespace Webshop.Data;

public partial class WebshopDbContext(DbContextOptions<WebshopDbContext> options) : DbContext(options)
{
    public DbSet<Allergen> Allergens { get; set; } = null!;
    public DbSet<UnitType> UnitTypes { get; set; } = null!;
    public DbSet<Part> Parts { get; set; } = null!;
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
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
            e.HasKey(pc => pc.Id);
            e.HasIndex(pc => new { pc.ProductId, pc.CategoryId }).IsUnique();
            e.HasOne(pc => pc.Product).WithMany(p => p.Categories).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(pc => pc.Category).WithMany(c => c.Products).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ProductPart>(e =>
        {
            e.HasKey(pp => pp.Id);
            e.HasIndex(pp => new { pp.ProductId, pp.PartId }).IsUnique();
            e.HasOne(pp => pp.Product).WithMany(p => p.Parts).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(pp => pp.Part).WithMany().OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ProductAllergen>(e =>
        {
            e.HasKey(pa => pa.Id);
            e.HasIndex(pa => new { pa.ProductId, pa.AllergenId }).IsUnique();
            e.HasOne(pa => pa.Product).WithMany(p => p.Allergens).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(pa => pa.Allergen).WithMany().OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ProductAllowedPartAddition>(e =>
        {
            e.HasKey(pa => pa.Id);
            e.HasIndex(pa => new { pa.ProductId, pa.PartId }).IsUnique();
            e.HasOne(pa => pa.Product).WithMany(p => p.AllowedPartAdditions).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(pa => pa.Part).WithMany().OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasMany(e => e.ContactData).WithOne().OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Addresses).WithOne().OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerOrganization>(e =>
        {
            e.HasKey(co => co.Id);
            e.HasIndex(co => new { co.CustomerId, co.OrganizationId }).IsUnique();
            e.HasOne(co => co.Customer).WithMany(c => c.Organizations).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(co => co.Organization).WithMany(o => o.Customers).OnDelete(DeleteBehavior.Restrict);
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
