using Microsoft.EntityFrameworkCore;
using PIM.Core.Constants;
using PIM.Data;
using PIM.Models.Catalog.UnitTypes;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Validators.Abstractions;

namespace PIM.DependencyInjection.Catalog;

public static class UnitTypeServiceConfiguration
{
    public static IEntityServiceCollection<PimDbContext> AddUnitTypes(this IEntityServiceCollection<PimDbContext> services)
    {
        services.For<UnitType>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
            // The code identifies the unit ("kg", "pcs"), so it is unique whatever its case
            e.Validate(async (ctx, db, token) =>
            {
                if (ctx.Operation == EntityWriteOperation.Remove || string.IsNullOrWhiteSpace(ctx.Item.Code))
                    return;
                var code = ctx.Item.Code.Trim().ToUpperInvariant();
                if (await db.UnitTypes.AnyAsync(x => x.Id != ctx.Item.Id && x.Code!.Trim().ToUpper() == code, token))
                    ctx.AddError(nameof(UnitType.Code), ValidationMessages.UnitTypeCodeTaken, new { code = ctx.Item.Code.Trim() });
            });
            // A unit type in use stays: products reference it, archived ones included
            e.Validate(async (ctx, db, token) =>
            {
                if (ctx.Operation != EntityWriteOperation.Remove)
                    return;
                var count = await db.Products.IgnoreQueryFilters().CountAsync(x => x.UnitTypeId == ctx.Item.Id, token);
                if (count > 0)
                    ctx.AddError("", ValidationMessages.UnitTypeInUse, new { count });
            });
        });
        return services;
    }
}
