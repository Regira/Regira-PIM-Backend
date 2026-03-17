using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.Models;
using Webshop.Identity.Data;

namespace Webshop.Identity.Models;

public static class WebshopUserServiceConfiguration
{
    public static IEntityServiceCollection<AccountsDbContext> AddWebshopUsers(this IEntityServiceCollection<AccountsDbContext> services)
    {
        services.For<WebshopUserEntity, string, WebshopUserSearchObject, EntitySortBy, WebshopUserIncludes>(e =>
        {
            e.UseEntityService<WebshopUserRepository>();
        });
        return services;
    }
}