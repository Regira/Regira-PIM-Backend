using Bogus;
using Microsoft.Extensions.Configuration;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Webshop.Identity.Entities;
using Webshop.Identity.Services;

namespace Webshop.DemoDataConsole.Infrastructure;

public class AccountsDataSeeder(
    IEntityService<WebshopUserEntity, string, WebshopUserSearchObject, EntitySortBy, WebshopUserIncludes> userService,
    IConfiguration configuration)
{
    public async Task SeedAsync()
    {
        await SeedAdminUsers();
        await SeedRandomUsers();
    }

    private async Task SeedAdminUsers()
    {
        var adminUsers = configuration.GetSection("Identity:AdminUsers").Get<AdminUserConfig[]>() ?? [];
        foreach (var adminConfig in adminUsers)
        {
            var user = new WebshopUserEntity
            {
                Email = adminConfig.Email,
                UserName = adminConfig.Email,
                NewPassword = adminConfig.Password,
                UserClaims =
                [
                    new IdentityUserClaimEntity { ClaimType = WebshopClaimTypes.Permission, ClaimValue = "Admin" }
                ]
            };
            await userService.Save(user);
        }
        await userService.SaveChanges();
    }

    private async Task SeedRandomUsers()
    {
        var permissions = new[] { "Customer", "Customer", "PurchasingUnit", "Supplier", "Supplier" };
        var f = new Faker("en");

        for (var i = 0; i < 5; i++)
        {
            var firstName = f.Name.FirstName();
            var lastName = f.Name.LastName();
            var email = f.Internet.Email(firstName, lastName);

            var user = new WebshopUserEntity
            {
                Email = email,
                UserName = email,
                GivenName = firstName,
                LastName = lastName,
                NewPassword = f.Internet.Password(8),
                UserClaims =
                [
                    new IdentityUserClaimEntity { ClaimType = WebshopClaimTypes.Permission, ClaimValue = permissions[i] }
                ]
            };
            await userService.Save(user);
        }
        await userService.SaveChanges();
    }
}

public class AdminUserConfig
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
