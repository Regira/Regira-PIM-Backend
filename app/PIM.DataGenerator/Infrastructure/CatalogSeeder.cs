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

    // Canonical ingredient list derived from the most common CSV ingredients.
    // Title → (Description, UnitCode, Price)
    private static readonly IReadOnlyDictionary<string, (string Description, string UnitCode, decimal Price)> CanonicalIngredients =
        new Dictionary<string, (string, string, decimal)>(StringComparer.OrdinalIgnoreCase)
        {
            // Basics
            ["Salt"]           = ("Fine sea salt",              "g",  0.05m),
            ["Sugar"]          = ("White granulated sugar",     "g",  0.10m),
            ["Pepper"]         = ("Ground black pepper",        "g",  0.05m),
            ["Black Pepper"]   = ("Ground black pepper",        "g",  0.05m),
            ["Water"]          = ("Clean water",                "ml", 0.01m),
            // Fats & oils
            ["Oil"]            = ("Cooking oil",                "ml", 0.10m),
            ["Olive Oil"]      = ("Extra-virgin olive oil",     "ml", 0.25m),
            ["Vegetable Oil"]  = ("Refined vegetable oil",      "ml", 0.10m),
            ["Sesame Oil"]     = ("Toasted sesame oil",         "ml", 0.30m),
            ["Butter"]         = ("Salted butter",              "g",  0.40m),
            ["Ghee"]           = ("Clarified butter (ghee)",    "g",  0.45m),
            // Aromatics
            ["Onion"]          = ("Yellow onion",               "pc", 0.20m),
            ["Garlic"]         = ("Fresh garlic cloves",        "g",  0.15m),
            ["Ginger"]         = ("Fresh ginger root",          "g",  0.20m),
            ["Scallions"]      = ("Spring onions / scallions",  "g",  0.15m),
            ["Lemongrass"]     = ("Fresh lemongrass stalk",     "pc", 0.30m),
            // Spices
            ["Cumin"]          = ("Ground cumin",               "g",  0.15m),
            ["Turmeric"]       = ("Ground turmeric",            "g",  0.15m),
            ["Cinnamon"]       = ("Ground cinnamon",            "g",  0.15m),
            ["Paprika"]        = ("Sweet paprika",              "g",  0.15m),
            ["Allspice"]       = ("Ground allspice",            "g",  0.15m),
            ["Cardamom"]       = ("Ground cardamom",            "g",  0.20m),
            ["Cloves"]         = ("Whole cloves",               "g",  0.20m),
            ["Saffron"]        = ("Saffron threads",            "g",  1.50m),
            ["Oregano"]        = ("Dried oregano",              "g",  0.10m),
            ["Thyme"]          = ("Fresh thyme",                "g",  0.10m),
            ["Rosemary"]       = ("Fresh rosemary",             "g",  0.10m),
            ["Bay Leaf"]       = ("Dried bay leaf",             "pc", 0.05m),
            ["Nutmeg"]         = ("Ground nutmeg",              "g",  0.20m),
            ["Fennel"]         = ("Fennel seeds",               "g",  0.15m),
            ["Garam Masala"]   = ("Garam masala spice blend",   "g",  0.20m),
            ["Curry Powder"]   = ("Mild curry powder",          "g",  0.15m),
            ["Coriander Powder"] = ("Ground coriander",         "g",  0.15m),
            ["Chili Powder"]   = ("Chili powder blend",         "g",  0.15m),
            ["Chili Flakes"]   = ("Dried red chili flakes",     "g",  0.15m),
            // Herbs
            ["Parsley"]        = ("Fresh flat-leaf parsley",    "g",  0.10m),
            ["Cilantro"]       = ("Fresh cilantro (coriander)", "g",  0.10m),
            ["Basil"]          = ("Fresh sweet basil",          "g",  0.10m),
            ["Dill"]           = ("Fresh dill",                 "g",  0.10m),
            ["Mint"]           = ("Fresh mint leaves",          "g",  0.10m),
            // Grains & starchy
            ["Flour"]          = ("All-purpose wheat flour",    "g",  0.05m),
            ["Rice"]           = ("Long-grain white rice",      "g",  0.08m),
            ["Basmati Rice"]   = ("Fragrant basmati rice",      "g",  0.12m),
            ["Cornmeal"]       = ("Yellow cornmeal",            "g",  0.08m),
            ["Pasta"]          = ("Dried pasta",                "g",  0.10m),
            ["Breadcrumbs"]    = ("Dry breadcrumbs",            "g",  0.08m),
            ["Cassava"]        = ("Fresh cassava root",         "g",  0.10m),
            ["Phyllo Dough"]   = ("Frozen phyllo pastry sheets","g",  0.20m),
            // Vegetables
            ["Potatoes"]       = ("White potatoes",             "g",  0.08m),
            ["Sweet Potato"]   = ("Orange sweet potato",        "g",  0.10m),
            ["Tomatoes"]       = ("Ripe tomatoes",              "pc", 0.30m),
            ["Tomato"]         = ("Ripe tomato",                "pc", 0.30m),
            ["Tomato Paste"]   = ("Concentrated tomato paste",  "g",  0.20m),
            ["Bell Pepper"]    = ("Mixed bell peppers",         "pc", 0.40m),
            ["Chili Pepper"]   = ("Fresh red chili pepper",     "pc", 0.20m),
            ["Scotch Bonnet"]  = ("Scotch bonnet pepper",       "pc", 0.25m),
            ["Carrots"]        = ("Fresh carrots",              "g",  0.08m),
            ["Cabbage"]        = ("White cabbage",              "g",  0.05m),
            ["Eggplant"]       = ("Aubergine / eggplant",       "g",  0.15m),
            ["Zucchini"]       = ("Courgette / zucchini",       "g",  0.12m),
            ["Mushrooms"]      = ("Button mushrooms",           "g",  0.45m),
            ["Spinach"]        = ("Fresh spinach leaves",       "g",  0.20m),
            ["Okra"]           = ("Fresh okra pods",            "g",  0.20m),
            ["Celery"]         = ("Celery stalk",               "pc", 0.15m),
            ["Plantain"]       = ("Green plantain",             "pc", 0.30m),
            // Legumes
            ["Chickpeas"]      = ("Canned chickpeas",           "g",  0.15m),
            ["Beans"]          = ("Dried beans",                "g",  0.12m),
            ["Peas"]           = ("Frozen green peas",          "g",  0.10m),
            ["Lentils"]        = ("Red lentils",                "g",  0.12m),
            // Fruits
            ["Lemon"]          = ("Fresh lemon",                "pc", 0.25m),
            ["Lemon Juice"]    = ("Freshly squeezed lemon juice","ml",0.20m),
            ["Lime Juice"]     = ("Freshly squeezed lime juice", "ml",0.20m),
            ["Raisins"]        = ("Seedless raisins",           "g",  0.20m),
            ["Mango"]          = ("Ripe mango",                 "pc", 0.60m),
            ["Banana"]         = ("Ripe banana",                "pc", 0.20m),
            ["Coconut"]        = ("Desiccated coconut",         "g",  0.25m),
            ["Tamarind"]       = ("Tamarind paste",             "g",  0.25m),
            // Dairy & eggs
            ["Eggs"]           = ("Free-range eggs",            "pc", 0.35m),
            ["Egg"]            = ("Free-range egg",             "pc", 0.35m),
            ["Milk"]           = ("Whole milk",                 "ml", 0.10m),
            ["Cream"]          = ("Heavy cooking cream",        "ml", 0.40m),
            ["Sour Cream"]     = ("Full-fat sour cream",        "ml", 0.30m),
            ["Yogurt"]         = ("Natural yogurt",             "ml", 0.30m),
            ["Cheese"]         = ("Cheddar cheese",             "g",  0.45m),
            ["Mozzarella"]     = ("Fresh mozzarella",           "g",  0.55m),
            ["Feta Cheese"]    = ("Crumbled feta cheese",       "g",  0.55m),
            ["Coconut Milk"]   = ("Full-fat coconut milk",      "ml", 0.25m),
            // Proteins
            ["Chicken"]        = ("Bone-in chicken pieces",     "g",  0.50m),
            ["Beef"]           = ("Beef chuck",                 "g",  0.60m),
            ["Ground Beef"]    = ("Lean ground beef",           "g",  0.55m),
            ["Lamb"]           = ("Diced lamb shoulder",        "g",  0.70m),
            ["Pork"]           = ("Pork shoulder",              "g",  0.55m),
            ["Bacon"]          = ("Smoked bacon",               "g",  0.60m),
            ["Fish"]           = ("White fish fillet",          "g",  0.65m),
            ["Shrimp"]         = ("Raw peeled shrimp",          "g",  0.80m),
            // Sauces & condiments
            ["Soy Sauce"]      = ("Light soy sauce",            "ml", 0.15m),
            ["Fish Sauce"]     = ("Thai fish sauce",            "ml", 0.20m),
            ["Vinegar"]        = ("White wine vinegar",         "ml", 0.10m),
            ["Mustard"]        = ("Dijon mustard",              "g",  0.20m),
            ["Peanut Butter"]  = ("Smooth peanut butter",       "g",  0.35m),
            ["Beef Stock"]     = ("Rich beef stock",            "ml", 0.15m),
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

        var ingByTitle = ingredients.ToDictionary(i => i.Title, i => i, StringComparer.OrdinalIgnoreCase);

        // Helper lambdas
        ProductComponent Comp(Product ing, decimal qty = 1, bool omittable = false)
            => new() { ComponentId = ing.Id, Quantity = qty, IsOmittable = omittable };

        ProductFacet Tag(string name)
            => new() { FacetId = facetByTitle[name].Id };

        // Phase 2: dish products from CSV recipes
        var servingUnitTypes = unitTypes
            .Where(u => u.Code is "pc" or "portion" or "plate" or "bowl")
            .ToList();

        var dishProducts = new List<Product>();

        var unmappedIngredients = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var recipe in recipes)
        {
            // Map recipe ingredients to canonical ingredient products (best-effort)
            var components = recipe.Ingredients
                .Select(name =>
                {
                    if (ingByTitle.TryGetValue(name, out var ing))
                        return Comp(ing, qty: 1, omittable: f.Random.Bool(0.3f));
                    unmappedIngredients.Add(name);
                    return (ProductComponent?)null;
                })
                .Where(c => c != null)
                .Cast<ProductComponent>()
                .DistinctBy(c => c.ComponentId)
                .ToList();

            var tags = new List<ProductFacet>();
            if (facetByTitle.TryGetValue(recipe.Dish, out var dishFacet))
                tags.Add(new ProductFacet { FacetId = dishFacet.Id });
            if (facetByTitle.TryGetValue(recipe.Country, out var countryFacet))
                tags.Add(new ProductFacet { FacetId = countryFacet.Id });

            // Build a short ingredient preview for the description
            var topIngredients = recipe.Ingredients.Take(3).ToList();
            var ingredientPreview = topIngredients.Any()
                ? string.Join(", ", topIngredients)
                : string.Empty;
            var description = string.IsNullOrEmpty(ingredientPreview)
                ? $"A traditional dish from {recipe.Country}"
                : $"A traditional dish from {recipe.Country} with {ingredientPreview}";

            var product = new Product
            {
                Title       = recipe.Dish,
                Description = description,
                Prices      = [new ProductPricePeriod { Price = Math.Round(f.Random.Decimal(6.99m, 24.99m), 2) }],
                UnitTypeId  = f.PickRandom(servingUnitTypes).Id,
                AllowAdditions = f.Random.Bool(0.7f),
                Facets      = tags,
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

        if (unmappedIngredients.Count > 0)
            logger.LogDebug("{Count} ingredient name(s) from CSV did not match the canonical list and were skipped: {Names}",
                unmappedIngredients.Count, string.Join(", ", unmappedIngredients.OrderBy(x => x)));

        logger.LogInformation("Seeding {Count} dish products...", dishProducts.Count);
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

            var product = new Product
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

            dishProducts.Add(product);
            await productService.Save(product);
            if ((i + 1) % BatchSize == 0)
                await productService.SaveChanges();
        }
        await productService.SaveChanges();

        return dishProducts;
    }
}
