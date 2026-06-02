using PIM.Identity.Data;
using PIM.Identity.Models;
using PIM.Identity.Services;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.QueryBuilders.Abstractions;
using Regira.Entities.Models;

namespace PIM.Identity.DependencyInjection;

public static class PimUserServiceConfiguration
{
    public static IEntityServiceCollection<AccountsDbContext> AddPimUsers(this IEntityServiceCollection<AccountsDbContext> services)
    {
        services.For<PimUserEntity, string, PimUserSearchObject, EntitySortBy, PimUserIncludes>(e =>
        {
            // The internal DbContext User entity is of type PimIdentityUser, so we need to add a query filter for that type
            e.AddTransient<IFilteredQueryBuilder<PimIdentityUser, string, PimUserSearchObject>, PimUserQueryFiler>();
            e.HasRepository<PimUserRepository>();
            e.UseEntityService<PimUserRepository>();
        });
        return services;
    }
}