using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Webshop.Models.Entities.Catalog.Products;

namespace Webshop.Services.Entities.Catalog.Products;

public class ProductQueryBuilder : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
{
    public override IQueryable<Product> Build(IQueryable<Product> query, ProductSearchObject? so)
    {
        if (so == null)
            return query;

        if (so.CategoryId?.Any() == true)
            query = query.Where(x => x.Categories!.Any(pc => so.CategoryId.Contains(pc.CategoryId)));
        if (so.PartId?.Any() == true)
            query = query.Where(x => x.Parts!.Any(pp => so.PartId.Contains(pp.PartId)));
        if (so.AllergenId?.Any() == true)
            query = query.Where(x => x.Allergens!.Any(pa => so.AllergenId.Contains(pa.AllergenId)));

        var priceDate = so.PriceDate ?? DateTime.UtcNow;
        if (so.MinPrice.HasValue)
            query = query.Where(p => p.Prices!.Any(ph => (ph.StartDate == null || ph.StartDate <= priceDate) && (ph.EndDate == null || ph.EndDate >= priceDate) && ph.Price >= so.MinPrice));
        if (so.MaxPrice.HasValue)
            query = query.Where(p => p.Prices!.Any(ph => (ph.StartDate == null || ph.StartDate <= priceDate) && (ph.EndDate == null || ph.EndDate >= priceDate) && ph.Price <= so.MaxPrice));

        return query;
    }
}
