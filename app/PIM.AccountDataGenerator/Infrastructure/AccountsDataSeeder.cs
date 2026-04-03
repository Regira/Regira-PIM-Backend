using Bogus;
using Microsoft.Extensions.Configuration;
using PIM.Core.Constants;
using PIM.Identity.Models;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;

namespace PIM.AccountDataGenerator.Infrastructure;

public class AccountsDataSeeder(
    IEntityService<PimUserEntity, string, PimUserSearchObject, EntitySortBy, PimUserIncludes> userService,
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
            var user = new PimUserEntity
            {
                Email = adminConfig.Email,
                UserName = adminConfig.Email,
                NewPassword = adminConfig.Password,
                UserClaims =
                [
                    new IdentityUserClaimEntity { ClaimType = PimClaimTypes.Permission, ClaimValue = PimPermissionValues.Admin },
                    new IdentityUserClaimEntity { ClaimType = PimClaimTypes.Permission, ClaimValue = PimPermissionValues.Editor }
                ]
            };
            await userService.Save(user);
        }
        await userService.SaveChanges();
    }

    private async Task SeedRandomUsers()
    {
        var permissions = Enumerable.Range(0, 5).Select(_ => PimPermissionValues.Supplier)
            .Concat(Enumerable.Range(0, 10).Select(_ => PimPermissionValues.Customer))
            .ToArray();

        var f = new Faker("en");

        foreach (var permission in permissions)
        {
            {
                var firstName = f.Name.FirstName();
                var lastName = f.Name.LastName();
                var email = f.Internet.Email(firstName, lastName);

                var user = new PimUserEntity
                {
                    Email = email,
                    UserName = email,
                    GivenName = firstName,
                    LastName = lastName,
                    NewPassword = "demo",
                    UserClaims =
                    [
                        new IdentityUserClaimEntity { ClaimType = PimClaimTypes.Permission, ClaimValue = permission }
                    ]
                };
                await userService.Save(user);
            }
            await userService.SaveChanges();
        }
    }
}