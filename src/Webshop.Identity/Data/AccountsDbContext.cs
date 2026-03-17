using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Webshop.Identity.Models;

namespace Webshop.Identity.Data;

public class AccountsDbContext(DbContextOptions<AccountsDbContext> options) : IdentityDbContext<WebshopIdentityUser, IdentityRole, string>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);


        builder.Entity<WebshopIdentityUser>(entity =>
        {
            entity.Property(e => e.Id).HasMaxLength(64);
            entity.Property(e => e.UserName).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.PhoneNumber).HasMaxLength(32);
            entity.Property(e => e.PasswordHash).HasMaxLength(2048);
            entity.Property(e => e.SecurityStamp).HasMaxLength(64);
            entity.Property(e => e.ConcurrencyStamp).HasMaxLength(64);

            entity.HasMany(e => e.UserClaims)
                .WithOne()
                .HasForeignKey(e => e.UserId);
        });
        builder.Entity<IdentityUserClaim<int>>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.ClaimType).HasMaxLength(64);
            entity.Property(e => e.ClaimValue).HasMaxLength(256);
        });

        builder.Entity<IdentityRole>(entity =>
        {
            entity.Property(e => e.Id).HasMaxLength(64);
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        builder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.Property(e => e.UserId).HasMaxLength(64);
            entity.Property(e => e.LoginProvider).HasMaxLength(256);
            entity.Property(e => e.ProviderKey).HasMaxLength(256);
            entity.Property(e => e.ProviderDisplayName).HasMaxLength(256);
        });

        builder.Entity<IdentityUserRole<string>>(entity =>
        {
            entity.Property(e => e.UserId).HasMaxLength(64);
            entity.Property(e => e.RoleId).HasMaxLength(64);
        });

        builder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.Property(e => e.UserId).HasMaxLength(64);
            entity.Property(e => e.LoginProvider).HasMaxLength(256);
            entity.Property(e => e.Name).HasMaxLength(256);
        });
    }
}