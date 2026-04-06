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
            ["Butter"]         = ("Salted butter",                  "g",  0.40m, ["BUTTER", "PANTRY"]),
            ["Ghee"]           = ("Clarified butter (ghee)",        "g",  0.45m, ["BUTTER", "PANTRY"]),
            // Aromatics
            ["Onion"]          = ("Yellow onion",                   "pc", 0.20m, ["ONIONS", "RAW_FOOD"]),
            ["Garlic"]         = ("Fresh garlic cloves",            "g",  0.15m, ["ONIONS", "RAW_FOOD"]),
            ["Ginger"]         = ("Fresh ginger root",              "g",  0.20m, ["STEM_VEG", "SPICES"]),
            ["Scallions"]      = ("Spring onions / scallions",      "g",  0.15m, ["ONIONS", "HERBS", "RAW_FOOD"]),
            ["Lemongrass"]     = ("Fresh lemongrass stalk",         "pc", 0.30m, ["STEM_VEG", "HERBS"]),
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
            ["Potatoes"]       = ("White potatoes",                 "g",  0.08m, ["ROOT_VEG"]),
            ["Sweet Potato"]   = ("Orange sweet potato",           "g",  0.10m, ["ROOT_VEG"]),
            ["Tomatoes"]       = ("Ripe tomatoes",                  "pc", 0.30m, ["TOMATOES", "RAW_FOOD"]),
            ["Tomato"]         = ("Ripe tomato",                    "pc", 0.30m, ["TOMATOES", "RAW_FOOD"]),
            ["Tomato Paste"]   = ("Concentrated tomato paste",      "g",  0.20m, ["TOMATOES", "CONDIMENTS"]),
            ["Bell Pepper"]    = ("Mixed bell peppers",             "pc", 0.40m, ["FRUIT_VEG", "RAW_FOOD"]),
            ["Chili Pepper"]   = ("Fresh red chili pepper",         "pc", 0.20m, ["FRUIT_VEG", "SPICES"]),
            ["Scotch Bonnet"]  = ("Scotch bonnet pepper",           "pc", 0.25m, ["FRUIT_VEG", "SPICES"]),
            ["Carrots"]        = ("Fresh carrots",                  "pc", 0.08m, ["ROOT_VEG", "RAW_FOOD"]),
            ["Cabbage"]        = ("White cabbage",                  "g",  0.05m, ["FLOWER_VEG", "RAW_FOOD"]),
            ["Eggplant"]       = ("Aubergine / eggplant",           "g",  0.15m, ["FRUIT_VEG"]),
            ["Zucchini"]       = ("Courgette / zucchini",           "g",  0.12m, ["FRUIT_VEG"]),
            ["Mushrooms"]      = ("Button mushrooms",               "g",  0.45m, ["VEGETABLES", "RAW_FOOD"]),
            ["Spinach"]        = ("Fresh spinach leaves",           "g",  0.20m, ["LEAFY_VEG", "RAW_FOOD"]),
            ["Okra"]           = ("Fresh okra pods",                "g",  0.20m, ["FLOWER_VEG"]),
            ["Celery"]         = ("Celery stalk",                   "pc", 0.15m, ["STEM_VEG", "RAW_FOOD"]),
            ["Plantain"]       = ("Green plantain",                 "pc", 0.30m, ["FRUIT_VEG"]),
            // Legumes
            ["Chickpeas"]      = ("Canned chickpeas",               "g",  0.15m, ["LEGUMES"]),
            ["Beans"]          = ("Dried beans",                    "g",  0.12m, ["LEGUMES"]),
            ["Peas"]           = ("Frozen green peas",              "g",  0.10m, ["LEGUMES"]),
            ["Lentils"]        = ("Red lentils",                    "g",  0.12m, ["LEGUMES"]),
            // Fruits & other produce
            ["Lemon"]          = ("Fresh lemon",                    "pc", 0.25m, ["FRUIT_VEG", "RAW_FOOD"]),
            ["Lemon Juice"]    = ("Freshly squeezed lemon juice",   "ml", 0.20m, ["CONDIMENTS"]),
            ["Lime Juice"]     = ("Freshly squeezed lime juice",    "ml", 0.20m, ["CONDIMENTS"]),
            ["Raisins"]        = ("Seedless raisins",               "g",  0.20m, ["PANTRY"]),
            ["Mango"]          = ("Ripe mango",                     "pc", 0.60m, ["FRUIT_VEG", "RAW_FOOD"]),
            ["Banana"]         = ("Ripe banana",                    "pc", 0.20m, ["FRUIT_VEG", "RAW_FOOD"]),
            ["Coconut"]        = ("Desiccated coconut",             "g",  0.25m, ["PANTRY"]),
            ["Tamarind"]       = ("Tamarind paste",                 "g",  0.25m, ["CONDIMENTS", "SPICES"]),
            // Dairy & eggs
            ["Eggs"]           = ("Free-range eggs",                "pc", 0.35m, ["DAIRY"]),
            ["Egg"]            = ("Free-range egg",                 "pc", 0.35m, ["DAIRY"]),
            ["Milk"]           = ("Whole milk",                     "ml", 0.10m, ["MILK"]),
            ["Cream"]          = ("Heavy cooking cream",            "ml", 0.40m, ["MILK"]),
            ["Sour Cream"]     = ("Full-fat sour cream",            "ml", 0.30m, ["MILK"]),
            ["Yogurt"]         = ("Natural yogurt",                 "ml", 0.30m, ["YOGHURT"]),
            ["Cheese"]         = ("Cheddar cheese",                 "g",  0.45m, ["HARD_CHEESE"]),
            ["Mozzarella"]     = ("Fresh mozzarella",               "g",  0.55m, ["FRESH_CHEESE"]),
            ["Feta Cheese"]    = ("Crumbled feta cheese",           "g",  0.55m, ["SOFT_CHEESE"]),
            ["Coconut Milk"]   = ("Full-fat coconut milk",          "ml", 0.25m, ["MILK", "PANTRY"]),
            // Proteins
            ["Chicken"]        = ("Bone-in chicken pieces",         "g",  0.50m, ["POULTRY"]),
            ["Beef"]           = ("Beef chuck",                     "g",  0.60m, ["BEEF"]),
            ["Ground Beef"]    = ("Lean ground beef",               "g",  0.55m, ["BEEF"]),
            ["Lamb"]           = ("Diced lamb shoulder",            "g",  0.70m, ["LAMB"]),
            ["Pork"]           = ("Pork shoulder",                  "g",  0.55m, ["PORK"]),
            ["Bacon"]          = ("Smoked bacon",                   "g",  0.60m, ["PORK"]),
            ["Fish"]           = ("White fish fillet",              "g",  0.65m, ["LEAN_FISH"]),
            ["Shrimp"]         = ("Raw peeled shrimp",              "g",  0.80m, ["CRUSTACEANS"]),
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
        // Reverse lookup: UnitTypeId → unit code (used for sensible quantity defaults)
        var unitCodeById = unitTypes
            .Where(u => u.Id != null && u.Code != null)
            .ToDictionary(u => u.Id!, u => u.Code!);
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
                UnitTypeId  = byCode.TryGetValue(GuessUnitCode(name), out var guessedUnit) ? guessedUnit.Id : null,
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

        // Helper lambda — applies a unit-aware default when the CSV provided no explicit quantity (qty==1)
        // so that 1g / 1ml of a substantial ingredient is replaced by a more realistic amount.
        ProductComponent Comp(Product ing, decimal qty = 1, bool omittable = false)
        {
            if (qty == 1m && ing.UnitTypeId != null &&
                unitCodeById.TryGetValue(ing.UnitTypeId, out var unitCode))
            {
                qty = unitCode switch
                {
                    "g"  => 100m,
                    "ml" => 100m,
                    _    => 1m,   // pc, portion, etc. — 1 is already realistic
                };
            }
            return new ProductComponent { ComponentId = ing.Id, Quantity = qty, IsOmittable = omittable };
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

            // Description: origin + protein source first, then up to 2 other ingredients
            var originText = allCountries.Count == 1
                ? allCountries[0]
                : $"{string.Join(", ", allCountries[..^1])} and {allCountries[^1]}";

            var validIngredients = primaryRecipe.Ingredients
                .Where(e => ingByTitle.ContainsKey(e.Name))
                .ToList();
            var proteinIngredient = validIngredients.FirstOrDefault(e => IsProteinIngredient(e.Name));
            var otherIngredients  = validIngredients
                .Where(e => !IsProteinIngredient(e.Name))
                .Take(proteinIngredient != null ? 2 : 3)
                .Select(e => e.Name);
            var describedIngredients = (proteinIngredient != null
                    ? new[] { proteinIngredient.Name }.Concat(otherIngredients)
                    : otherIngredients)
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
        // Only assign the most-specific (leaf) facet; parent codes are reachable via the hierarchy.
        if (IngredientsContainAny(ings, "chicken", "hen", "duck", "turkey", "galinha", "poulet"))
            codes.Add("POULTRY");
        if (IngredientsContainAny(ings, "beef", "steak", "brisket", "ground beef", "veal"))
            codes.Add("BEEF");
        if (IngredientsContainAny(ings, "pork", "bacon", "ham", "chorizo", "sausage",
                "lard", "pig", "ribs", "salt pork"))
            codes.Add("PORK");
        if (IngredientsContainAny(ings, "lamb", "mutton"))
            codes.Add("LAMB");
        if (IngredientsContainAny(ings, "salmon", "trout", "mackerel", "herring", "anchovy"))
            codes.Add("FAT_FISH");
        if (IngredientsContainAny(ings, "cod", "tilapia", "haddock", "barramundi", "hilsa",
                "dried fish", "smoked fish", "flying fish"))
            codes.Add("LEAN_FISH");
        if (IngredientsContainAny(ings, "tuna"))
            codes.Add("LEAN_FISH");
        if (IngredientsContainAny(ings, "fish") &&
            !IngredientsContainAny(ings, "fish sauce", "fish stock", "fish oil", "fish broth", "fish paste") &&
            !codes.Contains("FAT_FISH") && !codes.Contains("LEAN_FISH"))
            codes.Add("FISH");
        if (IngredientsContainAny(ings, "crab"))
            codes.Add("CRAB");
        if (IngredientsContainAny(ings, "shrimp", "prawn", "lobster", "crayfish"))
            codes.Add("CRUSTACEANS");
        if (IngredientsContainAny(ings, "mussel", "oyster", "clam", "scallop", "squid", "octopus", "conch"))
            codes.Add("MOLLUSKS");
        if (IngredientsContainAny(ings, "seafood") &&
            !codes.Contains("FISH") && !codes.Contains("FAT_FISH") && !codes.Contains("LEAN_FISH") &&
            !codes.Contains("CRUSTACEANS") && !codes.Contains("CRAB") && !codes.Contains("MOLLUSKS"))
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
    /// Guesses an appropriate unit type code for an auto-created ingredient product based on its name.
    /// Returns "ml" for liquids, "pc" for countable items, and "g" for everything else.
    /// </summary>
    private static string GuessUnitCode(string ingredientName)
    {
        var name = ingredientName.ToLower();

        // Ground/powdered/processed ingredients are always measured by weight
        if (ContainsAny(name, "flakes", "powder", "meal", "starch", "crumb",
                "ground", "paste", "zest"))
            return "g";

        // Liquids → ml
        // Use " oil" (with leading space) to avoid matching "boiled"
        if (name.Contains(" oil") || name.StartsWith("oil") ||
            ContainsAny(name, "milk", "cream", "yogurt", "juice", "stock", "broth",
                "wine", "beer", "vinegar", "sauce", "syrup", "molasses", "kefir",
                "batter", "gravy", "water", "puree"))
            return "ml";

        // Countable items → pc
        // Use specific phrases for "pepper" to avoid matching ground/spice pepper varieties
        if (ContainsAny(name,
                "egg", "yolk",
                "bell pepper", "chili pepper", "hot pepper", "sweet pepper",
                "chili", "jalapeño", "chile",
                "tomato", "onion", "shallot",
                "carrot", "potato", "eggplant", "zucchini", "courgette",
                "avocado", "mango", "banana", "apple", "orange", "lemon", "lime",
                "plum", "peach", "pear", "fig",
                "leaf", "leaves", "sprig", "stalk", "bunch",
                "fillet", "steak", "schnitzel", "cutlet", "breast", "leg", "drumstick", "thigh", "wing",
                "sausage", "bratwurst", "chorizo",
                "tortilla", "flatbread", "dumpling", "wonton", "pastry", "arepa",
                "cucumber", "cob", "plantain",
                "artichoke", "cauliflower", "broccoli", "beet", "beetroot",
                "piece", "pieces"))
            return "pc";

        // Default → weight
        return "g";
    }

    /// <summary>
    /// Guesses category facet codes for an ingredient product based on its name.
    /// Only assigns the most-specific (leaf) facet codes; parent codes are reachable via the hierarchy.
    /// Used when auto-creating ingredient products from CSV data.
    /// </summary>
    private static IEnumerable<string> GuessIngredientCategoryCodes(string ingredientName)
    {
        var name = ingredientName.ToLower();
        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // ── Proteins — leaf codes only ────────────────────────────────────────
        if (ContainsAny(name, "chicken", "duck", "turkey", "hen", "poultry",
                "drumstick", "wing", "thigh", "galinha"))
            codes.Add("POULTRY");
        if (ContainsAny(name, "beef", "steak", "brisket", "veal", "ox", "corned beef"))
            codes.Add("BEEF");
        if (ContainsAny(name, "pork", "bacon", "ham", "sausage", "chorizo", "lard", "ribs", "pig"))
            codes.Add("PORK");
        if (ContainsAny(name, "lamb", "mutton", "sheep"))
            codes.Add("LAMB");

        // ── Seafood — leaf codes only ─────────────────────────────────────────
        if (ContainsAny(name, "salmon", "trout", "mackerel", "herring", "sardine", "anchovy"))
            codes.Add("FAT_FISH");
        if (ContainsAny(name, "cod", "tilapia", "haddock", "sole", "plaice", "snapper",
                "barramundi", "hilsa", "flying fish", "whiting", "tuna"))
            codes.Add("LEAN_FISH");
        // Generic "fish" when no specific variety matched
        if (ContainsAny(name, "smoked fish", "dried fish") ||
            (name.Contains("fish") && !ContainsAny(name, "sauce", "oil", "stock", "broth", "paste") &&
             !codes.Contains("FAT_FISH") && !codes.Contains("LEAN_FISH")))
            codes.Add("FISH");
        if (ContainsAny(name, "crab"))
            codes.Add("CRAB");
        if (ContainsAny(name, "shrimp", "prawn", "lobster", "crawfish", "crayfish"))
            codes.Add("CRUSTACEANS");
        if (ContainsAny(name, "mussel", "oyster", "clam", "scallop", "squid", "octopus", "conch"))
            codes.Add("MOLLUSKS");
        if (ContainsAny(name, "seafood") &&
            !codes.Contains("FISH") && !codes.Contains("FAT_FISH") && !codes.Contains("LEAN_FISH") &&
            !codes.Contains("CRAB") && !codes.Contains("CRUSTACEANS") && !codes.Contains("MOLLUSKS"))
            codes.Add("SEAFOOD");

        // ── Legumes ──────────────────────────────────────────────────────────
        if (ContainsAny(name, "bean", "lentil", "chickpea", "split pea", "legume", "dal", "tofu", "soy"))
            codes.Add("LEGUMES");

        // ── Spices & Herbs ───────────────────────────────────────────────────
        if (ContainsAny(name, "pepper", "chili", "cayenne", "paprika", "cumin", "turmeric",
                "coriander", "cinnamon", "cardamom", "allspice", "nutmeg", "clove", "saffron",
                "spice", "blend", "masala", "berbere", "harissa", "ras el hanout", "sumac", "za'atar",
                "achu spice", "kanwa", "fenugreek", "anise", "caraway"))
            codes.Add("SPICES");
        if (ContainsAny(name, "thyme", "oregano", "rosemary", "basil", "dill", "mint",
                "parsley", "cilantro", "chive", "sage", "tarragon", "bay leaf", "lemongrass",
                "kaffir", "scallion", "leek"))
            codes.Add("HERBS");

        // ── Dairy — leaf codes only ───────────────────────────────────────────
        if (ContainsAny(name, "butter", "ghee"))
            codes.Add("BUTTER");
        if (ContainsAny(name, "yogurt", "yoghurt", "kefir"))
            codes.Add("YOGHURT");
        if (ContainsAny(name, "milk", "cream", "sour cream", "creme fraiche") && !codes.Contains("BUTTER") && !codes.Contains("YOGHURT"))
            codes.Add("MILK");
        if (ContainsAny(name, "cheddar", "parmesan", "parmigiano", "gouda", "gruyere",
                "pecorino", "manchego", "emmental", "comté"))
            codes.Add("HARD_CHEESE");
        if (ContainsAny(name, "brie", "camembert"))
            codes.Add("SOFT_CHEESE");
        if (ContainsAny(name, "ricotta", "mozzarella", "cream cheese", "cottage cheese",
                "mascarpone", "quark"))
            codes.Add("FRESH_CHEESE");
        if (ContainsAny(name, "gorgonzola", "stilton", "roquefort", "blue cheese"))
            codes.Add("BLUE_CHEESE");
        // Generic cheese/dairy when no specific sub-type matched
        if (ContainsAny(name, "feta", "halloumi", "paneer", "curd", "cheese") &&
            !codes.Contains("HARD_CHEESE") && !codes.Contains("SOFT_CHEESE") &&
            !codes.Contains("FRESH_CHEESE") && !codes.Contains("BLUE_CHEESE"))
            codes.Add("CHEESE");
        if (ContainsAny(name, "egg", "dairy") && !codes.Contains("DAIRY") &&
            !codes.Contains("BUTTER") && !codes.Contains("YOGHURT") && !codes.Contains("MILK") &&
            !codes.Contains("CHEESE") && !codes.Contains("HARD_CHEESE") && !codes.Contains("SOFT_CHEESE") &&
            !codes.Contains("FRESH_CHEESE") && !codes.Contains("BLUE_CHEESE"))
            codes.Add("DAIRY");

        // ── Vegetables — leaf codes only ──────────────────────────────────────
        // Root vegetables
        if (ContainsAny(name, "carrot", "potato", "beet", "beetroot", "parsnip", "turnip",
                "radish", "yam", "cassava", "cocoyam", "celeriac"))
        { codes.Add("ROOT_VEG"); codes.Add("RAW_FOOD"); }
        // Onions / alliums (child of STEM_VEG)
        if (ContainsAny(name, "onion", "shallot", "spring onion", "scallion", "leek", "chive", "garlic"))
        { codes.Add("ONIONS"); codes.Add("RAW_FOOD"); }
        // Other stem vegetables when not already an onion
        if (ContainsAny(name, "celery", "asparagus", "fennel stalk", "lemongrass") && !codes.Contains("ONIONS"))
            codes.Add("STEM_VEG");
        // Leafy vegetables
        if (ContainsAny(name, "spinach", "kale", "chard", "arugula", "watercress",
                "collard", "bok choy", "pak choi", "mustard green", "pea shoot"))
        { codes.Add("LEAFY_VEG"); codes.Add("RAW_FOOD"); }
        if (ContainsAny(name, "lettuce", "romaine", "iceberg", "endive", "radicchio"))
        { codes.Add("LETTUCE"); codes.Add("RAW_FOOD"); }  // LETTUCE is child of LEAFY_VEG
        // Flower vegetables
        if (ContainsAny(name, "broccoli", "cauliflower", "cabbage", "brussels sprout", "kohlrabi", "okra"))
        { codes.Add("FLOWER_VEG"); codes.Add("RAW_FOOD"); }
        // Tomatoes (leaf, child of FRUIT_VEG)
        if (ContainsAny(name, "tomato"))
        { codes.Add("TOMATOES"); codes.Add("RAW_FOOD"); }
        // Other fruit-vegetables
        if (ContainsAny(name, "bell pepper", "sweet pepper"))
        { codes.Add("FRUIT_VEG"); codes.Add("RAW_FOOD"); }
        if (ContainsAny(name, "eggplant", "aubergine", "zucchini", "courgette",
                "cucumber", "squash", "pumpkin", "plantain", "avocado"))
            codes.Add("FRUIT_VEG");
        // Fruits (use FRUIT_VEG as no further sub-type)
        if (ContainsAny(name, "mango", "banana", "apple", "orange", "lemon", "lime",
                "plum", "peach", "pear", "fig", "grape", "berry"))
        { codes.Add("FRUIT_VEG"); codes.Add("RAW_FOOD"); }
        // Mushrooms — no hierarchy sub-type, use VEGETABLES directly
        if (ContainsAny(name, "mushroom"))
            codes.Add("VEGETABLES");
        if (ContainsAny(name, "artichoke", "ginger") && !codes.Contains("STEM_VEG") && !codes.Contains("ONIONS"))
            codes.Add("VEGETABLES");
        if (ContainsAny(name, "vegetable", "greens", "bean sprout") &&
            !codes.Contains("ROOT_VEG") && !codes.Contains("LEAFY_VEG") && !codes.Contains("LETTUCE") &&
            !codes.Contains("FLOWER_VEG") && !codes.Contains("FRUIT_VEG") && !codes.Contains("TOMATOES") &&
            !codes.Contains("STEM_VEG") && !codes.Contains("ONIONS"))
            codes.Add("VEGETABLES");

        // ── Grains ───────────────────────────────────────────────────────────
        if (ContainsAny(name, "flour", "rice", "corn", "maize", "wheat", "oat", "barley",
                "breadcrumb", "grain", "cereal", "starch", "polenta", "semolina", "teff",
                "sorghum", "millet", "injera", "couscous", "bulgur", "quinoa"))
            codes.Add("GRAINS");

        // ── Condiments / Pantry ──────────────────────────────────────────────
        if (ContainsAny(name, "oil", "vinegar", "sauce", "stock", "broth", "soy sauce",
                "fish sauce", "tahini", "paste", "ketchup", "mustard", "mayo", "dressing",
                "niter kibbeh", "palm oil", "coconut milk", "peanut butter", "tamarind"))
            codes.Add("CONDIMENTS");
        if (ContainsAny(name, "sugar", "honey", "syrup", "vanilla", "cocoa", "baking", "yeast",
                "water", "salt", "limestone"))
            codes.Add("PANTRY");

        // Fallback
        if (codes.Count == 0)
            codes.Add("PANTRY");

        return codes;
    }
}
