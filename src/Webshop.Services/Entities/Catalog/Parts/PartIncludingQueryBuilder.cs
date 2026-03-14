using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Webshop.Models.Entities.Catalog.Parts;

namespace Webshop.Services.Entities.Catalog.Parts;

public class PartIncludingQueryBuilder : IIncludableQueryBuilder<Part, int, PartIncludes>
{
    public IQueryable<Part> AddIncludes(IQueryable<Part> query, PartIncludes? includes = null)
    {
        query = query.Include(x => x.UnitType);

        if (includes == null)
            return query;

        if (includes.Value.HasFlag(PartIncludes.PriceHistory))
            query = query.Include(x => x.Prices);

        return query;
    }
}
