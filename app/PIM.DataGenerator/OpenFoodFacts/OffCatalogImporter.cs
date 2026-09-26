using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PIM.Models.Catalog.Products;
using PIM.Models.Catalog.UnitTypes;
using PIM.Models.Stakeholders.Parties;
using Regira.Normalizing.Abstractions;

namespace PIM.DataGenerator.OpenFoodFacts;

/// <summary>
/// Streams the Open Food Facts product dump into the PIM catalog.
/// </summary>
/// <remarks>
/// <para>
/// The recursion comes from the parsed ingredient tree. A packaged product becomes an assembly whose
/// components are its top-level ingredients; a compound ingredient ("filling") is itself a product whose
/// components are its sub-ingredients ("ricotta", "milk"). Roughly 70% of OFF products carry at least one
/// compound ingredient, nesting up to four levels — the same shape the recipe seeder built by hand from
/// dish → partial dish → ingredient.
/// </para>
/// <para>
/// Ingredient products are shared: every product referencing "en:ricotta" points at one row, exactly as the
/// recipe seeder shared its canonical ingredients. That keeps the catalog navigable ("which products contain
/// ricotta?") but means a compound ingredient's children are global rather than per-parent, so the first
/// definition encountered wins and <see cref="OffOptions"/> caps how many children one may accumulate.
/// </para>
/// </remarks>
public class OffCatalogImporter(
    OffJsonlReader reader,
    OffDownloader downloader,
    OffOptions options,
    INormalizer normalizer,
    ILogger<OffCatalogImporter> logger)
{
    /// <summary>Unit types, with fixed Ids so component rows can reference them while streaming.</summary>
    private static readonly (int Id, string Code, string Title, bool IsUom)[] Units =
    [
        (1, "pc", "Piece", false),
        (2, "portion", "Portion", false),
        (3, "g", "Gram", true),
        (4, "kg", "Kilogram", true),
        (5, "mg", "Milligram", true),
        (6, "mL", "Milliliter", true),
        (7, "L", "Liter", true),
        (8, "cL", "Centiliter", true),
        (9, "oz", "Ounce", true),
        (10, "lb", "Pound", true),
        (11, "%", "Percent", true)
    ];

    private const int PercentUnitId = 11;

    private static readonly Dictionary<string, int> UnitIdByOffCode = new(StringComparer.OrdinalIgnoreCase)
    {
        ["g"] = 3, ["gram"] = 3, ["grams"] = 3,
        ["kg"] = 4, ["mg"] = 5,
        ["ml"] = 6, ["l"] = 7, ["cl"] = 8,
        ["oz"] = 9, ["lb"] = 10
    };

    private sealed record IngredientProduct(int Id, string Title);

    public async Task<OffImportStats> ImportAsync(
        SqlConnection connection,
        DbContext context,
        OffFacetIndex facetIndex,
        CancellationToken cancellationToken = default)
    {
        var stats = new OffImportStats();
        var now = DateTime.UtcNow;

        await WriteUnitTypesAsync(connection, context, now);

        // Shared state accumulated while streaming.
        var ingredients = new Dictionary<string, IngredientProduct>(StringComparer.Ordinal);
        var ingredientEdges = new HashSet<(int Assembly, int Component)>();
        var ingredientChildren = new Dictionary<int, List<int>>();
        var ingredientRank = new Dictionary<int, int>();
        var brands = new Dictionary<string, int>(StringComparer.Ordinal);

        var nextProductId = options.ProductIdBase;
        var nextIngredientId = options.IngredientProductIdBase;
        var nextBrandId = options.BrandPartyIdBase;
        var nextComponentId = 1;
        var nextProductFacetId = 1;
        var nextSupplierId = 1;

        await using var productWriter = new EfBulkWriter<Product>(connection, context, options.BatchSize, options.BulkCopyTimeout);
        await using var componentWriter = new EfBulkWriter<ProductComponent>(connection, context, options.BatchSize, options.BulkCopyTimeout);
        await using var productFacetWriter = new EfBulkWriter<ProductFacet>(connection, context, options.BatchSize, options.BulkCopyTimeout);
        await using var supplierWriter = new EfBulkWriter<ProductSupplier>(connection, context, options.BatchSize, options.BulkCopyTimeout);
        await using var organizationWriter = new EfBulkWriter<Organization>(connection, context, options.BatchSize, options.BulkCopyTimeout);

        var progressAt = DateTime.UtcNow;

        // The limit counts products actually imported, not records read, so it still means what it says
        // once a filter such as Language is rejecting most of the dump.
        await foreach (var offProduct in reader.ReadAsync(downloader.ProductsPath, maxProducts: null, cancellationToken))
        {
            if (options.MaxProducts is { } limit && stats.Products >= limit)
            {
                logger.LogInformation("Reached the configured limit of {Max:N0} products.", limit);
                break;
            }

            if (string.IsNullOrWhiteSpace(offProduct.Code))
            {
                stats.SkippedProducts++;
                continue;
            }

            if (options.RequireIngredients && offProduct.Ingredients is not { Count: > 0 })
            {
                stats.SkippedProducts++;
                continue;
            }

            if (options.Language is { Length: > 0 } language && !offProduct.HasNameIn(language))
            {
                stats.SkippedByLanguage++;
                continue;
            }

            var productId = nextProductId++;
            var title = OffText.ProductTitle(offProduct);
            var description = string.IsNullOrWhiteSpace(offProduct.GenericName) ? null : offProduct.GenericName.Trim();

            await productWriter.AddAsync(new Product
            {
                Id = productId,
                Title = title,
                Description = description,
                UnitTypeId = ResolveUnitId(offProduct.ProductQuantityUnit),
                DefaultQuantity = offProduct.ProductQuantity,
                Created = ToUtc(offProduct.CreatedT) ?? now,
                LastModified = ToUtc(offProduct.LastModifiedT),
                NormalizedTitle = normalizer.Normalize(title),
                NormalizedContent = normalizer.Normalize(OffText.Join(title, description)),
                ConcurrencyToken = Guid.NewGuid()
            });
            stats.Products++;

            // --- Facets -------------------------------------------------------------------------------
            var productFacetIds = new HashSet<int>();
            foreach (var (taxonomy, tags) in offProduct.FacetTags())
            {
                foreach (var tag in tags)
                {
                    var facetId = facetIndex.Resolve(taxonomy, tag);
                    if (facetId is null || !productFacetIds.Add(facetId.Value))
                        continue;

                    await productFacetWriter.AddAsync(new ProductFacet
                    {
                        Id = nextProductFacetId++,
                        ProductId = productId,
                        FacetId = facetId.Value
                    });
                    stats.ProductFacets++;
                }
            }

            // --- Suppliers ----------------------------------------------------------------------------
            if (options.ImportBrands && offProduct.BrandsTags is { Count: > 0 })
            {
                var linked = new HashSet<int>();
                foreach (var brandTag in offProduct.BrandsTags)
                {
                    if (string.IsNullOrWhiteSpace(brandTag))
                        continue;

                    if (!brands.TryGetValue(brandTag, out var brandId))
                    {
                        brandId = nextBrandId++;
                        brands[brandTag] = brandId;

                        var brandName = OffText.TitleFromKey(brandTag);
                        await organizationWriter.AddAsync(new Organization
                        {
                            Id = brandId,
                            Name = brandName,
                            Description = "Brand imported from Open Food Facts.",
                            Created = now,
                            NormalizedTitle = normalizer.Normalize(brandName),
                            NormalizedContent = normalizer.Normalize(OffText.Join(brandName, "Brand imported from Open Food Facts.")),
                            ConcurrencyToken = Guid.NewGuid()
                        });
                        stats.Brands++;
                    }

                    if (!linked.Add(brandId))
                        continue;

                    await supplierWriter.AddAsync(new ProductSupplier
                    {
                        Id = nextSupplierId++,
                        ProductId = productId,
                        SupplierId = brandId
                    });
                    stats.ProductSuppliers++;
                }
            }

            // --- Components (the recursive part) ------------------------------------------------------
            if (offProduct.Ingredients is { Count: > 0 })
            {
                var topLevel = new HashSet<int>();
                var sortOrder = 0;

                foreach (var node in offProduct.Ingredients)
                {
                    var ingredient = ResolveIngredient(node, ingredients, ref nextIngredientId);
                    if (ingredient is null)
                        continue;

                    if (topLevel.Add(ingredient.Id))
                    {
                        await componentWriter.AddAsync(new ProductComponent
                        {
                            Id = nextComponentId++,
                            AssemblyId = productId,
                            ComponentId = ingredient.Id,
                            Quantity = node.PercentEstimate ?? node.PercentMax ?? 0m,
                            IsOmittable = node.PercentMin is null or 0m,
                            SortOrder = sortOrder++
                        });
                        stats.Components++;
                    }

                    CollectNestedEdges(node, ingredient, ingredients, ingredientEdges, ingredientChildren, ingredientRank, ref nextIngredientId, stats);
                }
            }

            if (DateTime.UtcNow - progressAt > TimeSpan.FromSeconds(30))
            {
                logger.LogInformation(
                    "  {Products:N0} products, {Components:N0} components, {Facets:N0} facet links, {Ingredients:N0} ingredients...",
                    stats.Products, stats.Components, stats.ProductFacets, ingredients.Count);
                progressAt = DateTime.UtcNow;
            }
        }

        stats.MalformedLines = reader.MalformedLines;

        // Ingredient products are written last: their Ids come from a separate range, so the component rows
        // above could reference them before the rows themselves existed.
        logger.LogInformation("Writing {Count:N0} ingredient products...", ingredients.Count);
        foreach (var ingredient in ingredients.Values)
        {
            await productWriter.AddAsync(new Product
            {
                Id = ingredient.Id,
                Title = ingredient.Title,
                Description = "Ingredient from the Open Food Facts ingredients taxonomy.",
                UnitTypeId = PercentUnitId,
                Created = now,
                NormalizedTitle = normalizer.Normalize(ingredient.Title),
                NormalizedContent = normalizer.Normalize(OffText.Join(ingredient.Title, "Ingredient from the Open Food Facts ingredients taxonomy.")),
                ConcurrencyToken = Guid.NewGuid()
            });
        }
        stats.IngredientProducts = ingredients.Count;

        logger.LogInformation("Writing {Count:N0} ingredient sub-component links...", ingredientEdges.Count);
        foreach (var (assemblyId, componentId) in ingredientEdges)
        {
            await componentWriter.AddAsync(new ProductComponent
            {
                Id = nextComponentId++,
                AssemblyId = assemblyId,
                ComponentId = componentId,
                Quantity = 0m,
                IsOmittable = false,
                SortOrder = 0
            });
            stats.Components++;
        }

        await productWriter.FlushAsync();
        await componentWriter.FlushAsync();
        await productFacetWriter.FlushAsync();
        await supplierWriter.FlushAsync();
        await organizationWriter.FlushAsync();

        stats.OutOfRangeValues = productWriter.OutOfRangeValues + componentWriter.OutOfRangeValues
            + productFacetWriter.OutOfRangeValues + supplierWriter.OutOfRangeValues + organizationWriter.OutOfRangeValues;

        stats.MaxProductId = nextProductId - 1;
        stats.MaxIngredientProductId = nextIngredientId - 1;
        stats.MaxBrandPartyId = nextBrandId - 1;
        stats.MaxComponentId = nextComponentId - 1;
        stats.MaxProductFacetId = nextProductFacetId - 1;
        stats.MaxProductSupplierId = nextSupplierId - 1;

        return stats;
    }

    /// <summary>
    /// Gets or creates the shared product standing for an ingredient node.
    /// Nodes OFF could not resolve against its taxonomy carry no stable key, so they are skipped unless
    /// <see cref="OffOptions.TaxonomyIngredientsOnly"/> is off, in which case the literal text keys them.
    /// </summary>
    private IngredientProduct? ResolveIngredient(
        OffIngredient node,
        Dictionary<string, IngredientProduct> ingredients,
        ref int nextIngredientId)
    {
        var key = node.Id;

        if (string.IsNullOrWhiteSpace(key))
        {
            if (options.TaxonomyIngredientsOnly || string.IsNullOrWhiteSpace(node.Text))
                return null;
            key = $"text:{node.Text.Trim().ToLowerInvariant()}";
        }
        else if (options.TaxonomyIngredientsOnly && node.IsInTaxonomy != 1)
        {
            return null;
        }

        if (ingredients.TryGetValue(key, out var existing))
            return existing;

        var title = !string.IsNullOrWhiteSpace(node.Id)
            ? OffText.TitleFromKey(node.Id)
            : node.Text!.Trim();

        var created = new IngredientProduct(nextIngredientId++, title);
        ingredients[key] = created;
        return created;
    }

    /// <summary>
    /// Records the compound-ingredient edges that give the catalog its depth.
    /// </summary>
    /// <remarks>
    /// Ingredient products are shared across the whole catalog, so two products that disagree about which of
    /// "dough" and "flour" contains the other would close a loop. GetProductOffspring is a recursive CTE, and
    /// a loop in ProductComponent would make it churn to its level cap on every call — so each candidate edge
    /// is rejected when the child can already reach the parent.
    /// </remarks>
    private void CollectNestedEdges(
        OffIngredient node,
        IngredientProduct parent,
        Dictionary<string, IngredientProduct> ingredients,
        HashSet<(int, int)> edges,
        Dictionary<int, List<int>> children,
        Dictionary<int, int> rank,
        ref int nextIngredientId,
        OffImportStats stats)
    {
        if (node.Ingredients is not { Count: > 0 })
            return;

        foreach (var child in node.Ingredients)
        {
            var childIngredient = ResolveIngredient(child, ingredients, ref nextIngredientId);
            if (childIngredient is null)
                continue;

            if (!edges.Contains((parent.Id, childIngredient.Id)))
            {
                children.TryGetValue(parent.Id, out var siblings);
                rank.TryGetValue(parent.Id, out var parentRank);

                if (parentRank + 1 > options.MaxIngredientDepth)
                {
                    stats.SkippedDepthEdges++;
                    continue;
                }

                if (siblings is { Count: > 0 } && siblings.Count >= options.MaxComponentsPerIngredient)
                {
                    stats.SkippedOverflowEdges++;
                    continue;
                }

                if (parent.Id == childIngredient.Id || CanReach(childIngredient.Id, parent.Id, children))
                {
                    stats.SkippedCyclicEdges++;
                    continue;
                }

                edges.Add((parent.Id, childIngredient.Id));
                if (siblings is null)
                    children[parent.Id] = [childIngredient.Id];
                else
                    siblings.Add(childIngredient.Id);

                rank.TryGetValue(childIngredient.Id, out var childRank);
                if (parentRank + 1 > childRank)
                    rank[childIngredient.Id] = parentRank + 1;
            }

            CollectNestedEdges(child, childIngredient, ingredients, edges, children, rank, ref nextIngredientId, stats);
        }
    }

    /// <summary>
    /// Depth-first search for <paramref name="target"/> among the descendants of <paramref name="from"/>.
    /// </summary>
    private static bool CanReach(int from, int target, Dictionary<int, List<int>> children)
    {
        if (!children.ContainsKey(from))
            return false;

        var stack = new Stack<int>();
        var visited = new HashSet<int>();
        stack.Push(from);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!visited.Add(current) || !children.TryGetValue(current, out var next))
                continue;

            foreach (var child in next)
            {
                if (child == target)
                    return true;
                if (!visited.Contains(child))
                    stack.Push(child);
            }
        }

        return false;
    }

    private async Task WriteUnitTypesAsync(SqlConnection connection, DbContext context, DateTime now)
    {
        await using var writer = new EfBulkWriter<UnitType>(connection, context, options.BatchSize, options.BulkCopyTimeout);

        foreach (var (id, code, title, isUom) in Units)
        {
            await writer.AddAsync(new UnitType
            {
                Id = id,
                Code = code,
                Title = title,
                IsUom = isUom,
                Created = now,
                NormalizedContent = normalizer.Normalize(OffText.Join(title, code))
            });
        }

        await writer.FlushAsync();
        logger.LogInformation("Seeded {Count} unit types.", Units.Length);
    }

    private static int? ResolveUnitId(string? offUnit)
        => string.IsNullOrWhiteSpace(offUnit) ? null
            : UnitIdByOffCode.TryGetValue(offUnit.Trim(), out var id) ? id
            : null;

    /// <summary>
    /// Converts an OFF unix timestamp, rejecting values that cannot be represented.
    /// </summary>
    /// <remarks>
    /// The same community data that produced a product quantity of 312142424824242440000 also carries
    /// nonsense timestamps, and <see cref="DateTime.AddSeconds"/> throws rather than saturating — which
    /// would abort the import several million rows in, exactly as the quantity did.
    /// </remarks>
    private static DateTime? ToUtc(long? unixSeconds)
    {
        // 9999-12-31T23:59:59Z, the end of what DateTime and datetime2 can hold.
        const long maxUnixSeconds = 253402300799L;

        return unixSeconds is null or <= 0 || unixSeconds > maxUnixSeconds
            ? null
            : DateTime.UnixEpoch.AddSeconds(unixSeconds.Value);
    }
}
