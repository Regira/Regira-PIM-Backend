using Bogus;
using Microsoft.Extensions.Configuration;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Webshop.Core.Constants;
using Webshop.Identity.Models;

namespace Webshop.AccountDataGenerator.Infrastructure;

public class AccountsDataSeeder(
    IEntityService<WebshopUserEntity, string, WebshopUserSearchObject, EntitySortBy, WebshopUserIncludes> userService,
    IConfiguration configuration)
{
    public class AdminUserConfig
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

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
                    new IdentityUserClaimEntity { ClaimType = WebshopClaimTypes.Permission, ClaimValue = WebshopPermissionValues.Admin },
                    new IdentityUserClaimEntity { ClaimType = WebshopClaimTypes.Permission, ClaimValue = WebshopPermissionValues.Editor }
                ]
            };
            await userService.Save(user);
        }
        await userService.SaveChanges();
    }

    private async Task SeedRandomUsers()
    {
        var permissions = Enumerable.Range(0, 5).Select(_ => WebshopPermissionValues.Supplier)
            .Concat(Enumerable.Range(0, 10).Select(_ => WebshopPermissionValues.Customer))
            .ToArray();

        var f = new Faker("en");

        foreach (var permission in permissions)
        {
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
                    NewPassword = "demo",
                    UserClaims =
                    [
                        new IdentityUserClaimEntity { ClaimType = WebshopClaimTypes.Permission, ClaimValue = permission }
                    ]
                };
                await userService.Save(user);
            }
            await userService.SaveChanges();
        }
    }
}