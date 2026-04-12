using PIM.Data;
using PIM.Models.Catalog.Pricing.Utilities;
using PIM.Models.Catalog.Products;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Keywords.Abstractions;

namespace PIM.Services.Entities.Catalog.Products;

public class ProductQueryFilter(PimDbContext dbContext, IQKeywordHelper qHelper) : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
{
    public override IQueryable<Product> Build(IQueryable<Product> query, ProductSearchObject? so)
    {
        if (so == null)
            return query;

        if (!string.IsNullOrWhiteSpace(so.Title))
        {
            var keywords = qHelper.Parse(so.Title);
            query = query.FilterNormalizedTitle(keywords);
        }

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

        if (so.AncestorId?.Any() == true)
            query = query.Where(x => dbContext.GetProductOffspring(so.AncestorId, 9).Any(o => o.ChildId == x.Id));
        if (so.OffspringId?.Any() == true)
            query = query.Where(x => dbContext.GetProductAncestors(so.OffspringId, 9).Any(o => o.ParentId == x.Id));
        if (so.AllComponentId?.Any() == true)
        {
            // For each component, we need to check if it's in the assemblies or components of the product, or if it's in the ancestors of the product (i.e. the product is an offspring of the component).
            foreach (var componentId in so.AllComponentId)
                query = query.Where(x => dbContext.GetProductAncestors(new[] { componentId }, 9).Any(o => o.ParentId == x.Id));
        }

        if (so.SupplierId?.Any() == true)
            query = query.Where(x => x.Suppliers!.Any(s => so.SupplierId.Contains(s.SupplierId)));

        var priceDate = so.PriceDate ?? DateTime.UtcNow;
        if (so.MinPrice.HasValue)
            query = query.Where(x =>
                x.Prices!.Any(p => p.StartDate == null || p.StartDate <= priceDate)
                && x.Prices!.Where(p => p.StartDate == null || p.StartDate <= priceDate)
                    .OrderByDescending(p => p.StartDate)
                    .First().Price >= so.MinPrice);
        if (so.MaxPrice.HasValue)
            query = query.Where(x =>
                x.Prices!.Any(p => p.StartDate == null || p.StartDate <= priceDate)
                && x.Prices!.Where(p => p.StartDate == null || p.StartDate <= priceDate)
                    .OrderByDescending(p => p.StartDate)
                    .First().Price <= so.MaxPrice);
        if (so.HasPrice.HasValue)
            query = query.Where(a => so.HasPrice.Value == a.Prices!.AsQueryable().Any(PricePeriodUtility.IsActiveOn<ProductPricePeriod>(priceDate)));

        return query;
    }
}
