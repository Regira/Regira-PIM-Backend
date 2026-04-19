using Microsoft.Extensions.Logging;
using PIM.Models.Taxonomy.FacetGroupFacets;
using PIM.Models.Taxonomy.FacetGroups;
using PIM.Models.Taxonomy.Facets;
using PIM.Services.Entities.Taxonomy.Abstractions;
using Regira.Entities.Services.Abstractions;

namespace PIM.DataGenerator.Infrastructure;

public class TaxonomySeeder(IFacetService facetService, IEntityService<FacetGroup> facetGroupService, ILogger<TaxonomySeeder> logger)
{
    public async Task<IList<Facet>> SeedAsync()
    {
        var facets = await SeedFacets();
        await SeedFacetGroups(facets);
        return facets;
    }

    public async Task<IList<Facet>> SeedFacets()
    {
        var existing = await facetService.List();
        if (existing.Any())
        {
            logger.LogInformation("Facets already exist, skipping seeding.");
            return existing;
        }

        var recipes = RecipeDataLoader.Load();
        var categoryEntries = RecipeDataLoader.LoadFacetCategories();

        // Category facets from facets.csv (Soups, Pasta, Poultry, Beef, Spices, Vegetables, etc.)
        var categoryFacets = categoryEntries.ToDictionary(
            e => e.Code,
            e => new Facet { Code = e.Code, Title = e.Title, Description = e.Description },
            StringComparer.OrdinalIgnoreCase);

        // Region facets (Level 2 of cuisine hierarchy)
        var regionDescriptions = new Dictionary<string, string>
        {
            ["European"] = "Cuisines from European countries",
            ["African"] = "Cuisines from African countries",
            ["Asian"] = "Cuisines from Asian countries",
            ["Middle Eastern"] = "Cuisines from the Middle East",
            ["Caribbean"] = "Cuisines from Caribbean islands and surrounding regions",
            ["South American"] = "Cuisines from South American countries",
            ["North American"] = "Cuisines from North and Central American countries",
            ["Oceanian"] = "Cuisines from Oceania",
        };

        var regionFacets = regionDescriptions.ToDictionary(
            kv => kv.Key,
            kv => new Facet { Title = $"{kv.Key} Cuisine", Description = kv.Value });

        // Country facets (Level 3)
        var countryFacets = recipes
            .Select(r => r.Country)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c)
            .ToDictionary(
                c => c,
                c => new Facet { Title = c, Description = $"Traditional dishes from {c}" },
                StringComparer.OrdinalIgnoreCase);

        var allFacets = categoryFacets.Values
            .Concat(regionFacets.Values)
            .Concat(countryFacets.Values)
            .ToList();

        logger.LogInformation("Seeding {Count} facets...", allFacets.Count);

        foreach (var facet in allFacets)
            await facetService.Save(facet);
        await facetService.SaveChanges();

        // Parent-child relationships — set after flush so IDs are assigned
        FacetLink Child(Facet child) => new() { ChildId = child.Id };

        // Region → Country (driven by CountryRegions lookup)
        foreach (var group in countryFacets.Keys
            .GroupBy(c => RecipeDataLoader.CountryRegions.GetValueOrDefault(c, string.Empty))
            .Where(g => !string.IsNullOrEmpty(g.Key)))
        {
            if (regionFacets.TryGetValue(group.Key, out var regionFacet))
                regionFacet.ChildEntities = group
                    .Select(c => Child(countryFacets[c]))
                    .ToList();
        }

        // Category facet hierarchy — driven by ParentCode column in facets.csv
        foreach (var entry in categoryEntries.Where(e => !string.IsNullOrEmpty(e.ParentCode)))
        {
            if (!categoryFacets.TryGetValue(entry.ParentCode, out var parent)) continue;
            if (!categoryFacets.TryGetValue(entry.Code, out var child)) continue;
            parent.ChildEntities ??= [];
            parent.ChildEntities.Add(Child(child));
        }

        logger.LogInformation("Saving facet hierarchy...");
        foreach (var facet in regionFacets.Values
            .Concat(countryFacets.Values)
            .Concat(categoryFacets.Values.Where(f => f.ChildEntities?.Count > 0)))
            await facetService.Save(facet);
        await facetService.SaveChanges();

        return allFacets;
    }

    public async Task<IList<FacetGroup>> SeedFacetGroups(IList<Facet> facets)
    {
        var existing = await facetGroupService.List();
        if (existing.Any())
        {
            logger.LogInformation("Facet groups already exist, skipping seeding.");
            return existing;
        }

        var groupEntries = RecipeDataLoader.LoadFacetGroups();
        var facetEntries = RecipeDataLoader.LoadFacetCategories();

        // Map group code → list of facet codes (from FacetGroups column in facets.csv)
        var groupFacetCodes = facetEntries
            .SelectMany(e => e.FacetGroups
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(g => (GroupCode: g, FacetCode: e.Code)))
            .GroupBy(x => x.GroupCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(x => x.FacetCode).ToList(), StringComparer.OrdinalIgnoreCase);

        Facet? F(string title) => facets.FirstOrDefault(f =>
            string.Equals(f.Title, title, StringComparison.OrdinalIgnoreCase));
        Facet? FC(string code) => facets.FirstOrDefault(f =>
            string.Equals(f.Code, code, StringComparison.OrdinalIgnoreCase));

        FacetParentGroup Link(Facet facet) => new() { FacetId = facet.Id };

        string[] regionTitles =
        [
            "European Cuisine", "African Cuisine", "Asian Cuisine",
            "Middle Eastern Cuisine", "Caribbean Cuisine", "South American Cuisine",
            "North American Cuisine", "Oceanian Cuisine"
        ];

        var groups = groupEntries.Select(entry =>
        {
            // WORLD_CUISINES facets are region facets created dynamically (not in facets.csv)
            var childFacets = string.Equals(entry.Code, "WORLD_CUISINES", StringComparison.OrdinalIgnoreCase)
                ? regionTitles.Select(F).Where(f => f != null).Select(f => Link(f!)).ToList()
                : groupFacetCodes.TryGetValue(entry.Code, out var codes)
                    ? codes.Select(FC).Where(f => f != null).Select(f => Link(f!)).ToList()
                    : new List<FacetParentGroup>();

            return new FacetGroup
            {
                Code = entry.Code,
                Title = entry.Title,
                Description = entry.Description,
                ChildFacets = childFacets
            };
        }).ToList();

        logger.LogInformation("Seeding {Count} facet groups...", groups.Count);

        // Save groups without links first to get generated IDs,
        // then re-save with links to avoid EF Core MERGE FK conflicts.
        var groupLinks = groups.Select(g => { var l = g.ChildFacets; g.ChildFacets = null; return l; }).ToList();
        foreach (var group in groups)
            await facetGroupService.Save(group);
        await facetGroupService.SaveChanges();

        for (var i = 0; i < groups.Count; i++)
        {
            groups[i].ChildFacets = groupLinks[i];
            await facetGroupService.Save(groups[i]);
        }
        await facetGroupService.SaveChanges();

        return groups;
    }

}
