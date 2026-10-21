using Microsoft.EntityFrameworkCore;
using PIM.Core.Constants;
using PIM.Data;
using PIM.Models.Stakeholders.Parties;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Validators.Abstractions;

namespace PIM.DependencyInjection.Stakeholders;

public static class RelationshipTypeServiceConfiguration
{
    public static IEntityServiceCollection<PimDbContext> AddRelationshipTypes(this IEntityServiceCollection<PimDbContext> services)
    {
        services.For<RelationshipType>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
            // A relationship type in use stays: deleting it would take every relationship of that type with it
            e.Validate(async (ctx, db, token) =>
            {
                if (ctx.Operation != EntityWriteOperation.Remove)
                    return;
                var count = await db.Set<PartyRelationship>().CountAsync(x => x.RelationshipTypeId == ctx.Item.Id, token);
                if (count > 0)
                    ctx.AddError("", ValidationMessages.RelationshipTypeInUse, new { count });
            });
        });

        return services;
    }
}
