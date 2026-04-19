using PIM.Data;
using PIM.Models.Catalog.Products;
using PIM.Models.Taxonomy.Facets;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Keywords.Abstractions;

namespace PIM.Services.Entities.Catalog.Products;

public class ProductQueryFilter(PimDbContext dbContext, IQKeywordHelper qHelper) : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
{
    IQueryable<FacetTreeItem> GetFacetOffspring(IEnumerable<int>? facetIds, IEnumerable<int>? facetGroupIds = null)
        => dbContext.GetFacetOffspring(facetIds, facetGroupIds).Where(o => o.ChildType == nameof(Facet));

    // Products inherit facets from their ancestors, so we also check ancestor products that carry the facets.
    IQueryable<Product> FilterByFacets(IQueryable<Product> query, IEnumerable<int> facetIds)
        => query.Where(x => x.Facets!.Any(ac => facetIds.Contains(ac.FacetId))
                            || dbContext.Set<ProductFacet>().Where(pf => facetIds.Contains(pf.FacetId))
                                .Any(pf => dbContext.GetProductOffspring().Where(o => o.ParentId == x.Id).Any(o => o.ChildId == pf.ProductId)));

    IQueryable<Product> ExcludeByFacets(IQueryable<Product> query, IEnumerable<int> facetIds)
        => query.Where(x => !(x.Facets!.Any(ac => facetIds.Contains(ac.FacetId))
                              || dbContext.Set<ProductFacet>().Where(pf => facetIds.Contains(pf.FacetId))
                                  .Any(pf => dbContext.GetProductOffspring().Where(o => o.ParentId == x.Id).Any(o => o.ChildId == pf.ProductId))));

    public override IQueryable<Product> Build(IQueryable<Product> query, ProductSearchObject? so)
    {
        if (so == null)
            return query;

        if (!string.IsNullOrWhiteSpace(so.Title))
        {
            var keywords = qHelper.Parse(so.Title);
            query = query.FilterNormalizedTitle(keywords);
        }

        if (so.FacetGroupId?.Any() == true)
        {
            var facetIds = GetFacetOffspring(null, so.FacetGroupId)
                .Select(o => o.ChildId)
                .ToHashSet();
            query = FilterByFacets(query, facetIds);
        }
        if (so.ExcludeFacetGroupId?.Any() == true)
        {
            // !(A) AND !(B) == !(A OR B): combine all groups into one set and exclude in a single pass
            var facetIds = GetFacetOffspring(null, so.ExcludeFacetGroupId)
                .Select(o => o.ChildId)
                .ToHashSet();
            query = ExcludeByFacets(query, facetIds);
        }

        if (so.FacetId?.Any() == true)
        {
            var offspringIds = GetFacetOffspring(so.FacetId)
                .Select(o => o.ChildId)
                .ToHashSet();
            var facetIds = offspringIds.Concat(so.FacetId).ToHashSet();
            query = FilterByFacets(query, facetIds);
        }
        if (so.AllFacetId?.Any() == true)
        {
            // Each facet is ANDed: product must match all of them — one Where per facet
            var facetOffspringIds = GetFacetOffspring(so.AllFacetId).ToList();
            foreach (var facetId in so.AllFacetId)
            {
                var facetIds = facetOffspringIds
                    .Where(o => o.ParentId == facetId)
                    .Select(o => o.ChildId)
                    .Append(facetId)
                    .ToHashSet();
                query = FilterByFacets(query, facetIds);
            }
        }
        if (so.ExcludeFacetId?.Any() == true)
        {
            // !(A) AND !(B) == !(A OR B): combine all excluded facets and their offspring into one set
            var facetIds = GetFacetOffspring(so.ExcludeFacetId)
                .Select(o => o.ChildId)
                .ToList()
                .Concat(so.ExcludeFacetId)
                .ToHashSet();
            query = ExcludeByFacets(query, facetIds);
        }

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
            foreach (var componentId in so.AllComponentId)
                query = query.Where(x => dbContext.GetProductAncestors(new[] { componentId }, 9).Any(o => o.ParentId == x.Id));
        }
        if (so.ExcludeComponentId?.Any() == true)
        {
            foreach (var componentId in so.ExcludeComponentId)
                query = query.Where(x => !dbContext.GetProductAncestors(new[] { componentId }, 9).Any(o => o.ParentId == x.Id));
        }

        if (so.SupplierId?.Any() == true)
            query = query.Where(x => x.Suppliers!.Any(s => so.SupplierId.Contains(s.SupplierId)));

        return query;
    }
}
