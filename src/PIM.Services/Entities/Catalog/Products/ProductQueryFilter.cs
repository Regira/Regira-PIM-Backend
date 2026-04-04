using PIM.Models.Catalog.Products;
using PIM.Models.Catalog.Pricing.Utilities;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

namespace PIM.Services.Entities.Catalog.Products;

public class ProductQueryFilter : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
{
    public override IQueryable<Product> Build(IQueryable<Product> query, ProductSearchObject? so)
    {
        if (so == null)
            return query;

        if (so.FacetId?.Any() == true)
            query = query.Where(x => x.Facets!.Any(ac => so.FacetId.Contains(ac.FacetId)));

        if (so.IsRoot.HasValue)
            query = query.Where(x => so.IsRoot.Value == !x.Assemblies!.Any());
        if (so.IsComponent.HasValue)
            query = query.Where(x => so.IsComponent.Value == x.Assemblies!.Any());
        if (so.IsAssembly.HasValue)
            query = query.Where(x => so.IsAssembly.Value == x.Components!.Any());

        if (so.AssemblyId?.Any() == true)
            query = query.Where(x => x.Assemblies!.Any(a => so.AssemblyId.Contains(a.AssemblyId)));
        if (so.ComponentId?.Any() == true)
            query = query.Where(x => x.Components!.Any(ac => so.ComponentId.Contains(ac.ComponentId)));
        foreach (var componentId in so.AllComponentId ?? [])
            query = query.Where(x => x.Components!.Any(ac => ac.ComponentId == componentId));
        if (so.SupplierId?.Any() == true)
            query = query.Where(x => x.Suppliers!.Any(s => so.SupplierId.Contains(s.SupplierId)));

        var priceDate = so.PriceDate ?? DateTime.UtcNow;
        if (so.MinPrice.HasValue)
            query = query.Where(a => a.Prices!.Any(ph => (ph.StartDate == null || ph.StartDate <= priceDate) && (ph.EndDate == null || ph.EndDate >= priceDate) && ph.Price >= so.MinPrice));
        if (so.MaxPrice.HasValue)
            query = query.Where(a => a.Prices!.Any(ph => (ph.StartDate == null || ph.StartDate <= priceDate) && (ph.EndDate == null || ph.EndDate >= priceDate) && ph.Price <= so.MaxPrice));
        if (so.HasPrice.HasValue)
            query = query.Where(a => so.HasPrice.Value == a.Prices!.AsQueryable().Any(PricePeriodUtility.IsActiveOn<ProductPricePeriod>(priceDate)));

        return query;
    }
}
