using Bogus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PIM.Core.Constants;
using PIM.Identity.Models;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;

namespace PIM.AccountDataGenerator.Infrastructure;

public class AccountsDataSeeder(IEntityService<PimUserEntity, string, PimUserSearchObject, EntitySortBy, PimUserIncludes> userService, IConfiguration configuration, ILogger<AccountsDataSeeder> logger)
{
    public class AdminUserConfig
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public async Task SeedAsync()
    {
        var hadUsers = await userService.Count(new PimUserSearchObject()) > 0;
        await SeedAdminUsers();
        await SeedDemoUsers();
        if (!hadUsers)
            await SeedRandomUsers();
        await userService.SaveChanges();
    }

    private async Task SeedAdminUsers()
    {
        var adminUsers = configuration.GetSection("Identity:AdminUsers").Get<AdminUserConfig[]>() ?? [];
        foreach (var adminConfig in adminUsers)
        {
            if (await userService.Count(new { UserName = adminConfig.Email }) == 0)
            {
                logger.LogInformation($"Seeding admin user: {adminConfig.Email}");
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
        }

    }
    async Task SeedDemoUsers()
    {
        var demoUsers = configuration.GetSection("Identity:DemoUsers").Get<AdminUserConfig[]>() ?? [];

        foreach (var demoConfig in demoUsers)
        {
            if (await userService.Count(new { UserName = demoConfig.Email }) == 0)
            {
                logger.LogInformation($"Seeding demo user: {demoConfig.Email}");
                var user = new PimUserEntity
                {
                    Email = demoConfig.Email,
                    UserName = demoConfig.Email,
                    NewPassword = demoConfig.Password,
                    UserClaims =
                    [
                        new IdentityUserClaimEntity { ClaimType = PimClaimTypes.Permission, ClaimValue = PimPermissionValues.Editor }
                    ]
                };
                await userService.Save(user);
            }
        }
    }

    private async Task SeedRandomUsers()
    {
        logger.LogInformation("Seeding random users...");
        var permissions = Enumerable.Range(0, 50).Select(_ => PimPermissionValues.Supplier)
            .Concat(Enumerable.Range(0, 100).Select(_ => PimPermissionValues.Customer))
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
        }
    }
}