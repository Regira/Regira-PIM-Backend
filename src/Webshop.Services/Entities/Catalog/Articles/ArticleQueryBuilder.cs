using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Webshop.Models.Entities.Catalog.Articles;
using Webshop.Models.Entities.Catalog.Pricing.Utilities;

namespace Webshop.Services.Entities.Catalog.Articles;

public class ArticleQueryBuilder : FilteredQueryBuilderBase<Article, int, ArticleSearchObject>
{
    public override IQueryable<Article> Build(IQueryable<Article> query, ArticleSearchObject? so)
    {
        if (so == null)
            return query;

        if (so.CategoryId?.Any() == true)
            query = query.Where(x => x.Categories!.Any(ac => so.CategoryId.Contains(ac.CategoryId)));
        if (so.ComponentId?.Any() == true)
            query = query.Where(x => x.Components!.Any(ac => so.ComponentId.Contains(ac.ChildId)));
        foreach (var componentId in so.AllComponentId ?? [])
            query = query.Where(x => x.Components!.Any(ac => ac.ChildId == componentId));
        if (so.SupplierId?.Any() == true)
            query = query.Where(x => x.Suppliers!.Any(s => so.SupplierId.Contains(s.SupplierId)));

        var priceDate = so.PriceDate ?? DateTime.UtcNow;
        if (so.MinPrice.HasValue)
            query = query.Where(a => a.Prices!.Any(ph => (ph.StartDate == null || ph.StartDate <= priceDate) && (ph.EndDate == null || ph.EndDate >= priceDate) && ph.Price >= so.MinPrice));
        if (so.MaxPrice.HasValue)
            query = query.Where(a => a.Prices!.Any(ph => (ph.StartDate == null || ph.StartDate <= priceDate) && (ph.EndDate == null || ph.EndDate >= priceDate) && ph.Price <= so.MaxPrice));
        if (so.HasPrice.HasValue)
            query = query.Where(a => so.HasPrice.Value == a.Prices!.AsQueryable().Any(PriceHistoryUtility.IsActiveOn<ArticlePriceHistory>(priceDate)));

        return query;
    }
}
