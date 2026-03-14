using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Webshop.Models.Contexts;
using Webshop.Models.Entities.Catalog.Parts;

namespace Webshop.Services.Entities.Catalog.Parts;

public class PartQueryBuilder(IOrderContext orderContext) : FilteredQueryBuilderBase<Part, int, PartSearchObject>
{
    public override IQueryable<Part> Build(IQueryable<Part> query, PartSearchObject? so)
    {
        if (so == null)
            return query;

        if (so.Code != null)
            query = query.Where(p => p.Code == so.Code);

        var priceDate = orderContext.OrderDate ?? DateTime.UtcNow;
        if (so.MinPrice.HasValue)
            query = query.Where(p => p.Prices!.Any(ph =>
                (ph.StartDate == null || ph.StartDate <= priceDate)
                && (ph.EndDate == null || ph.EndDate >= priceDate)
                && ph.Price >= so.MinPrice));
        if (so.MaxPrice.HasValue)
            query = query.Where(p => p.Prices!.Any(ph =>
                (ph.StartDate == null || ph.StartDate <= priceDate)
                && (ph.EndDate == null || ph.EndDate >= priceDate)
                && ph.Price <= so.MaxPrice));

        return query;
    }
}
