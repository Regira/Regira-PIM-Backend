using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Webshop.Data;
using Webshop.Models.Entities.Clients.Organizations;

namespace Webshop.DependencyInjection.Clients;

public static class OrganizationServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddOrganizations(this IEntityServiceCollection<WebshopDbContext> services)
    {
        services.For<Organization>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
        return services;
    }
}
