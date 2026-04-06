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

        // Dish facets (Level 4) — one per unique dish name, shared across countries
        var dishFacets = recipes
            .GroupBy(r => r.Dish, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var countries = g.Select(r => r.Country).ToList();
                    var desc = countries.Count == 1
                        ? $"Traditional dish from {countries[0]}"
                        : $"Traditional dish shared by {string.Join(", ", countries)}";
                    return new Facet { Title = g.Key, Description = desc };
                },
                StringComparer.OrdinalIgnoreCase);

        var allFacets = categoryFacets.Values
            .Concat(regionFacets.Values)
            .Concat(countryFacets.Values)
            .Concat(dishFacets.Values)
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

        // Country → Dish
        foreach (var group in recipes.GroupBy(r => r.Country, StringComparer.OrdinalIgnoreCase))
        {
            if (countryFacets.TryGetValue(group.Key, out var countryFacet))
                countryFacet.ChildEntities = group
                    .Select(r => r.Dish)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Where(dishFacets.ContainsKey)
                    .Select(d => Child(dishFacets[d]))
                    .ToList();
        }

        // Dish facets get linked to food-type category facets
        // Build mapping: category code → dish facets
        var dishCategoryMap = BuildDishCategoryMap(recipes, categoryFacets);
        var dishesByCategory = new Dictionary<string, List<Facet>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (dishName, categoryCodes) in dishCategoryMap)
        {
            if (!dishFacets.TryGetValue(dishName, out var dishFacet)) continue;
            foreach (var code in categoryCodes)
            {
                if (!dishesByCategory.TryGetValue(code, out var list))
                    dishesByCategory[code] = list = [];
                list.Add(dishFacet);
            }
        }

        foreach (var (code, dishes) in dishesByCategory)
        {
            if (!categoryFacets.TryGetValue(code, out var categoryFacet)) continue;
            categoryFacet.ChildEntities = dishes
                .Select(d => new FacetLink { ChildId = d.Id })
                .ToList();
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
                ChildFacets = new[] { "POULTRY", "BEEF", "PORK", "LAMB", "SEAFOOD", "LEGUMES" }
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
                ChildFacets = new[] { "VEGAN", "VEGETARIAN", "GLUTEN_FREE", "KETO" }
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

    /// <summary>
    /// Maps each unique dish to a set of category facet codes based on
    /// the dish name (keyword matching) and its ingredients (protein detection).
    /// </summary>
    private static Dictionary<string, IReadOnlyList<string>> BuildDishCategoryMap(
        IReadOnlyList<RecipeEntry> recipes,
        IReadOnlyDictionary<string, Facet> categoryFacets)
    {
        var result = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var group in recipes.GroupBy(r => r.Dish, StringComparer.OrdinalIgnoreCase))
        {
            var dish = group.Key.ToLower();
            // Merge all ingredient lists from all countries that share this dish
            var ingredients = group
                .SelectMany(r => r.Ingredients)
                .Select(e => e.Name.ToLower())
                .ToHashSet();

            var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // ── Course ──────────────────────────────────────────────────────────
            if (ContainsAny(dish, "cake", "pudding", "ice cream", "halva", "baklava", "duff",
                    "torte", "tart", "sweet", "dessert", "ladoo", "mochi", "gulab", "tiramisu",
                    "mousse", "strudel", "churro", "flan", "crepe"))
                codes.Add("DESSERTS");
            else if (ContainsAny(dish, "juice", "tea", "coffee", "smoothie", "cocktail", "lemonade", "chai", "lassi", "ayran"))
                codes.Add("DRINKS");
            else if (ContainsAny(dish, "fufu", "ugali", "injera", "bogobe", "sadza", "tô",
                    "bread", "naan", "pita", "flatbread", "rice", "chips", "fries", "polenta", "grits"))
                codes.Add("SIDES");
            else if (ContainsAny(dish, "salad", "dip", "hummus", "guacamole", "bruschetta",
                    "spring roll", "dumpling", "samosa", "fritter", "empanada", "kibbeh", "starter"))
                codes.Add("STARTERS");
            else
                codes.Add("MAIN");

            // ── Food Type ───────────────────────────────────────────────────────
            if (ContainsAny(dish, "soup", "broth", "shorba", "chorba", "chowder", "potage", "bisque",
                    "bouillabaisse", "pho", "ramen", "harira", "shchi", "rassolnik"))
                codes.Add("SOUPS");
            if (ContainsAny(dish, "stew", "tagine", "wat", "doro", "casserole", "pot", "ragù") ||
                ContainsAny(dish, "carbonade", "calulu", "machanka", "kavarma"))
                codes.Add("SOUPS"); // stews grouped under soups
            if (ContainsAny(dish, "pasta", "spaghetti", "noodle", "pho", "ramen", "kuy teav",
                    "laksa", "pad thai", "lo mein", "mee", "bún", "bun cha"))
                codes.Add("PASTA");
            if (ContainsAny(dish, "rice", "biryani", "paella", "plov", "pilaf", "risotto",
                    "fried rice", "nasi", "arroz", "jollof", "riz", "pilau", "mujaddara", "machboos", "muhammar"))
                codes.Add("RICE");
            if (ContainsAny(dish, "curry", "masala", "korma", "dal", "daal", "rendang",
                    "tikka", "vindaloo", "adobo", "ema datshi"))
                codes.Add("CURRIES");
            if (ContainsAny(dish, "grilled", "bbq", "roasted", "asado", "kebab", "satay",
                    "khorovats", "souvla", "churrasco", "anticucho", "yakitori", "galbi", "bulgogi"))
                codes.Add("GRILLED");
            if (ContainsAny(dish, "bread", "naan", "pita", "injera", "flatbread", "roti",
                    "chapati", "pancake", "crepe", "waffle", "byrek", "burek", "banitsa", "phyllo",
                    "pastel", "pie", "tarte", "strudel", "apfelstrudel", "pastry", "empanada",
                    "patty", "knish", "samosa", "börek", "lavash"))
                codes.Add("BREADS");
            if (ContainsAny(dish, "salad"))
                codes.Add("SALADS");
            if (ContainsAny(dish, "hummus", "dip", "guacamole", "salsa", "tahini", "tzatziki",
                    "baba", "muhammara", "pesto", "aioli", "chimichurri"))
                codes.Add("DIPS");
            if (ContainsAny(dish, "fritter", "spring roll", "samosa", "falafel", "gyoza",
                    "dumpling", "wonton", "snack", "tapa", "bite", "chip", "kuli", "puff"))
                codes.Add("SNACKS");

            // ── Protein (from ingredient list) ─────────────────────────────────
            if (IngredientsContainAny(ingredients, "chicken", "hen", "duck", "turkey", "quail",
                    "poultry", "galinha", "poulet", "gallina"))
                codes.Add("POULTRY");
            if (IngredientsContainAny(ingredients, "beef", "steak", "brisket", "ground beef",
                    "veal", "ox", "corned beef"))
                codes.Add("BEEF");
            if (IngredientsContainAny(ingredients, "pork", "bacon", "ham", "chorizo", "sausage",
                    "lard", "pig", "ribs", "pork belly", "salt pork", "pig tail", "pig trotters"))
                codes.Add("PORK");
            if (IngredientsContainAny(ingredients, "lamb", "mutton", "sheep"))
                codes.Add("LAMB");
            if (IngredientsContainAny(ingredients, "fish", "shrimp", "prawn", "crab", "mussel",
                    "salmon", "tuna", "cod", "lobster", "squid", "octopus", "anchovy", "sardine",
                    "seafood", "conch", "barramundi", "hilsa", "flying fish", "dried fish", "smoked fish"))
                codes.Add("SEAFOOD");
            if (IngredientsContainAny(ingredients, "bean", "lentil", "chickpea", "pea",
                    "dal", "split pea", "kidney bean", "black bean", "lentils", "soybeans"))
                codes.Add("LEGUMES");

            result[group.Key] = [.. codes];
        }

        return result;
    }

    private static bool ContainsAny(string text, params string[] keywords)
        => keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));

    private static bool IngredientsContainAny(IEnumerable<string> ingredients, params string[] keywords)
        => ingredients.Any(ing => keywords.Any(k => ing.Contains(k, StringComparison.OrdinalIgnoreCase)));
}
