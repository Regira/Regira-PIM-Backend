using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Globalization.LibPhoneNumber;
using Webshop.Data;
using Webshop.Models.Stakeholders.Parties;
using Webshop.Models.Stakeholders.Parties.Relations;
using Webshop.Services.Entities.Stakeholders.Addresses;
using Webshop.Services.Entities.Stakeholders.ContactData;
using Webshop.Services.Entities.Stakeholders.Parties;

namespace Webshop.DependencyInjection.Stakeholders;

public static class StakeholderServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddStakeholders(this IEntityServiceCollection<WebshopDbContext> services)
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
