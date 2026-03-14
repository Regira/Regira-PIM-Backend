using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Globalization.LibPhoneNumber;
using Webshop.Data;
using Webshop.Services.Entities.Clients.Addresses;
using Webshop.Services.Entities.Clients.ContactData;

namespace Webshop.DependencyInjection.Clients;


public static class ClientServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddClients(this IEntityServiceCollection<WebshopDbContext> services)
    {
        services.Services
            .AddTransient<PhoneNumberFormatter>()
            .AddTransient<ContactDataNormalizer>()
            .AddTransient<AddressNormalizer>();

        return services
            .AddOrganizations()
            .AddCustomers();
    }
}