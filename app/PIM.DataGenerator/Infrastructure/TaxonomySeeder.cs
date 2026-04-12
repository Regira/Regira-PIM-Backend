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
        var categoryEntries = RecipeDataLoader.LoadFacetCategories();

        // Category facets from facets.csv (Soups, Pasta, Poultry, Beef, Spices, Vegetables, etc.)
        var categoryFacets = categoryEntries.ToDictionary(
            e => e.Code,
            e => new Facet { Code = e.Code, Title = e.Title, Description = e.Description },
            StringComparer.OrdinalIgnoreCase);

        // Region facets (Level 2 of cuisine hierarchy)
        var regionDescriptions = new Dictionary<string, string>
        {
            ["European"]       = "Cuisines from European countries",
            ["African"]        = "Cuisines from African countries",
            ["Asian"]          = "Cuisines from Asian countries",
            ["Middle Eastern"] = "Cuisines from the Middle East",
            ["Caribbean"]      = "Cuisines from Caribbean islands and surrounding regions",
            ["South American"] = "Cuisines from South American countries",
            ["North American"] = "Cuisines from North and Central American countries",
            ["Oceanian"]       = "Cuisines from Oceania",
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

        // Region → Country
        foreach (var group in countryFacets.Keys.GroupBy(c =>
            RecipeDataLoader.CountryRegions.GetValueOrDefault(c, string.Empty))
            .Where(g => !string.IsNullOrEmpty(g.Key)))
        {
            if (regionFacets.TryGetValue(group.Key, out var regionFacet))
                regionFacet.ChildEntities = group
                    .Select(c => Child(countryFacets[c]))
                    .ToList();
        }

        // Category facet hierarchy — set parent-child links between category facets
        void SetChildren(string parentCode, params string[] childCodes)
        {
            if (!categoryFacets.TryGetValue(parentCode, out var parent)) return;
            parent.ChildEntities = childCodes
                .Where(categoryFacets.ContainsKey)
                .Select(code => Child(categoryFacets[code]))
                .ToList();
        }

        // Protein / meat hierarchy
        SetChildren("MEAT",     "RED_MEAT", "WHITE_MEAT", "POULTRY");
        SetChildren("RED_MEAT", "BEEF", "LAMB");
        SetChildren("WHITE_MEAT", "PORK");

        // Seafood hierarchy
        SetChildren("SEAFOOD",     "FISH", "SHELLFISH");
        SetChildren("FISH",        "FAT_FISH", "LEAN_FISH");
        SetChildren("SHELLFISH",   "CRUSTACEANS", "MOLLUSKS");
        SetChildren("CRUSTACEANS", "CRAB");

        // Dairy hierarchy
        SetChildren("DAIRY",  "MILK");
        SetChildren("MILK",   "BUTTER", "YOGHURT", "CHEESE");
        SetChildren("CHEESE", "HARD_CHEESE", "SOFT_CHEESE", "FRESH_CHEESE", "BLUE_CHEESE");

        // Vegetable hierarchy
        SetChildren("VEGETABLES", "ROOT_VEG", "LEAFY_VEG", "FLOWER_VEG", "FRUIT_VEG", "STEM_VEG");
        SetChildren("LEAFY_VEG",  "LETTUCE");
        SetChildren("FRUIT_VEG",  "TOMATOES");
        SetChildren("STEM_VEG",   "ONIONS");

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

        var groups = new List<FacetGroup>
        {
            new()
            {
                Code = "WORLD_CUISINES",
                Title = "World Cuisines",
                Description = "Cuisines from around the world organized by region and country",
                ChildFacets = regionTitles
                    .Select(F).Where(f => f != null).Select(f => Link(f!))
                    .ToList()
            },
            new()
            {
                Code = "COURSE",
                Title = "Course",
                Description = "Meal course categories",
                ChildFacets = new[] { "STARTERS", "MAIN", "SIDES", "DESSERTS", "DRINKS" }
                    .Select(FC).Where(f => f != null).Select(f => Link(f!))
                    .ToList()
            },
            new()
            {
                Code = "FOOD_TYPES",
                Title = "Food Types",
                Description = "Categorization by food preparation style",
                ChildFacets = new[] { "SOUPS", "PASTA", "RICE", "GRILLED", "CURRIES", "BREADS", "SALADS", "DIPS", "SNACKS" }
                    .Select(FC).Where(f => f != null).Select(f => Link(f!))
                    .ToList()
            },
            new()
            {
                Code = "PROTEINS",
                Title = "Proteins",
                Description = "Dishes and ingredients categorized by protein source",
                ChildFacets = new[] { "MEAT", "SEAFOOD", "LEGUMES" }
                    .Select(FC).Where(f => f != null).Select(f => Link(f!))
                    .ToList()
            },
            new()
            {
                Code = "INGREDIENTS",
                Title = "Ingredients",
                Description = "Ingredient types and pantry categories",
                ChildFacets = new[] { "VEGETABLES", "HERBS", "SPICES", "GRAINS", "DAIRY", "CONDIMENTS", "PANTRY" }
                    .Select(FC).Where(f => f != null).Select(f => Link(f!))
                    .ToList()
            },
            new()
            {
                Code = "DIETARY",
                Title = "Dietary",
                Description = "Dietary preference categories",
                ChildFacets = new[] { "VEGAN", "VEGETARIAN", "GLUTEN_FREE", "KETO", "RAW" }
                    .Select(FC).Where(f => f != null).Select(f => Link(f!))
                    .ToList()
            },
        };

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
