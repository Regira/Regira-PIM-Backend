using System.Globalization;
using Bogus;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using PIM.Models.Catalog.Products;
using PIM.Models.Catalog.UnitTypes;
using PIM.Models.Taxonomy.FacetGroups;
using PIM.Models.Taxonomy.Facets;
using PIM.Services.Entities.Taxonomy.Abstractions;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;

namespace PIM.DataGenerator.Infrastructure;

public class CatalogSeeder(IEntityRepository<Product> productService, IEntityService<FacetGroup, FacetGroupSearchObject, EntitySortBy, FacetGroupIncludes> facetGroupService, IEntityService<UnitType> unitTypeService, IFacetService facetService, ILogger<CatalogSeeder> logger)
{
    private const int BatchSize = 100;
    private static readonly HashSet<string> AllergenFacetCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "GLUTEN", "EGGS", "DAIRY", "FISH", "SHELLFISH", "PEANUTS", "SOY",
        "TREE_NUTS", "SESAME", "MUSTARD", "CELERY", "SULPHITES", "LUPIN"
    };

    // Canonical ingredient list loaded from CSV.
    // Key → (Description, UnitCode, CategoryCodes, IsAssembled)
    private static readonly Lazy<IReadOnlyDictionary<string, (string Description, string UnitCode, string[] CategoryCodes, bool IsAssembled)>> LazyCanonicalIngredients =
        new(LoadCanonicalIngredientsFromCsv);

    private static IReadOnlyDictionary<string, (string Description, string UnitCode, string[] CategoryCodes, bool IsAssembled)> CanonicalIngredients =>
        LazyCanonicalIngredients.Value;

    private record IngredientCsvRow
    {
        public string Code { get; init; } = "";
        public string Title { get; init; } = "";
        public string UnitType { get; init; } = "";
        public string Facets { get; init; } = "";
        public string DerivedFrom { get; init; } = "";
        public string Description { get; init; } = "";
    }

    /// <summary>
    /// Loads canonical ingredients from the ingredients.csv file.
    /// </summary>
    private static IReadOnlyDictionary<string, (string Description, string UnitCode, string[] CategoryCodes, bool IsAssembled)> LoadCanonicalIngredientsFromCsv()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "ingredients.csv");

        if (!File.Exists(path))
            return new Dictionary<string, (string, string, string[], bool)>(StringComparer.OrdinalIgnoreCase);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, config);

        var ingredients = csv.GetRecords<IngredientCsvRow>()
            .ToDictionary(
                row => row.Title,
                row =>
                (
                    Description: !string.IsNullOrWhiteSpace(row.Description) ? row.Description : row.Title,
                    UnitCode: row.UnitType,
                    CategoryCodes: row.Facets.Split(';').Select(f => f.Trim()).Where(f => !string.IsNullOrWhiteSpace(f)).ToArray(),
                    IsAssembled: !string.IsNullOrWhiteSpace(row.DerivedFrom)
                ),
                StringComparer.OrdinalIgnoreCase
            );

        Console.WriteLine($"Loaded {ingredients.Count} canonical ingredients from {path}");
        return ingredients;
    }

    public async Task<IList<Product>> SeedAsync()
    {
        var products = await productService.List();

        if (products.Any())
        {
            logger.LogInformation("Products already exist, skipping seeding.");
            return products;
        }

        var faker = new Faker();

        var unitTypes = await unitTypeService.List();
        var facetGroups = await facetGroupService.List([], [], FacetGroupIncludes.Facets, null);
        var facets = await facetService.List([], [], FacetIncludes.Parents | FacetIncludes.FacetGroups, null);
        //var facetTree = await facetService.GetOffspring(null, facetGroups.Select(g => g.Id).ToArray());

        var facetByTitle = facets.ToDictionary(x => x.Title, x => x, StringComparer.OrdinalIgnoreCase);
        var facetByCode = facets
            .Where(x => !string.IsNullOrEmpty(x.Code))
            .ToDictionary(x => x.Code!, x => x, StringComparer.OrdinalIgnoreCase);
        var unitByCode = unitTypes.ToDictionary(u => u.Code!, u => u, StringComparer.OrdinalIgnoreCase);
        // Reverse lookup: UnitTypeId → unit code (used for sensible quantity defaults)
        var unitCodeById = unitTypes
            .Where(u => u.Code != null)
            .ToDictionary(u => u.Id, u => u.Code!);
        var recipes = RecipeDataLoader.Load();
        var partialDishes = RecipeDataLoader.LoadPartialDishes();

        // Phase 1: seed canonical ingredient products
        var ingredients = CanonicalIngredients
            .Select(kv => new Product
            {
                Title = kv.Key,
                Description = kv.Value.Description,
                UnitTypeId = unitByCode.TryGetValue(kv.Value.UnitCode, out var ut) ? ut.Id : null,
                DefaultQuantity = kv.Value.IsAssembled
                    ? GetDefaultQuantityForIngredient(kv.Value.UnitCode, kv.Value.CategoryCodes)
                    : null,
            })
            .ToList();

        logger.LogInformation("Seeding {Count} ingredient products...", ingredients.Count);
        foreach (var ingredient in ingredients)
        {
            //if (f.Random.Bool(0.6f))
            //{
            //    ingredient.Suppliers = f.PickRandom(suppliers, f.Random.Int(1, Math.Min(3, suppliers.Count)))
            //        .DistinctBy(s => s.Id)
            //        .Select(s => new ProductSupplier { SupplierId = s.Id })
            //        .ToList();
            //}
            await productService.Save(ingredient);
        }
        await productService.SaveChanges();

        // Assign category facets to ingredient products.
        foreach (var ingredient in ingredients)
        {
            if (!CanonicalIngredients.TryGetValue(ingredient.Title, out var meta)) continue;
            var ingredientFacets = meta.CategoryCodes
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(facetByCode.ContainsKey)
                .Select(code => new ProductFacet { FacetId = facetByCode[code].Id })
                .ToList();
            if (ingredientFacets.Count > 0)
            {
                ingredient.Facets = ingredientFacets;
                await productService.Save(ingredient);
            }
        }
        await productService.SaveChanges();

        var ingByTitle = ingredients.ToDictionary(i => i.Title, i => i, StringComparer.OrdinalIgnoreCase);

        // Helper lambda — resolves the component quantity:
        // 1. Uses the explicit CSV quantity when provided (qty > 0).
        // 2. Falls back to the product's DefaultQuantity if set (covers assembled ingredients and partial dishes).
        // 3. Finally falls back to a unit-aware heuristic (100 g/ml, 1 for everything else).
        ProductComponent Comp(Product ing, decimal qty = 0, bool omittable = false)
        {
            if (qty == 0m)
            {
                qty = ing.DefaultQuantity
                    ?? (ing.UnitTypeId != null && unitCodeById.TryGetValue(ing.UnitTypeId ?? 0, out var unitCode)
                        ? unitCode switch { "g" => 100m, "mL" => 100m, _ => 1m }
                        : 1m);
            }
            return new ProductComponent { ComponentId = ing.Id, Quantity = qty, IsOmittable = omittable };
        }

        // Phase 1.5: seed partial dish products (stocks, sauces, pastes, bases, spice mixes)
        // Each partial dish is saved immediately and registered in ingByTitle so subsequent
        // partial dishes (e.g. Mole Sauce referencing Chicken Stock) can reference it.
        if (partialDishes.Count > 0)
        {
            logger.LogInformation("Seeding {Count} partial dish products...", partialDishes.Count);
            foreach (var partialDish in partialDishes)
            {
                var partialComponents = partialDish.Ingredients
                    .Where(e => ingByTitle.ContainsKey(e.Name))
                    .Select(e => Comp(ingByTitle[e.Name], qty: e.Quantity, omittable: false))
                    .DistinctBy(c => c.ComponentId)
                    .ToList();

                var unitCode = partialDish.Category switch
                {
                    "Stock" or "Sauce" => "mL",
                    "Base" => "portion",
                    _ => "g"   // Paste, Spice Mix, Blend
                };

                var partialFacetCodes = (IEnumerable<string>)partialDish.Facets;
                var partialFacets = partialFacetCodes
                    .Where(code => !IsAllergenFacetCode(code))
                    .Where(facetByCode.ContainsKey)
                    .Select(code => new ProductFacet { FacetId = facetByCode[code].Id })
                    .ToList();

                var product = new Product
                {
                    Title = partialDish.Name,
                    Description = !string.IsNullOrWhiteSpace(partialDish.Description)
                        ? partialDish.Description
                        : $"A {partialDish.Category.ToLower()} used as a base component in cooking",
                    UnitTypeId = unitByCode.TryGetValue(unitCode, out var partialUnit) ? partialUnit.Id : null,
                    DefaultQuantity = GetDefaultQuantityForPartialDish(partialDish.Category),
                    Facets = partialFacets,
                    Components = partialComponents,
                };

                // Register immediately so subsequent partial dishes and Phase 2 full dishes
                // can reference this product by name as a component.
                ingByTitle[product.Title] = product;

                await productService.Save(product);
                await productService.SaveChanges();
            }
        }

        // Phase 2: dish products from CSV recipes — deduplicated by dish name
        var servingUnitTypes = unitTypes
            .Where(u => u.Code is "pc" or "portion" or "plate" or "bowl")
            .ToList();

        var dishProducts = new List<Product>();

        // Group by unique dish name; each unique dish becomes one product
        foreach (var recipeGroup in recipes.GroupBy(r => r.Dish, StringComparer.OrdinalIgnoreCase))
        {
            var dishName = recipeGroup.Key;
            var primaryRecipe = recipeGroup.First();
            var allCountries = recipeGroup.Select(r => r.Country).Distinct().ToList();

            // Map every CSV ingredient to its Product component using the parsed quantity
            var components = primaryRecipe.Ingredients
                .Where(e => ingByTitle.ContainsKey(e.Name))
                .Select(e => Comp(ingByTitle[e.Name], qty: e.Quantity, omittable: faker.Random.Bool(0.3f)))
                .DistinctBy(c => c.ComponentId)
                .ToList();

            // Facets: all country facets + category facets — NOT the dish-name facet itself
            var tags = new List<ProductFacet>();
            foreach (var country in allCountries)
                if (facetByTitle.TryGetValue(country, out var countryFacet))
                    tags.Add(new ProductFacet { FacetId = countryFacet.Id });

            var categoryCodes = (IEnumerable<string>)primaryRecipe.Facets;
            foreach (var code in categoryCodes)
            {
                if (code.Equals("LEGUMES", StringComparison.OrdinalIgnoreCase) || IsAllergenFacetCode(code))
                    continue;
                if (facetByCode.TryGetValue(code, out var catFacet))
                    tags.Add(new ProductFacet { FacetId = catFacet.Id });
            }

            // Description: prefer CSV description; fall back to auto-generated origin+ingredient summary
            var description = !string.IsNullOrWhiteSpace(primaryRecipe.Description)
                ? primaryRecipe.Description
                : BuildDishDescription(primaryRecipe, allCountries, ingByTitle);

            var product = new Product
            {
                Title = dishName,
                Description = description,
                UnitTypeId = faker.PickRandom(servingUnitTypes).Id,
                DefaultQuantity = 1m,
                Facets = tags.DistinctBy(t => t.FacetId).ToList(),
                Components = components,
                //Suppliers = f.Random.Bool(0.5f)
                //    ? f.PickRandom(suppliers, f.Random.Int(1, Math.Min(2, suppliers.Count)))
                //        .DistinctBy(s => s.Id)
                //        .Select(s => new ProductSupplier { SupplierId = s.Id })
                //        .ToList()
                //    : null,
            };

            dishProducts.Add(product);
        }

        logger.LogInformation("Seeding {Count} unique dish products...", dishProducts.Count);
        foreach (var (product, idx) in dishProducts.Select((p, i) => (p, i)))
        {
            await productService.Save(product);
            if ((idx + 1) % BatchSize == 0)
            {
                logger.LogInformation($"Saving Products batch {idx + 1}...");
                await productService.SaveChanges();
            }
        }
        await productService.SaveChanges();

        return dishProducts;
    }

    /// <summary>
    /// Returns true when the ingredient name is a primary protein source (meat, poultry, seafood).
    /// Used to prioritize protein in dish descriptions.
    /// </summary>
    private static bool IsProteinIngredient(string name) =>
        ContainsAny(name.ToLower(),
            "chicken", "beef", "lamb", "pork", "fish", "shrimp", "prawn", "crab", "mussel",
            "salmon", "tuna", "cod", "turkey", "duck", "bacon", "ham", "sausage", "steak",
            "meat", "seafood", "lobster", "scallop", "oyster", "squid", "octopus", "conch",
            "barramundi", "hilsa", "anchovy", "sardine", "tilapia");

    /// <summary>
    /// Builds an auto-generated description for a dish based on its origin countries and key ingredients.
    /// </summary>
    private static string BuildDishDescription(RecipeEntry recipe, IReadOnlyList<string> allCountries, IDictionary<string, Product> ingByTitle)
    {
        var originText = allCountries.Count == 1
            ? allCountries[0]
            : $"{string.Join(", ", allCountries.SkipLast(1))} and {allCountries[^1]}";

        var validIngredients = recipe.Ingredients
            .Where(e => ingByTitle.ContainsKey(e.Name))
            .ToList();
        var proteinIngredient = validIngredients.FirstOrDefault(e => IsProteinIngredient(e.Name));
        var otherIngredients = validIngredients
            .Where(e => !IsProteinIngredient(e.Name))
            .Take(proteinIngredient != null ? 2 : 3)
            .Select(e => e.Name);
        var describedIngredients = (proteinIngredient != null
                ? new[] { proteinIngredient.Name }.Concat(otherIngredients)
                : otherIngredients)
            .ToList();

        return describedIngredients.Count > 0
            ? $"A traditional dish from {originText} with {string.Join(", ", describedIngredients)}"
            : $"A traditional dish from {originText}";
    }

    private static bool ContainsAny(string text, params string[] keywords)
        => keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));

    private static bool IsAllergenFacetCode(string code)
        => AllergenFacetCodes.Contains(code);

    /// <summary>
    /// Returns a realistic DefaultQuantity for an assembled ingredient (one with a DerivedFrom)
    /// based on its unit type and primary facet categories.
    /// </summary>
    private static decimal GetDefaultQuantityForIngredient(string unitCode, string[] facets)
    {
        return unitCode switch
        {
            "pc" => 1m,
            "bunch" => 1m,
            "clove" => 2m,
            "sprig" => 1m,
            "leaf" => 3m,
            "stalk" => 1m,
            "mL" => 50m,
            "g" => GetDefaultGramsForIngredient(facets),
            _ => 1m
        };
    }

    private static decimal GetDefaultGramsForIngredient(string[] facets)
    {
        // Main proteins — 150 g per person is a standard serving
        if (facets.Any(f => f is "BEEF" or "POULTRY" or "LAMB" or "PORK" or "LEAN_FISH" or "FAT_FISH" or "FISH"))
            return 150m;
        if (facets.Any(f => f is "CRUSTACEANS" or "MOLLUSKS" or "CRAB"))
            return 100m;
        // Spices and dried herbs — used in small pinches/teaspoons
        if (facets.Any(f => f is "SPICES" or "HERBS"))
            return 5m;
        // Condiments and sauces
        if (facets.Any(f => f is "CONDIMENTS"))
            return 30m;
        // Legumes (e.g. refried beans)
        if (facets.Any(f => f is "LEGUMES"))
            return 80m;
        // Starchy sides (e.g. French fries, mashed potatoes)
        if (facets.Any(f => f is "GRAINS" or "ROOT_VEG"))
            return 100m;
        return 40m;
    }

    /// <summary>
    /// Returns a realistic DefaultQuantity per serving for a partial dish based on its category.
    /// </summary>
    private static decimal GetDefaultQuantityForPartialDish(string category) =>
        category switch
        {
            "Stock" => 250m,        // 250 ml stock per serving
            "Sauce" => 150m,        // 150 ml sauce per serving
            "Paste" => 30m,         // ~2 tbsp paste per serving
            "Spice Mix" => 5m,      // ~1 tsp spice mix per serving
            "Base" => 1m,           // 1 portion (unit is already "portion")
            "Blend" => 200m,        // 200 g blended protein per serving
            _ => 100m
        };

}
