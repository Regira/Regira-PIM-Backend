using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PIM.Models.Taxonomy.FacetGroupFacets;
using PIM.Models.Taxonomy.FacetGroups;
using PIM.Models.Taxonomy.Facets;
using Regira.Normalizing.Abstractions;

namespace PIM.DataGenerator.OpenFoodFacts;

/// <summary>
/// Imports the Open Food Facts taxonomies as the PIM facet hierarchy: one FacetGroup per taxonomy file,
/// one Facet per entry, and one FacetLink per parent reference.
/// </summary>
/// <remarks>
/// The taxonomies are directed acyclic graphs, not trees — roughly 2,600 of the 14,600 categories declare
/// more than one parent. That is exactly what the many-to-many <see cref="FacetLink"/> table expresses,
/// so the shape survives the import intact.
/// </remarks>
public class OffTaxonomyImporter(
    OffDownloader downloader,
    OffOptions options,
    INormalizer normalizer,
    ILogger<OffTaxonomyImporter> logger)
{
    /// <summary>One entry of a taxonomy JSON file.</summary>
    private class TaxonomyEntry
    {
        [JsonPropertyName("parents")]
        public List<string>? Parents { get; set; }

        [JsonPropertyName("name")]
        public Dictionary<string, string>? Name { get; set; }
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        // A few taxonomy entries carry a scalar where a map is declared; skipping them beats aborting.
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private static readonly Dictionary<string, string> GroupTitles = new(StringComparer.Ordinal)
    {
        ["categories"] = "Categories",
        ["labels"] = "Labels & Certifications",
        ["allergens"] = "Allergens",
        ["additives"] = "Additives",
        ["food_groups"] = "Food Groups",
        ["countries"] = "Countries",
        ["origins"] = "Origins",
        ["packaging_materials"] = "Packaging Materials"
    };

    public async Task<OffFacetIndex> ImportAsync(SqlConnection connection, DbContext context, CancellationToken cancellationToken = default)
    {
        var index = new OffFacetIndex(options);

        await using var groupWriter = new EfBulkWriter<FacetGroup>(connection, context, options.BatchSize, options.BulkCopyTimeout);
        await using var facetWriter = new EfBulkWriter<Facet>(connection, context, options.BatchSize, options.BulkCopyTimeout);
        await using var linkWriter = new EfBulkWriter<FacetLink>(connection, context, options.BatchSize, options.BulkCopyTimeout);
        await using var groupLinkWriter = new EfBulkWriter<FacetParentGroup>(connection, context, options.BatchSize, options.BulkCopyTimeout);

        var now = DateTime.UtcNow;
        var nextFacetId = 1;
        var nextLinkId = 1;
        var nextGroupLinkId = 1;
        var groupId = 0;

        foreach (var taxonomy in options.Taxonomies)
        {
            var path = downloader.TaxonomyPath(taxonomy);
            if (!File.Exists(path))
            {
                logger.LogWarning("Taxonomy '{Taxonomy}' was not downloaded, skipping.", taxonomy);
                continue;
            }

            Dictionary<string, TaxonomyEntry>? entries;
            await using (var stream = File.OpenRead(path))
                entries = await JsonSerializer.DeserializeAsync<Dictionary<string, TaxonomyEntry>>(stream, SerializerOptions, cancellationToken);

            if (entries is null || entries.Count == 0)
            {
                logger.LogWarning("Taxonomy '{Taxonomy}' is empty, skipping.", taxonomy);
                continue;
            }

            groupId++;
            var title = GroupTitles.GetValueOrDefault(taxonomy, OffText.TitleFromKey(taxonomy));

            await groupWriter.AddAsync(new FacetGroup
            {
                Id = groupId,
                Code = taxonomy,
                Title = title,
                Description = $"Open Food Facts '{taxonomy}' taxonomy",
                AllowMultiSelect = true,
                Created = now,
                NormalizedTitle = normalizer.Normalize(OffText.Join(title, taxonomy)),
                NormalizedContent = normalizer.Normalize(OffText.Join(taxonomy, title, $"Open Food Facts '{taxonomy}' taxonomy"))
            });

            // Ids first, so a parent reference can be resolved regardless of file order.
            var idsByKey = new Dictionary<string, int>(entries.Count, StringComparer.Ordinal);
            foreach (var key in entries.Keys)
                idsByKey[key] = nextFacetId++;

            foreach (var (key, entry) in entries)
            {
                var name = OffText.DisplayName(key, entry.Name);
                await facetWriter.AddAsync(new Facet
                {
                    Id = idsByKey[key],
                    Code = key,
                    Title = name,
                    Description = $"{title}: {name}",
                    Created = now,
                    NormalizedTitle = normalizer.Normalize(OffText.Join(name, key)),
                    NormalizedContent = normalizer.Normalize(OffText.Join(key, name, $"{title}: {name}"))
                });
            }

            // Parent → child edges. The unique index on (ParentId, ChildId) means duplicates must not be sent.
            var seenLinks = new HashSet<(int, int)>();
            var danglingParents = 0;

            foreach (var (key, entry) in entries)
            {
                if (entry.Parents is not { Count: > 0 })
                    continue;

                foreach (var parent in entry.Parents)
                {
                    if (!idsByKey.TryGetValue(parent, out var parentId))
                    {
                        danglingParents++;
                        continue;
                    }

                    var childId = idsByKey[key];
                    if (parentId == childId || !seenLinks.Add((parentId, childId)))
                        continue;

                    await linkWriter.AddAsync(new FacetLink { Id = nextLinkId++, ParentId = parentId, ChildId = childId });
                }
            }

            // Root entries hang off the group, so the group is the entry point into the tree.
            var roots = entries.Where(e => e.Value.Parents is not { Count: > 0 }).Select(e => idsByKey[e.Key]).ToList();
            foreach (var rootId in roots)
                await groupLinkWriter.AddAsync(new FacetParentGroup { Id = nextGroupLinkId++, FacetGroupId = groupId, FacetId = rootId });

            index.Initialize(taxonomy, groupId, idsByKey);

            logger.LogInformation(
                "Taxonomy '{Taxonomy}': {Facets:N0} facets, {Links:N0} parent links, {Roots:N0} roots{Dangling}.",
                taxonomy, entries.Count, seenLinks.Count, roots.Count,
                danglingParents > 0 ? $", {danglingParents:N0} unresolved parent refs" : "");
        }

        await groupWriter.FlushAsync();
        await facetWriter.FlushAsync();
        await linkWriter.FlushAsync();
        await groupLinkWriter.FlushAsync();

        logger.LogInformation(
            "Taxonomy import done: {Groups} groups, {Facets:N0} facets, {Links:N0} facet links.",
            groupId, facetWriter.RowsWritten, linkWriter.RowsWritten);

        return index;
    }

    /// <summary>
    /// Writes the facets that were discovered on products but are absent from the published taxonomies.
    /// Must run after the catalog pass, which is what discovers them.
    /// </summary>
    public async Task WriteAdHocFacetsAsync(SqlConnection connection, DbContext context, OffFacetIndex index, int firstGroupLinkId)
    {
        if (index.AdHocFacets.Count == 0)
            return;

        await using var facetWriter = new EfBulkWriter<Facet>(connection, context, options.BatchSize, options.BulkCopyTimeout);
        await using var groupLinkWriter = new EfBulkWriter<FacetParentGroup>(connection, context, options.BatchSize, options.BulkCopyTimeout);

        foreach (var facet in index.AdHocFacets)
        {
            facet.NormalizedTitle = normalizer.Normalize(OffText.Join(facet.Title, facet.Code));
            facet.NormalizedContent = normalizer.Normalize(OffText.Join(facet.Code, facet.Title, facet.Description));
            await facetWriter.AddAsync(facet);
        }

        var nextId = firstGroupLinkId;
        foreach (var (groupId, facetId) in index.AdHocGroupLinks)
            await groupLinkWriter.AddAsync(new FacetParentGroup { Id = nextId++, FacetGroupId = groupId, FacetId = facetId });

        await facetWriter.FlushAsync();
        await groupLinkWriter.FlushAsync();

        logger.LogInformation("Wrote {Count:N0} facets discovered on products but absent from the taxonomies.", index.AdHocFacets.Count);
    }
}
