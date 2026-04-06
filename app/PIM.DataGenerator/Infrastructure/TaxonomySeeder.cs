using Microsoft.Extensions.Logging;
using PIM.Models.Taxonomy.FacetGroupFacets;
using PIM.Models.Taxonomy.FacetGroups;
using PIM.Models.Taxonomy.Facets;
using Regira.Entities.Services.Abstractions;

namespace PIM.DataGenerator.Infrastructure;

public class TaxonomySeeder(IEntityService<Facet> facetService, IEntityService<FacetGroup> facetGroupService, ILogger<TaxonomySeeder> logger)
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

        // Level 2: Region facets
        var regionDescriptions = new Dictionary<string, string>
        {
            ["European"]      = "Cuisines from European countries",
            ["African"]       = "Cuisines from African countries",
            ["Asian"]         = "Cuisines from Asian countries",
            ["Middle Eastern"] = "Cuisines from the Middle East",
            ["Caribbean"]     = "Cuisines from Caribbean islands and surrounding regions",
            ["South American"] = "Cuisines from South American countries",
            ["North American"] = "Cuisines from North and Central American countries",
            ["Oceanian"]      = "Cuisines from Oceania",
        };

        var regionFacets = regionDescriptions
            .ToDictionary(
                kv => kv.Key,
                kv => new Facet { Title = $"{kv.Key} Cuisine", Description = kv.Value });

        // Level 3: Country facets
        var countryFacets = recipes
            .Select(r => r.Country)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c)
            .ToDictionary(
                c => c,
                c => new Facet { Title = c, Description = $"Traditional dishes from {c}" },
                StringComparer.OrdinalIgnoreCase);

        // Level 4: Dish facets — one facet per unique dish name (shared across countries)
        var dishFacets = recipes
            .GroupBy(r => r.Dish, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => new Facet { Title = g.Key, Description = $"Traditional dish from {g.First().Country}" },
                StringComparer.OrdinalIgnoreCase);

        // Dietary facets
        var veganFacet       = new Facet { Title = "Vegan",        Description = "Plant-based options" };
        var vegetarianFacet  = new Facet { Title = "Vegetarian",   Description = "Meat-free options" };
        var glutenFreeFacet  = new Facet { Title = "Gluten-Free",  Description = "Gluten-free options" };
        var ketoFacet        = new Facet { Title = "Keto",         Description = "Low-carb, high-fat options" };

        var allFacets = regionFacets.Values
            .Concat(countryFacets.Values)
            .Concat(dishFacets.Values)
            .Concat([veganFacet, vegetarianFacet, glutenFreeFacet, ketoFacet])
            .ToList();

        logger.LogInformation("Seeding {Count} facets...", allFacets.Count);

        foreach (var facet in allFacets)
            await facetService.Save(facet);
        await facetService.SaveChanges();

        // Parent-child relationships — saved after flush so IDs are assigned
        FacetLink Child(Facet child) => new() { ChildId = child.Id };

        // Region → Country
        var countriesByRegion = countryFacets.Keys
            .GroupBy(c => RecipeDataLoader.CountryRegions.GetValueOrDefault(c, string.Empty))
            .Where(g => !string.IsNullOrEmpty(g.Key));

        foreach (var group in countriesByRegion)
        {
            if (regionFacets.TryGetValue(group.Key, out var regionFacet))
                regionFacet.ChildEntities = group
                    .Select(c => Child(countryFacets[c]))
                    .ToList();
        }

        // Country → Dish (a dish facet may have multiple parent countries)
        var dishesByCountry = recipes
            .GroupBy(r => r.Country, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(r => r.Dish).ToList(), StringComparer.OrdinalIgnoreCase);

        foreach (var (country, dishNames) in dishesByCountry)
        {
            if (countryFacets.TryGetValue(country, out var countryFacet))
                countryFacet.ChildEntities = dishNames
                    .Where(d => dishFacets.ContainsKey(d))
                    .Select(d => Child(dishFacets[d]))
                    .ToList();
        }

        logger.LogInformation("Saving facet hierarchy...");
        foreach (var parent in regionFacets.Values.Concat(countryFacets.Values))
            await facetService.Save(parent);
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

        var byTitle = facets.ToDictionary(f => f.Title, f => f, StringComparer.OrdinalIgnoreCase);

        FacetParentGroup Link(Facet facet) => new() { FacetId = facet.Id };

        string[] regionTitles =
        [
            "European Cuisine", "African Cuisine", "Asian Cuisine",
            "Middle Eastern Cuisine", "Caribbean Cuisine", "South American Cuisine",
            "North American Cuisine", "Oceanian Cuisine"
        ];

        var groups = new List<FacetGroup>
        {
            new()
            {
                Code = "WORLD_CUISINES",
                Title = "World Cuisines",
                Description = "Cuisines from around the world, organized by region and country",
                ChildFacets = regionTitles
                    .Where(byTitle.ContainsKey)
                    .Select(t => Link(byTitle[t]))
                    .ToList()
            },
            new()
            {
                Code = "DIETARY",
                Title = "Dietary",
                Description = "Dietary preference categories",
                ChildFacets = [Link(byTitle["Vegan"]), Link(byTitle["Vegetarian"]),
                               Link(byTitle["Gluten-Free"]), Link(byTitle["Keto"])]
            },
        };

        logger.LogInformation("Seeding facet groups...");

        var groupLinks = groups.Select(g => { var links = g.ChildFacets; g.ChildFacets = null; return links; }).ToList();
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
