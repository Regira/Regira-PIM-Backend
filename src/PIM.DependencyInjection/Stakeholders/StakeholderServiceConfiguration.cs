using Microsoft.Extensions.DependencyInjection;
using PIM.Data;
using PIM.Services.Entities.Stakeholders.Addresses;
using PIM.Services.Entities.Stakeholders.ContactData;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Globalization.LibPhoneNumber;

namespace PIM.DependencyInjection.Stakeholders;

public static class StakeholderServiceConfiguration
{
    public static IEntityServiceCollection<PimDbContext> AddStakeholders(this IEntityServiceCollection<PimDbContext> services)
    {
        services.Services.AddTransient<PhoneNumberFormatter>();
        services.Services.AddTransient<ContactDataNormalizer>();
        services.Services.AddTransient<AddressNormalizer>();

        services.AddParties();
        services.AddRelationshipTypes();

        return services;
    }
}