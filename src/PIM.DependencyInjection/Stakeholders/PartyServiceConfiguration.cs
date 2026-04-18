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

            e.AddPrepper<PartyPrepper>(); // put first!
            e.Related(x => x.ContactData, x => x.ContactData?.Prepare());
            e.Related(x => x.Addresses, x => x.Addresses?.Prepare());
            e.Related(x => x.ChildRelationships, x => x.ChildRelationships?.Prepare());
            e.Related(x => x.ParentRelationships, x => x.ParentRelationships?.Prepare());

            e.AddNormalizer<PartyNormalizer>();
            e.HasRepository<PartyRepository>();
            e.AddTransient<IPartyRepository, PartyRepository>();
            e.AddTransient<IPartyService, PartyRepository>();
        });

        return services;
    }
}
