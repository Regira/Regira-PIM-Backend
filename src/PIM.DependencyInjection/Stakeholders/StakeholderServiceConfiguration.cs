using Microsoft.Extensions.DependencyInjection;
using PIM.Data;
using PIM.Models.Stakeholders.Parties;
using PIM.Models.Stakeholders.Parties.Relations;
using PIM.Services.Entities.Stakeholders.Addresses;
using PIM.Services.Entities.Stakeholders.ContactData;
using PIM.Services.Entities.Stakeholders.Parties;
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
        
        services.For<Party, PartySearchObject, PartySortBy, PartyIncludes>(e =>
        {
            e.AddFilter<PartyQueryBuilder>();
            e.AddSortBy<PartySortingQueryBuilder>();
            e.AddIncludes<PartyIncludingQueryBuilder>();

            e.Related(x => x.ContactData);
            e.Related(x => x.Addresses);
            e.Related(x => x.ChildRelationships);
            e.Related(x => x.ParentRelationships);

            e.AddNormalizer<PartyNormalizer>();
        });

        services.For<RelationshipType>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
        });

        return services;
    }
}
