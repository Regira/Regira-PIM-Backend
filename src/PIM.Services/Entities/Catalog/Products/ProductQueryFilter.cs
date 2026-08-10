using System.Linq.Expressions;
using PIM.Data;
using PIM.Models.Catalog.Products;
using PIM.Models.Taxonomy.Facets;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.QueryBuilders.Abstractions;
using Regira.Entities.Keywords.Abstractions;

namespace PIM.Services.Entities.Catalog.Products;

public class ProductQueryFilter(PimDbContext dbContext, IQKeywordHelper qHelper) : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
{
    IQueryable<FacetTreeItem> GetFacetOffspring(IEnumerable<int>? facetIds, IEnumerable<int>? facetGroupIds = null)
        => dbContext.GetFacetOffspring(facetIds, facetGroupIds).Where(o => o.ChildType == nameof(Facet));

    /// <summary>
    /// A product matches when it carries one of the facets itself, or when one of its components does —
    /// a dish counts as containing gluten if one of its ingredients is marked with it.
    /// </summary>
    /// <remarks>
    /// This used to walk GetProductOffspring() with no id filter, which materialises the component closure
    /// of the entire catalog and then correlates it to each candidate product. On 1M products / 7.2M
    /// components that measured 47 seconds and 62.7 million logical reads. Reaching through the Components
    /// navigation expresses the same set — GetProductOffspring rows carry the *immediate* assembly in
    /// ParentId, so matching ParentId to the product only ever selected its direct components — and it
    /// resolves through the indexes on ProductComponent and ProductFacet instead.
    /// </remarks>
    static Expression<Func<Product, bool>> MatchesFacets(IEnumerable<int> facetIds)
        => x => x.Facets!.Any(pf => facetIds.Contains(pf.FacetId))
                || x.Components!.Any(c => c.Component!.Facets!.Any(pf => facetIds.Contains(pf.FacetId)));

    IQueryable<Product> FilterByFacets(IQueryable<Product> query, IEnumerable<int> facetIds)
        => query.Where(MatchesFacets(facetIds));

    IQueryable<Product> ExcludeByFacets(IQueryable<Product> query, IEnumerable<int> facetIds)
        => query.Where(Not(MatchesFacets(facetIds)));

    static Expression<Func<Product, bool>> Not(Expression<Func<Product, bool>> predicate)
        => Expression.Lambda<Func<Product, bool>>(Expression.Not(predicate.Body), predicate.Parameters);

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

        // Branch on the flag rather than comparing it to the expression. Comparing makes EF emit
        // "@flag <> CASE WHEN EXISTS (...) THEN 1 ELSE 0 END", where the parameter stops SQL Server
        // folding the CASE at compile time — so it evaluates the subquery per row instead of running
        // an anti-semi-join against IX_ProductComponent_ComponentId. Same rows either way.
        if (so.IsRoot == true)
            query = query.Where(x => !x.Assemblies!.Any());
        else if (so.IsRoot == false)
            query = query.Where(x => x.Assemblies!.Any());

        if (so.IsComponent == true)
            query = query.Where(x => x.Assemblies!.Any());
        else if (so.IsComponent == false)
            query = query.Where(x => !x.Assemblies!.Any());

        if (so.IsAssembly == true)
            query = query.Where(x => x.Components!.Any());
        else if (so.IsAssembly == false)
            query = query.Where(x => !x.Components!.Any());

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
