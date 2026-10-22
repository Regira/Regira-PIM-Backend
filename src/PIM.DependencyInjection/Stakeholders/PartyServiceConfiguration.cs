using PIM.Core.Extensions;
using PIM.Data;
using PIM.DependencyInjection.Catalog;
using PIM.Models.Stakeholders.Parties;
using PIM.Services.Entities.Stakeholders.Abstractions;
using PIM.Services.Entities.Stakeholders.Parties;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Reactors.Abstractions;

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
            e.AddValidator<PartyValidator>();
            // A renamed, archived or restored supplier changes the search text of the products it supplies. Title is
            // computed, so the stored column its names feed is watched instead.
            e.React(c => c.Kind == EntityChangeKind.Modified && (c.HasChanged(x => x.NormalizedTitle) || c.HasChanged(x => x.IsArchived)),
                (c, sp, _) => ProductSearchContentReactions.Refresh(sp, (refresher, token) => refresher.ForSupplier(c.Entity.Id, token)));
            e.HasRepository<PartyRepository>();
            e.AddTransient<IPartyRepository, PartyRepository>();
            e.AddTransient<IPartyService, PartyRepository>();
        });

        return services;
    }
}
