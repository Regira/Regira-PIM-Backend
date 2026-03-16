using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.Models;
using Webshop.Admin.Entities;
using Webshop.Identity.Data;

namespace Webshop.Admin.DependencyInjection;

public static class WebshopUserServiceConfiguration
{
    public static IEntityServiceCollection<WebshopAccountsDbContext> AddWebshopUsers(this IEntityServiceCollection<WebshopAccountsDbContext> services)
    {
        services.For<WebshopUserEntity, string, WebshopUserSearchObject, EntitySortBy, WebshopUserIncludes>(e =>
        {
            e.UseEntityService<WebshopUserRepository>();
        });
        return services;
    }
}