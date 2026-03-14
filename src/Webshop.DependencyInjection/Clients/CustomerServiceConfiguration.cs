using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.Extensions;
using Webshop.Data;
using Webshop.Models.Entities.Clients.Customers;
using Webshop.Services.Entities.Clients.Customers;

namespace Webshop.DependencyInjection.Clients;

public static class CustomerServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddCustomers(this IEntityServiceCollection<WebshopDbContext> services)
    {
        services.For<Customer, CustomerSearchObject, CustomerSortBy, CustomerIncludes>(e =>
        {
            e.AddQueryFilter<CustomerQueryBuilder>();
            e.SortBy<CustomerSortingQueryBuilder>();
            e.Includes<CustomerIncludingQueryBuilder>();
            e.Related(x => x.ContactData, item => item.ContactData?.SetSortOrder());
            e.Related(x => x.Addresses, item => item.Addresses?.SetSortOrder());
            e.Related(x => x.Organizations);
            e.AddNormalizer<CustomerNormalizer>();
        });
        return services;
    }
}
