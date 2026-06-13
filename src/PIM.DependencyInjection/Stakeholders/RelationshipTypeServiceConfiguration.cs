using PIM.Data;
using PIM.Models.Stakeholders.Parties;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace PIM.DependencyInjection.Stakeholders;

public static class RelationshipTypeServiceConfiguration
{
    public static IEntityServiceCollection<PimDbContext> AddRelationshipTypes(this IEntityServiceCollection<PimDbContext> services)
    {
        services.For<RelationshipType>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
        });

        return services;
    }
}