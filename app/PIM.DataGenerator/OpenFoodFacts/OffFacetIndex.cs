using PIM.Models.Taxonomy.Facets;

namespace PIM.DataGenerator.OpenFoodFacts;

/// <summary>
/// Maps Open Food Facts taxonomy keys ("en:hazelnut-spreads") onto the Facet Ids assigned during import,
/// one map per taxonomy. Tags a product carries but the published taxonomy does not define get an Id
/// allocated on demand from a separate range.
/// </summary>
public class OffFacetIndex(OffOptions options)
{
    private readonly Dictionary<string, Dictionary<string, int>> _byTaxonomy = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _groupIdByTaxonomy = new(StringComparer.Ordinal);
    private int _nextAdHocId;

    /// <summary>Facets discovered on products rather than in a taxonomy file, pending write.</summary>
    public List<Facet> AdHocFacets { get; } = [];

    /// <summary>Group membership rows for the ad-hoc facets, pending write.</summary>
    public List<(int GroupId, int FacetId)> AdHocGroupLinks { get; } = [];

    public void Initialize(string taxonomy, int facetGroupId, Dictionary<string, int> facetIdsByKey)
    {
        _byTaxonomy[taxonomy] = facetIdsByKey;
        _groupIdByTaxonomy[taxonomy] = facetGroupId;
        if (_nextAdHocId == 0)
            _nextAdHocId = options.AdHocFacetIdBase;
    }

    /// <summary>
    /// Resolves a product tag to a Facet Id, creating a facet for an unknown tag so that no product
    /// classification is silently dropped.
    /// </summary>
    public int? Resolve(string taxonomy, string tag)
    {
        if (string.IsNullOrWhiteSpace(tag) || !_byTaxonomy.TryGetValue(taxonomy, out var map))
            return null;

        if (map.TryGetValue(tag, out var existing))
            return existing;

        var id = _nextAdHocId++;
        map[tag] = id;

        AdHocFacets.Add(new Facet
        {
            Id = id,
            Code = tag,
            Title = OffText.TitleFromKey(tag),
            Description = $"Imported from the Open Food Facts '{taxonomy}' tags.",
            Created = DateTime.UtcNow
        });

        if (_groupIdByTaxonomy.TryGetValue(taxonomy, out var groupId))
            AdHocGroupLinks.Add((groupId, id));

        return id;
    }
}
