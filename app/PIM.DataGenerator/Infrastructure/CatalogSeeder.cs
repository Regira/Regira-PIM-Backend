using Bogus;
using Microsoft.Extensions.Logging;
using PIM.Models.Catalog.Products;
using PIM.Models.Catalog.UnitTypes;
using PIM.Models.Stakeholders.Parties;
using PIM.Models.Taxonomy.Facets;
using Regira.Entities.Services.Abstractions;

namespace PIM.DataGenerator.Infrastructure;

public class CatalogSeeder(IEntityRepository<Product> productService, IEntityService<UnitType> unitTypeService, ILogger<CatalogSeeder> logger)
{
    private const int BatchSize = 100;

    // Canonical ingredient list.
    // Key → (Description, UnitCode, Price, CategoryCodes)
    private static readonly IReadOnlyDictionary<string, (string Description, string UnitCode, decimal Price, string[] CategoryCodes)> CanonicalIngredients =
        new Dictionary<string, (string, string, decimal, string[])>(StringComparer.OrdinalIgnoreCase)
        {
            // Basics / Pantry
            ["Salt"]           = ("Fine sea salt",                  "g",  0.05m, ["PANTRY", "SPICES"]),
            ["Sugar"]          = ("White granulated sugar",         "g",  0.10m, ["PANTRY"]),
            ["Pepper"]         = ("Ground black pepper",            "g",  0.05m, ["PANTRY", "SPICES"]),
            ["Black Pepper"]   = ("Ground black pepper",            "g",  0.05m, ["PANTRY", "SPICES"]),
            ["Water"]          = ("Clean water",                    "ml", 0.01m, ["PANTRY"]),
            // Fats & oils
            ["Oil"]            = ("Cooking oil",                    "ml", 0.10m, ["PANTRY", "CONDIMENTS"]),
            ["Olive Oil"]      = ("Extra-virgin olive oil",         "ml", 0.25m, ["PANTRY", "CONDIMENTS"]),
            ["Vegetable Oil"]  = ("Refined vegetable oil",          "ml", 0.10m, ["PANTRY", "CONDIMENTS"]),
            ["Sesame Oil"]     = ("Toasted sesame oil",             "ml", 0.30m, ["PANTRY", "CONDIMENTS"]),
            ["Butter"]         = ("Salted butter",                  "g",  0.40m, ["DAIRY", "PANTRY"]),
            ["Ghee"]           = ("Clarified butter (ghee)",        "g",  0.45m, ["DAIRY", "PANTRY"]),
            // Aromatics
            ["Onion"]          = ("Yellow onion",                   "pc", 0.20m, ["VEGETABLES"]),
            ["Garlic"]         = ("Fresh garlic cloves",            "g",  0.15m, ["VEGETABLES"]),
            ["Ginger"]         = ("Fresh ginger root",              "g",  0.20m, ["VEGETABLES", "SPICES"]),
            ["Scallions"]      = ("Spring onions / scallions",      "g",  0.15m, ["VEGETABLES", "HERBS"]),
            ["Lemongrass"]     = ("Fresh lemongrass stalk",         "pc", 0.30m, ["HERBS", "VEGETABLES"]),
            // Spices
            ["Cumin"]          = ("Ground cumin",                   "g",  0.15m, ["SPICES"]),
            ["Turmeric"]       = ("Ground turmeric",                "g",  0.15m, ["SPICES"]),
            ["Cinnamon"]       = ("Ground cinnamon",                "g",  0.15m, ["SPICES"]),
            ["Paprika"]        = ("Sweet paprika",                  "g",  0.15m, ["SPICES"]),
            ["Allspice"]       = ("Ground allspice",                "g",  0.15m, ["SPICES"]),
            ["Cardamom"]       = ("Ground cardamom",                "g",  0.20m, ["SPICES"]),
            ["Cloves"]         = ("Whole cloves",                   "g",  0.20m, ["SPICES"]),
            ["Saffron"]        = ("Saffron threads",                "g",  1.50m, ["SPICES"]),
            ["Oregano"]        = ("Dried oregano",                  "g",  0.10m, ["HERBS", "SPICES"]),
            ["Thyme"]          = ("Fresh thyme",                    "g",  0.10m, ["HERBS"]),
            ["Rosemary"]       = ("Fresh rosemary",                 "g",  0.10m, ["HERBS"]),
            ["Bay Leaf"]       = ("Dried bay leaf",                 "pc", 0.05m, ["HERBS"]),
            ["Nutmeg"]         = ("Ground nutmeg",                  "g",  0.20m, ["SPICES"]),
            ["Fennel"]         = ("Fennel seeds",                   "g",  0.15m, ["SPICES", "HERBS"]),
            ["Garam Masala"]   = ("Garam masala spice blend",       "g",  0.20m, ["SPICES"]),
            ["Curry Powder"]   = ("Mild curry powder",              "g",  0.15m, ["SPICES"]),
            ["Coriander Powder"] = ("Ground coriander",             "g",  0.15m, ["SPICES"]),
            ["Chili Powder"]   = ("Chili powder blend",             "g",  0.15m, ["SPICES"]),
            ["Chili Flakes"]   = ("Dried red chili flakes",         "g",  0.15m, ["SPICES"]),
            // Herbs
            ["Parsley"]        = ("Fresh flat-leaf parsley",        "g",  0.10m, ["HERBS"]),
            ["Cilantro"]       = ("Fresh cilantro (coriander)",     "g",  0.10m, ["HERBS"]),
            ["Basil"]          = ("Fresh sweet basil",              "g",  0.10m, ["HERBS"]),
            ["Dill"]           = ("Fresh dill",                     "g",  0.10m, ["HERBS"]),
            ["Mint"]           = ("Fresh mint leaves",              "g",  0.10m, ["HERBS"]),
            // Grains & starchy
            ["Flour"]          = ("All-purpose wheat flour",        "g",  0.05m, ["GRAINS"]),
            ["Rice"]           = ("Long-grain white rice",          "g",  0.08m, ["GRAINS"]),
            ["Basmati Rice"]   = ("Fragrant basmati rice",          "g",  0.12m, ["GRAINS"]),
            ["Cornmeal"]       = ("Yellow cornmeal",                "g",  0.08m, ["GRAINS"]),
            ["Pasta"]          = ("Dried pasta",                    "g",  0.10m, ["GRAINS"]),
            ["Breadcrumbs"]    = ("Dry breadcrumbs",                "g",  0.08m, ["GRAINS"]),
            ["Cassava"]        = ("Fresh cassava root",             "g",  0.10m, ["GRAINS", "VEGETABLES"]),
            ["Phyllo Dough"]   = ("Frozen phyllo pastry sheets",    "g",  0.20m, ["GRAINS"]),
            // Vegetables
            ["Potatoes"]       = ("White potatoes",                 "g",  0.08m, ["VEGETABLES"]),
            ["Sweet Potato"]   = ("Orange sweet potato",           "g",  0.10m, ["VEGETABLES"]),
            ["Tomatoes"]       = ("Ripe tomatoes",                  "pc", 0.30m, ["VEGETABLES"]),
            ["Tomato"]         = ("Ripe tomato",                    "pc", 0.30m, ["VEGETABLES"]),
            ["Tomato Paste"]   = ("Concentrated tomato paste",      "g",  0.20m, ["VEGETABLES", "CONDIMENTS"]),
            ["Bell Pepper"]    = ("Mixed bell peppers",             "pc", 0.40m, ["VEGETABLES"]),
            ["Chili Pepper"]   = ("Fresh red chili pepper",         "pc", 0.20m, ["VEGETABLES", "SPICES"]),
            ["Scotch Bonnet"]  = ("Scotch bonnet pepper",           "pc", 0.25m, ["VEGETABLES", "SPICES"]),
            ["Carrots"]        = ("Fresh carrots",                  "g",  0.08m, ["VEGETABLES"]),
            ["Cabbage"]        = ("White cabbage",                  "g",  0.05m, ["VEGETABLES"]),
            ["Eggplant"]       = ("Aubergine / eggplant",           "g",  0.15m, ["VEGETABLES"]),
            ["Zucchini"]       = ("Courgette / zucchini",           "g",  0.12m, ["VEGETABLES"]),
            ["Mushrooms"]      = ("Button mushrooms",               "g",  0.45m, ["VEGETABLES"]),
            ["Spinach"]        = ("Fresh spinach leaves",           "g",  0.20m, ["VEGETABLES"]),
            ["Okra"]           = ("Fresh okra pods",                "g",  0.20m, ["VEGETABLES"]),
            ["Celery"]         = ("Celery stalk",                   "pc", 0.15m, ["VEGETABLES"]),
            ["Plantain"]       = ("Green plantain",                 "pc", 0.30m, ["VEGETABLES"]),
            // Legumes
            ["Chickpeas"]      = ("Canned chickpeas",               "g",  0.15m, ["LEGUMES"]),
            ["Beans"]          = ("Dried beans",                    "g",  0.12m, ["LEGUMES"]),
            ["Peas"]           = ("Frozen green peas",              "g",  0.10m, ["LEGUMES"]),
            ["Lentils"]        = ("Red lentils",                    "g",  0.12m, ["LEGUMES"]),
            // Fruits & other produce
            ["Lemon"]          = ("Fresh lemon",                    "pc", 0.25m, ["VEGETABLES"]),
            ["Lemon Juice"]    = ("Freshly squeezed lemon juice",   "ml", 0.20m, ["CONDIMENTS"]),
            ["Lime Juice"]     = ("Freshly squeezed lime juice",    "ml", 0.20m, ["CONDIMENTS"]),
            ["Raisins"]        = ("Seedless raisins",               "g",  0.20m, ["PANTRY"]),
            ["Mango"]          = ("Ripe mango",                     "pc", 0.60m, ["VEGETABLES"]),
            ["Banana"]         = ("Ripe banana",                    "pc", 0.20m, ["VEGETABLES"]),
            ["Coconut"]        = ("Desiccated coconut",             "g",  0.25m, ["PANTRY"]),
            ["Tamarind"]       = ("Tamarind paste",                 "g",  0.25m, ["CONDIMENTS", "SPICES"]),
            // Dairy & eggs
            ["Eggs"]           = ("Free-range eggs",                "pc", 0.35m, ["DAIRY"]),
            ["Egg"]            = ("Free-range egg",                 "pc", 0.35m, ["DAIRY"]),
            ["Milk"]           = ("Whole milk",                     "ml", 0.10m, ["DAIRY"]),
            ["Cream"]          = ("Heavy cooking cream",            "ml", 0.40m, ["DAIRY"]),
            ["Sour Cream"]     = ("Full-fat sour cream",            "ml", 0.30m, ["DAIRY"]),
            ["Yogurt"]         = ("Natural yogurt",                 "ml", 0.30m, ["DAIRY"]),
            ["Cheese"]         = ("Cheddar cheese",                 "g",  0.45m, ["DAIRY"]),
            ["Mozzarella"]     = ("Fresh mozzarella",               "g",  0.55m, ["DAIRY"]),
            ["Feta Cheese"]    = ("Crumbled feta cheese",           "g",  0.55m, ["DAIRY"]),
            ["Coconut Milk"]   = ("Full-fat coconut milk",          "ml", 0.25m, ["DAIRY", "PANTRY"]),
            // Proteins
            ["Chicken"]        = ("Bone-in chicken pieces",         "g",  0.50m, ["POULTRY"]),
            ["Beef"]           = ("Beef chuck",                     "g",  0.60m, ["BEEF"]),
            ["Ground Beef"]    = ("Lean ground beef",               "g",  0.55m, ["BEEF"]),
            ["Lamb"]           = ("Diced lamb shoulder",            "g",  0.70m, ["LAMB"]),
            ["Pork"]           = ("Pork shoulder",                  "g",  0.55m, ["PORK"]),
            ["Bacon"]          = ("Smoked bacon",                   "g",  0.60m, ["PORK"]),
            ["Fish"]           = ("White fish fillet",              "g",  0.65m, ["SEAFOOD"]),
            ["Shrimp"]         = ("Raw peeled shrimp",              "g",  0.80m, ["SEAFOOD"]),
            // Sauces & condiments
            ["Soy Sauce"]      = ("Light soy sauce",                "ml", 0.15m, ["CONDIMENTS"]),
            ["Fish Sauce"]     = ("Thai fish sauce",                "ml", 0.20m, ["CONDIMENTS"]),
            ["Vinegar"]        = ("White wine vinegar",             "ml", 0.10m, ["CONDIMENTS", "PANTRY"]),
            ["Mustard"]        = ("Dijon mustard",                  "g",  0.20m, ["CONDIMENTS"]),
            ["Peanut Butter"]  = ("Smooth peanut butter",           "g",  0.35m, ["CONDIMENTS", "LEGUMES"]),
            ["Beef Stock"]     = ("Rich beef stock",                "ml", 0.15m, ["CONDIMENTS"]),
        };

    public async Task<IList<Product>> SeedAsync(IList<Facet> facets, IList<Organization> suppliers)
    {
        var unitTypes = await SeedUnitTypes();
        return await SeedProducts(facets, suppliers, unitTypes);
    }

    public async Task<IList<UnitType>> SeedUnitTypes()
    {
        var types = await unitTypeService.List();

        if (types.Any())
        {
            logger.LogInformation("Unit types already exist, skipping seeding.");
            return types;
        }

        types = new List<UnitType>
        {
            new() { Code = "pc",      Title = "Piece"      },
            new() { Code = "portion", Title = "Portion"    },
            new() { Code = "slice",   Title = "Slice"      },
            new() { Code = "cup",     Title = "Cup"        },
            new() { Code = "bowl",    Title = "Bowl"       },
            new() { Code = "plate",   Title = "Plate"      },
            new() { Code = "g",       Title = "Gram"       },
            new() { Code = "kg",      Title = "Kilogram"   },
            new() { Code = "ml",      Title = "Milliliter" },
            new() { Code = "l",       Title = "Liter"      },
        };

        logger.LogInformation("Seeding unit types...");
        foreach (var type in types)
            await unitTypeService.Save(type);
        await unitTypeService.SaveChanges();

        return types;
    }

    public async Task<IList<Product>> SeedProducts(IList<Facet> facets, IList<Organization> suppliers, IList<UnitType> unitTypes)
    {
        var products = await productService.List();

        if (products.Any())
        {
            logger.LogInformation("Products already exist, skipping seeding.");
            return products;
        }

        var f = new Faker();
        var facetByTitle = facets.ToDictionary(x => x.Title, x => x, StringComparer.OrdinalIgnoreCase);
        var facetByCode  = facets
            .Where(x => !string.IsNullOrEmpty(x.Code))
            .ToDictionary(x => x.Code!, x => x, StringComparer.OrdinalIgnoreCase);
        var byCode = unitTypes.ToDictionary(u => u.Code!, u => u);
        var recipes = RecipeDataLoader.Load();

        // Phase 1: seed canonical ingredient products
        var ingredients = CanonicalIngredients
            .Select(kv => new Product
            {
                Title       = kv.Key,
                Description = kv.Value.Description,
                Prices      = [new ProductPricePeriod { Price = kv.Value.Price }],
                UnitTypeId  = byCode.TryGetValue(kv.Value.UnitCode, out var ut) ? ut.Id : null,
            })
            .ToList();

        logger.LogInformation("Seeding {Count} ingredient products...", ingredients.Count);
        foreach (var ingredient in ingredients)
        {
            if (f.Random.Bool(0.6f))
            {
                ingredient.Suppliers = f.PickRandom(suppliers, f.Random.Int(1, Math.Min(3, suppliers.Count)))
                    .DistinctBy(s => s.Id)
                    .Select(s => new ProductSupplier { SupplierId = s.Id })
                    .ToList();
            }
            await productService.Save(ingredient);
        }
        await productService.SaveChanges();

        // Assign category facets to ingredient products
        foreach (var ingredient in ingredients)
        {
            if (!CanonicalIngredients.TryGetValue(ingredient.Title, out var meta)) continue;
            var ingredientFacets = meta.CategoryCodes
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

        // Auto-create ingredient products for any CSV ingredient not yet in the canonical list,
        // so that every recipe ingredient can become an actual Product component.
        var allCsvIngredients = recipes
            .SelectMany(r => r.Ingredients)
            .Select(i => i.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n)
            .ToList();

        var extraIngredients = allCsvIngredients
            .Where(name => !ingByTitle.ContainsKey(name))
            .Select(name => new Product
            {
                Title       = name,
                Description = name,
                Prices      = [new ProductPricePeriod { Price = Math.Round(f.Random.Decimal(0.05m, 2.00m), 2) }],
                UnitTypeId  = byCode.TryGetValue("g", out var gUnit) ? gUnit.Id : null,
            })
            .ToList();

        if (extraIngredients.Count > 0)
        {
            logger.LogInformation("Seeding {Count} extra ingredient products from CSV...", extraIngredients.Count);
            foreach (var (extra, idx) in extraIngredients.Select((e, i) => (e, i)))
            {
                await productService.Save(extra);
                if ((idx + 1) % BatchSize == 0)
                    await productService.SaveChanges();
            }
            await productService.SaveChanges();

            // Assign guessed category facets to extra ingredients and add to the lookup
            foreach (var extra in extraIngredients)
            {
                ingByTitle[extra.Title] = extra;
                var extraFacets = GuessIngredientCategoryCodes(extra.Title)
                    .Where(facetByCode.ContainsKey)
                    .Select(code => new ProductFacet { FacetId = facetByCode[code].Id })
                    .ToList();
                if (extraFacets.Count > 0)
                {
                    extra.Facets = extraFacets;
                    await productService.Save(extra);
                }
            }
            await productService.SaveChanges();

            ingredients = [.. ingredients, .. extraIngredients];
        }

        // Helper lambda
        ProductComponent Comp(Product ing, decimal qty = 1, bool omittable = false)
            => new() { ComponentId = ing.Id, Quantity = qty, IsOmittable = omittable };

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
                .Select(e => Comp(ingByTitle[e.Name], qty: e.Quantity, omittable: f.Random.Bool(0.3f)))
                .DistinctBy(c => c.ComponentId)
                .ToList();

            // Facets: all country facets + category facets — NOT the dish-name facet itself
            var tags = new List<ProductFacet>();
            foreach (var country in allCountries)
                if (facetByTitle.TryGetValue(country, out var countryFacet))
                    tags.Add(new ProductFacet { FacetId = countryFacet.Id });

            // Add category facets based on ingredients and dish name
            var ingredientNames = primaryRecipe.Ingredients.Select(e => e.Name).ToList();
            foreach (var code in GetDishCategoryCodes(dishName, ingredientNames))
                if (facetByCode.TryGetValue(code, out var catFacet))
                    tags.Add(new ProductFacet { FacetId = catFacet.Id });

            // Description: origin + first up to 3 ingredients that are actual components
            var originText = allCountries.Count == 1
                ? allCountries[0]
                : $"{string.Join(", ", allCountries[..^1])} and {allCountries[^1]}";

            var describedIngredients = primaryRecipe.Ingredients
                .Where(e => ingByTitle.ContainsKey(e.Name))
                .Select(e => e.Name)
                .Take(3)
                .ToList();
            var description = describedIngredients.Count > 0
                ? $"A traditional dish from {originText} with {string.Join(", ", describedIngredients)}"
                : $"A traditional dish from {originText}";

            var product = new Product
            {
                Title       = dishName,
                Description = description,
                Prices      = [new ProductPricePeriod { Price = Math.Round(f.Random.Decimal(6.99m, 24.99m), 2) }],
                UnitTypeId  = f.PickRandom(servingUnitTypes).Id,
                AllowAdditions = f.Random.Bool(0.7f),
                Facets      = tags.DistinctBy(t => t.FacetId).ToList(),
                Components  = components,
                AllowedComponentAdditions = f.Random.Bool(0.4f) && ingByTitle.Count > 0
                    ? f.PickRandom(ingredients, f.Random.Int(1, Math.Min(3, ingredients.Count)))
                        .DistinctBy(i => i.Id)
                        .Select(i => new ProductAllowedComponentAddition { ComponentId = i.Id })
                        .ToList()
                    : null,
                Suppliers = f.Random.Bool(0.5f)
                    ? f.PickRandom(suppliers, f.Random.Int(1, Math.Min(2, suppliers.Count)))
                        .DistinctBy(s => s.Id)
                        .Select(s => new ProductSupplier { SupplierId = s.Id })
                        .ToList()
                    : null,
            };

            dishProducts.Add(product);
        }

        logger.LogInformation("Seeding {Count} unique dish products...", dishProducts.Count);
        foreach (var (product, idx) in dishProducts.Select((p, i) => (p, i)))
        {
            await productService.Save(product);
            if ((idx + 1) % BatchSize == 0)
                await productService.SaveChanges();
        }
        await productService.SaveChanges();

        // Phase 3: random product variants for volume
        string[] adjectives = ["Spicy", "Smoky", "Crispy", "Golden", "Grilled", "Fresh", "Loaded", "Signature", "Double", "BBQ", "Tangy", "Savory", "Hearty", "Zesty", "Rustic"];
        string[] nouns = ["Stew", "Wrap", "Bowl", "Salad", "Platter", "Bake", "Roast", "Curry", "Broth", "Medley"];

        decimal ComponentQty(Product component) =>
            CanonicalIngredients.TryGetValue(component.Title, out var meta) ? meta.UnitCode switch
            {
                "pc" => f.Random.Int(1, 3),
                "g"  => f.PickRandom(25m, 50m, 75m, 100m, 150m),
                "ml" => f.PickRandom(15m, 30m, 50m, 100m, 150m),
                "kg" => f.PickRandom(0.25m, 0.5m, 1m),
                "l"  => f.PickRandom(0.25m, 0.5m, 1m),
                _    => 1m
            } : 1m;

        logger.LogInformation("Seeding 500 random variant products...");
        for (int i = 0; i < 500; i++)
        {
            var selectedIngredients = f.PickRandom(ingredients, f.Random.Int(2, Math.Min(6, ingredients.Count)))
                .DistinctBy(c => c.Id)
                .ToList();

            var randomProduct = new Product
            {
                Title       = $"{f.PickRandom(adjectives)} {f.PickRandom(nouns)} #{i + 1}",
                Description = f.Lorem.Sentence(),
                Prices      = [new ProductPricePeriod { Price = Math.Round(f.Random.Decimal(3.50m, 24.99m), 2) }],
                UnitTypeId  = f.PickRandom(servingUnitTypes).Id,
                Facets      = f.PickRandom(facets, f.Random.Int(1, 3))
                    .DistinctBy(c => c.Id)
                    .Select(c => new ProductFacet { FacetId = c.Id })
                    .ToList(),
                Components  = selectedIngredients
                    .Select(c => new ProductComponent { ComponentId = c.Id, Quantity = ComponentQty(c), IsOmittable = f.Random.Bool(0.4f) })
                    .ToList(),
                AllowAdditions = f.Random.Bool(0.9f),
                AllowedComponentAdditions = f.Random.Bool(0.5f)
                    ? f.PickRandom(ingredients, f.Random.Int(1, Math.Min(3, ingredients.Count)))
                        .DistinctBy(c => c.Id)
                        .Select(c => new ProductAllowedComponentAddition { ComponentId = c.Id })
                        .ToList()
                    : null,
                Suppliers = f.Random.Bool(0.5f)
                    ? f.PickRandom(suppliers, f.Random.Int(1, Math.Min(2, suppliers.Count)))
                        .DistinctBy(s => s.Id)
                        .Select(s => new ProductSupplier { SupplierId = s.Id })
                        .ToList()
                    : null,
            };

            dishProducts.Add(randomProduct);
            await productService.Save(randomProduct);
            if ((i + 1) % BatchSize == 0)
                await productService.SaveChanges();
        }
        await productService.SaveChanges();

        return dishProducts;
    }

    /// <summary>
    /// Determines relevant facet category codes for a dish based on its name and ingredient list.
    /// </summary>
    private static IEnumerable<string> GetDishCategoryCodes(string dishName, IReadOnlyList<string> ingredients)
    {
        var dish = dishName.ToLower();
        var ings = ingredients.Select(i => i.ToLower()).ToHashSet();
        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // ── Course ──────────────────────────────────────────────────────────
        if (ContainsAny(dish, "cake", "pudding", "ice cream", "halva", "baklava", "duff",
                "torte", "tart", "sweet", "dessert", "ladoo", "mochi", "gulab", "tiramisu",
                "mousse", "strudel", "churro", "flan"))
            codes.Add("DESSERTS");
        else if (ContainsAny(dish, "juice", "tea", "coffee", "smoothie", "cocktail", "lassi", "ayran"))
            codes.Add("DRINKS");
        else if (ContainsAny(dish, "fufu", "ugali", "injera", "bogobe", "sadza", "tô",
                "bread", "naan", "pita", "flatbread", "rice", "chips", "fries", "polenta"))
            codes.Add("SIDES");
        else if (ContainsAny(dish, "salad", "hummus", "guacamole", "bruschetta",
                "spring roll", "dumpling", "samosa", "fritter", "empanada", "kibbeh"))
            codes.Add("STARTERS");
        else
            codes.Add("MAIN");

        // ── Food type ────────────────────────────────────────────────────────
        if (ContainsAny(dish, "soup", "broth", "shorba", "chorba", "chowder", "potage",
                "bisque", "harira", "shchi", "rassolnik"))
            codes.Add("SOUPS");
        if (ContainsAny(dish, "stew", "tagine", "wat", "casserole") ||
            ContainsAny(dish, "carbonade", "calulu", "machanka", "kavarma", "ndole"))
            codes.Add("SOUPS");
        if (ContainsAny(dish, "pasta", "spaghetti", "noodle", "kuy teav", "laksa",
                "pad thai", "lo mein"))
            codes.Add("PASTA");
        if (ContainsAny(dish, "rice", "biryani", "paella", "plov", "pilaf", "risotto",
                "jollof", "riz", "pilau", "mujaddara", "machboos", "muhammar", "nasi"))
            codes.Add("RICE");
        if (ContainsAny(dish, "curry", "masala", "korma", "dal", "rendang", "tikka",
                "vindaloo", "adobo", "ema datshi"))
            codes.Add("CURRIES");
        if (ContainsAny(dish, "grilled", "bbq", "roasted", "asado", "kebab", "satay",
                "khorovats", "souvla", "churrasco", "yakitori", "galbi", "bulgogi"))
            codes.Add("GRILLED");
        if (ContainsAny(dish, "bread", "naan", "pita", "injera", "flatbread", "roti",
                "chapati", "pancake", "crepe", "byrek", "burek", "banitsa", "phyllo",
                "pastel", "pie", "strudel", "pastry", "börek", "lavash"))
            codes.Add("BREADS");
        if (ContainsAny(dish, "salad"))
            codes.Add("SALADS");
        if (ContainsAny(dish, "hummus", "dip", "guacamole", "tahini", "tzatziki",
                "baba", "muhammara", "pesto", "aioli", "chimichurri"))
            codes.Add("DIPS");
        if (ContainsAny(dish, "fritter", "spring roll", "samosa", "falafel", "gyoza",
                "dumpling", "wonton", "kuli", "puff"))
            codes.Add("SNACKS");

        // ── Protein (from ingredient list) ────────────────────────────────
        if (IngredientsContainAny(ings, "chicken", "hen", "duck", "turkey", "galinha", "poulet"))
            codes.Add("POULTRY");
        if (IngredientsContainAny(ings, "beef", "steak", "brisket", "ground beef", "veal"))
            codes.Add("BEEF");
        if (IngredientsContainAny(ings, "pork", "bacon", "ham", "chorizo", "sausage",
                "lard", "pig", "ribs", "salt pork"))
            codes.Add("PORK");
        if (IngredientsContainAny(ings, "lamb", "mutton"))
            codes.Add("LAMB");
        if (IngredientsContainAny(ings, "fish", "shrimp", "prawn", "crab", "mussel",
                "salmon", "tuna", "cod", "lobster", "squid", "anchovy", "seafood", "conch",
                "barramundi", "hilsa", "dried fish", "smoked fish"))
            codes.Add("SEAFOOD");
        if (IngredientsContainAny(ings, "bean", "lentil", "chickpea", "split pea",
                "kidney bean", "black bean"))
            codes.Add("LEGUMES");

        return codes;
    }

    private static bool ContainsAny(string text, params string[] keywords)
        => keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));

    private static bool IngredientsContainAny(IEnumerable<string> ingredients, params string[] keywords)
        => ingredients.Any(ing => keywords.Any(k => ing.Contains(k, StringComparison.OrdinalIgnoreCase)));

    /// <summary>
    /// Guesses category facet codes for an ingredient product based on its name.
    /// Used when auto-creating ingredient products from CSV data.
    /// </summary>
    private static IEnumerable<string> GuessIngredientCategoryCodes(string ingredientName)
    {
        var name = ingredientName.ToLower();
        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (ContainsAny(name, "chicken", "duck", "turkey", "hen", "poultry", "drumstick", "wing", "thigh", "galinha"))
            codes.Add("POULTRY");
        if (ContainsAny(name, "beef", "steak", "brisket", "veal", "ox", "corned beef"))
            codes.Add("BEEF");
        if (ContainsAny(name, "pork", "bacon", "ham", "sausage", "chorizo", "lard", "ribs", "pig"))
            codes.Add("PORK");
        if (ContainsAny(name, "lamb", "mutton", "sheep"))
            codes.Add("LAMB");
        if (ContainsAny(name, "fish", "salmon", "tuna", "cod", "tilapia", "shrimp", "prawn",
                "crab", "mussel", "lobster", "squid", "anchovy", "sardine", "seafood", "conch",
                "barramundi", "hilsa", "flying fish"))
            codes.Add("SEAFOOD");
        if (ContainsAny(name, "bean", "lentil", "chickpea", "split pea", "legume", "dal", "tofu", "soy"))
            codes.Add("LEGUMES");
        if (ContainsAny(name, "pepper", "chili", "cayenne", "paprika", "cumin", "turmeric",
                "coriander", "cinnamon", "cardamom", "allspice", "nutmeg", "clove", "saffron",
                "spice", "blend", "masala", "berbere", "harissa", "ras el hanout", "sumac", "za'atar",
                "achu spice", "kanwa", "fenugreek", "anise", "caraway"))
            codes.Add("SPICES");
        if (ContainsAny(name, "thyme", "oregano", "rosemary", "basil", "dill", "mint",
                "parsley", "cilantro", "chive", "sage", "tarragon", "bay leaf", "lemongrass",
                "kaffir", "scallion", "leek"))
            codes.Add("HERBS");
        if (ContainsAny(name, "onion", "garlic", "ginger", "tomato", "potato", "carrot",
                "cabbage", "eggplant", "zucchini", "spinach", "okra", "celery",
                "turnip", "radish", "squash", "pumpkin", "yam", "cocoyam",
                "plantain", "cassava", "mango", "avocado", "vegetable", "greens", "mushroom",
                "artichoke", "asparagus", "broccoli", "cauliflower", "beet", "fennel"))
            codes.Add("VEGETABLES");
        if (ContainsAny(name, "flour", "rice", "corn", "maize", "wheat", "oat", "barley",
                "breadcrumb", "grain", "cereal", "starch", "polenta", "semolina", "teff",
                "sorghum", "millet", "injera", "couscous", "bulgur", "quinoa"))
            codes.Add("GRAINS");
        if (ContainsAny(name, "milk", "cream", "butter", "cheese", "yogurt", "egg", "dairy",
                "ghee", "mozzarella", "feta", "curd", "paneer", "ricotta", "parmesan"))
            codes.Add("DAIRY");
        if (ContainsAny(name, "oil", "vinegar", "sauce", "stock", "broth", "soy sauce",
                "fish sauce", "tahini", "paste", "ketchup", "mustard", "mayo", "dressing",
                "niter kibbeh", "palm oil", "coconut milk", "peanut butter", "tamarind"))
            codes.Add("CONDIMENTS");
        if (ContainsAny(name, "sugar", "honey", "syrup", "vanilla", "cocoa", "baking", "yeast",
                "water", "salt", "limestone"))
            codes.Add("PANTRY");

        // Fallback if nothing matched
        if (codes.Count == 0)
            codes.Add("PANTRY");

        return codes;
    }
}
