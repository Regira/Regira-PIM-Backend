using PIM.Core.Extensions;
using PIM.Data;
using PIM.Models.Stakeholders.Parties;
using PIM.Services.Entities.Stakeholders.Abstractions;
using PIM.Services.Entities.Stakeholders.Parties;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;

namespace PIM.DependencyInjection.Stakeholders;

public static class PartyServiceConfiguration
{
    public static IEntityServiceCollection<PimDbContext> AddParties(this IEntityServiceCollection<PimDbContext> services)
    {
        services.For<Party, PartySearchObject, PartySortBy, PartyIncludes>(e =>
        {
            e.AddFilter<PartyQueryFilter>();
            e.AddSortBy<PartySortingQueryBuilder>();
            e.AddIncludes<PartyIncludingQueryBuilder>();

            e.Related(item => item.ContactData, item => item.ContactData?.Prepare());
            e.Related(item => item.Addresses, item => item.Addresses?.Prepare());
            e.Related(
                item => item.ChildRelationships, item => item.ChildRelationships?.Prepare(),
                rel => rel.Related(r => r.ContactData, r => r.ContactData?.Prepare())
            );
            e.Related(
                item => item.ParentRelationships, item => item.ParentRelationships?.Prepare(),
                rel => rel.Related(r => r.ContactData, r => r.ContactData?.Prepare())
            );

            e.AddNormalizer<PartyNormalizer>();
            e.HasRepository<PartyRepository>();
            e.AddTransient<IPartyRepository, PartyRepository>();
            e.AddTransient<IPartyService, PartyRepository>();
        });

        return services;
    }
}
