using PIM.Identity.Data;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.Models;

namespace PIM.Identity.Models;

public static class PimUserServiceConfiguration
{
    public static IEntityServiceCollection<AccountsDbContext> AddPimUsers(this IEntityServiceCollection<AccountsDbContext> services)
    {
        services.For<PimUserEntity, string, PimUserSearchObject, EntitySortBy, PimUserIncludes>(e =>
        {
            e.UseEntityService<PimUserRepository>();
        });
        return services;
    }
}