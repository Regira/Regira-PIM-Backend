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
        var facetByTitle = facets.ToDictionary(x => x.Title, x => x);

        // Phase 1: seed ingredient products
        var ingredients = new List<Product>
        {
            // Proteins
            new() { Title = "Beef Patty",      Description = "Seasoned beef patty",           Prices = [new ProductPricePeriod { Price = 2.00m }] },
            new() { Title = "Veggie Patty",    Description = "Plant-based burger patty",      Prices = [new ProductPricePeriod { Price = 2.25m }] },
            new() { Title = "Chicken Breast",  Description = "Grilled chicken breast",        Prices = [new ProductPricePeriod { Price = 2.50m }] },
            new() { Title = "Ground Beef",     Description = "Seasoned ground beef",          Prices = [new ProductPricePeriod { Price = 2.00m }] },
            new() { Title = "Bacon",           Description = "Crispy smoked bacon strips",    Prices = [new ProductPricePeriod { Price = 1.50m }] },
            new() { Title = "Ham",             Description = "Sliced smoked ham",             Prices = [new ProductPricePeriod { Price = 1.25m }] },
            new() { Title = "Turkey",          Description = "Sliced roasted turkey",         Prices = [new ProductPricePeriod { Price = 1.50m }] },
            new() { Title = "Pepperoni",       Description = "Sliced pepperoni",              Prices = [new ProductPricePeriod { Price = 1.00m }] },
            new() { Title = "Egg",             Description = "Free-range egg",                Prices = [new ProductPricePeriod { Price = 0.35m }] },
            // Dairy
            new() { Title = "Cheese",          Description = "Cheddar cheese slice",          Prices = [new ProductPricePeriod { Price = 0.75m }] },
            new() { Title = "Mozzarella",      Description = "Fresh mozzarella cheese",       Prices = [new ProductPricePeriod { Price = 1.20m }] },
            new() { Title = "Parmesan",        Description = "Grated Parmesan cheese",        Prices = [new ProductPricePeriod { Price = 1.00m }] },
            new() { Title = "Feta Cheese",     Description = "Crumbled feta cheese",          Prices = [new ProductPricePeriod { Price = 1.10m }] },
            new() { Title = "Cream",           Description = "Heavy cooking cream",           Prices = [new ProductPricePeriod { Price = 0.60m }] },
            new() { Title = "Butter",          Description = "Salted butter",                 Prices = [new ProductPricePeriod { Price = 0.40m }] },
            new() { Title = "Yogurt",          Description = "Natural Greek yogurt",          Prices = [new ProductPricePeriod { Price = 0.55m }] },
            // Vegetables
            new() { Title = "Lettuce",         Description = "Fresh romaine lettuce",         Prices = [new ProductPricePeriod { Price = 0.25m }] },
            new() { Title = "Tomato",          Description = "Sliced ripe tomato",            Prices = [new ProductPricePeriod { Price = 0.30m }] },
            new() { Title = "Onion",           Description = "Sliced red onion",              Prices = [new ProductPricePeriod { Price = 0.20m }] },
            new() { Title = "Cucumber",        Description = "Sliced fresh cucumber",         Prices = [new ProductPricePeriod { Price = 0.25m }] },
            new() { Title = "Bell Pepper",     Description = "Sliced mixed bell peppers",     Prices = [new ProductPricePeriod { Price = 0.40m }] },
            new() { Title = "Mushrooms",       Description = "Sliced button mushrooms",       Prices = [new ProductPricePeriod { Price = 0.45m }] },
            new() { Title = "Olives",          Description = "Kalamata olives",               Prices = [new ProductPricePeriod { Price = 0.50m }] },
            new() { Title = "Avocado",         Description = "Fresh sliced avocado",          Prices = [new ProductPricePeriod { Price = 1.50m }] },
            new() { Title = "Pickles",         Description = "Dill pickle slices",            Prices = [new ProductPricePeriod { Price = 0.25m }] },
            new() { Title = "Jalapeños",       Description = "Pickled jalapeño slices",       Prices = [new ProductPricePeriod { Price = 0.35m }] },
            // Sauces & condiments
            new() { Title = "Ketchup",         Description = "Tomato ketchup",                Prices = [new ProductPricePeriod { Price = 0.15m }] },
            new() { Title = "Mustard Sauce",   Description = "Yellow mustard",                Prices = [new ProductPricePeriod { Price = 0.15m }] },
            new() { Title = "Mayonnaise",      Description = "Classic mayo",                  Prices = [new ProductPricePeriod { Price = 0.15m }] },
            new() { Title = "Tomato Sauce",    Description = "Classic pizza tomato sauce",     Prices = [new ProductPricePeriod { Price = 0.35m }] },
            new() { Title = "Caesar Dressing", Description = "Creamy Caesar salad dressing",  Prices = [new ProductPricePeriod { Price = 0.45m }] },
            new() { Title = "Sriracha",        Description = "Spicy sriracha sauce",          Prices = [new ProductPricePeriod { Price = 0.20m }] },
            new() { Title = "Chocolate Sauce", Description = "Dark chocolate drizzle",        Prices = [new ProductPricePeriod { Price = 0.40m }] },
            new() { Title = "Caramel Syrup",   Description = "Caramel flavored syrup",        Prices = [new ProductPricePeriod { Price = 0.45m }] },
            new() { Title = "Maple Syrup",     Description = "Pure maple syrup",              Prices = [new ProductPricePeriod { Price = 0.50m }] },
            // Bases & grains
            new() { Title = "Bun",             Description = "Brioche burger bun",            Prices = [new ProductPricePeriod { Price = 0.50m }] },
            new() { Title = "Pizza Dough",     Description = "Stone-baked pizza base",        Prices = [new ProductPricePeriod { Price = 1.00m }] },
            new() { Title = "Tortilla",        Description = "Flour tortilla wrap",           Prices = [new ProductPricePeriod { Price = 0.40m }] },
            new() { Title = "Spaghetti",       Description = "Italian durum wheat spaghetti", Prices = [new ProductPricePeriod { Price = 0.55m }] },
            new() { Title = "Croutons",        Description = "Garlic herb croutons",          Prices = [new ProductPricePeriod { Price = 0.30m }] },
            // Beverages & extras
            new() { Title = "Espresso",        Description = "Single espresso shot",          Prices = [new ProductPricePeriod { Price = 0.80m }] },
            new() { Title = "Extra Shot",      Description = "Additional espresso shot",      Prices = [new ProductPricePeriod { Price = 0.80m }] },
            new() { Title = "Tea Leaves",      Description = "Premium loose-leaf tea",        Prices = [new ProductPricePeriod { Price = 0.60m }] },
            new() { Title = "Oat Milk",        Description = "Barista oat milk",              Prices = [new ProductPricePeriod { Price = 0.60m }] },
            new() { Title = "Soy Milk",        Description = "Organic soy milk",              Prices = [new ProductPricePeriod { Price = 0.50m }] },
            new() { Title = "Mixed Fruit",     Description = "Seasonal fresh fruit blend",    Prices = [new ProductPricePeriod { Price = 1.20m }] },
            new() { Title = "Whipped Cream",   Description = "Fresh whipped cream topping",   Prices = [new ProductPricePeriod { Price = 0.50m }] },
        };

        logger.LogInformation("Seeding products...");
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

        // Lookup helpers
        var byCode = unitTypes.ToDictionary(u => u.Code!, u => u);
        var ing = ingredients.ToDictionary(i => i.Title, i => i);

        // Unit type assignment per ingredient
        var ingredientUnitTypes = new Dictionary<string, string>
        {
            // Proteins
            ["Beef Patty"] = "pc",
            ["Veggie Patty"] = "pc",
            ["Chicken Breast"] = "g",
            ["Ground Beef"] = "g",
            ["Bacon"] = "g",
            ["Ham"] = "g",
            ["Turkey"] = "g",
            ["Pepperoni"] = "g",
            ["Egg"] = "pc",
            // Dairy
            ["Cheese"] = "g",
            ["Mozzarella"] = "g",
            ["Parmesan"] = "g",
            ["Feta Cheese"] = "g",
            ["Cream"] = "ml",
            ["Butter"] = "g",
            ["Yogurt"] = "ml",
            // Vegetables
            ["Lettuce"] = "g",
            ["Tomato"] = "pc",
            ["Onion"] = "pc",
            ["Cucumber"] = "pc",
            ["Bell Pepper"] = "pc",
            ["Mushrooms"] = "g",
            ["Olives"] = "g",
            ["Avocado"] = "pc",
            ["Pickles"] = "g",
            ["Jalapeños"] = "g",
            // Sauces
            ["Ketchup"] = "ml",
            ["Mustard Sauce"] = "ml",
            ["Mayonnaise"] = "ml",
            ["Tomato Sauce"] = "ml",
            ["Caesar Dressing"] = "ml",
            ["Sriracha"] = "ml",
            ["Chocolate Sauce"] = "ml",
            ["Caramel Syrup"] = "ml",
            ["Maple Syrup"] = "ml",
            // Bases
            ["Bun"] = "pc",
            ["Pizza Dough"] = "pc",
            ["Tortilla"] = "pc",
            ["Spaghetti"] = "g",
            ["Croutons"] = "g",
            // Beverages
            ["Espresso"] = "ml",
            ["Extra Shot"] = "ml",
            ["Tea Leaves"] = "g",
            ["Oat Milk"] = "ml",
            ["Soy Milk"] = "ml",
            ["Mixed Fruit"] = "g",
            ["Whipped Cream"] = "ml",
        };

        foreach (var ingredient in ingredients)
        {
            if (ingredientUnitTypes.TryGetValue(ingredient.Title, out var code))
                ingredient.UnitTypeId = byCode[code].Id;
            await productService.Save(ingredient);
        }
        await productService.SaveChanges();

        ProductComponent Comp(string name, decimal qty = 1, bool omittable = false)
            => new() { ComponentId = ing[name].Id, Quantity = qty, IsOmittable = omittable };
        ProductAllowedComponentAddition Extra(string name)
            => new() { ComponentId = ing[name].Id };
        ProductFacet Tag(string name)
            => new() { FacetId = facetByTitle[name].Id };
        int UnitId(string code)
            => byCode[code].Id;

        // Phase 2: menu items derived from leaf-level facets
        var menuItems = new List<Product>
        {
            // --- Burgers ---
            new()
            {
                Title = "Classic Burger",
                Description = "Traditional beef burger with lettuce and tomato",
                Prices = [new ProductPricePeriod { Price = 8.99m }],
                AllowAdditions = true,
                UnitTypeId = UnitId("portion"),
                Facets = [Tag("Classic Burger"), Tag("Burgers")],
                Components =
                [
                    Comp("Bun"), Comp("Beef Patty"),
                    Comp("Lettuce"), Comp("Tomato"),
                    Comp("Onion", omittable: true), Comp("Pickles", omittable: true),
                    Comp("Ketchup", omittable: true), Comp("Mustard Sauce", omittable: true)
                ],
                AllowedComponentAdditions = [Extra("Cheese"), Extra("Bacon"), Extra("Avocado"), Extra("Jalapeños"), Extra("Mayonnaise")]
            },
            new()
            {
                Title = "Cheeseburger",
                Description = "Beef burger with melted cheese",
                Prices = [new ProductPricePeriod { Price = 9.49m }],
                AllowAdditions = true,
                UnitTypeId = UnitId("portion"),
                Facets = [Tag("Cheeseburger"), Tag("Burgers")],
                Components =
                [
                    Comp("Bun"), Comp("Beef Patty"), Comp("Cheese"),
                    Comp("Lettuce"), Comp("Tomato"),
                    Comp("Pickles", omittable: true), Comp("Ketchup", omittable: true)
                ],
                AllowedComponentAdditions = [Extra("Bacon"), Extra("Avocado"), Extra("Jalapeños"), Extra("Onion"), Extra("Mayonnaise")]
            },
            new()
            {
                Title = "Veggie Burger",
                Description = "Plant-based burger patty with fresh toppings",
                Prices = [new ProductPricePeriod { Price = 9.99m }],
                AllowAdditions = true,
                UnitTypeId = UnitId("portion"),
                Facets = [Tag("Veggie Burger"), Tag("Burgers"), Tag("Vegan"), Tag("Vegetarian")],
                Components =
                [
                    Comp("Bun"), Comp("Veggie Patty"),
                    Comp("Lettuce"), Comp("Tomato"), Comp("Avocado"),
                    Comp("Onion", omittable: true), Comp("Mustard Sauce", omittable: true)
                ],
                AllowedComponentAdditions = [Extra("Cheese"), Extra("Jalapeños"), Extra("Mayonnaise"), Extra("Ketchup")]
            },
            // --- Pizza ---
            new()
            {
                Title = "Margherita",
                Description = "Classic tomato and mozzarella pizza",
                Prices = [new ProductPricePeriod { Price = 11.99m }],
                AllowAdditions = true,
                UnitTypeId = UnitId("slice"),
                Facets = [Tag("Margherita"), Tag("Pizza"), Tag("Vegetarian")],
                Components =
                [
                    Comp("Pizza Dough"), Comp("Tomato Sauce"), Comp("Mozzarella")
                ],
                AllowedComponentAdditions = [Extra("Mushrooms"), Extra("Bell Pepper"), Extra("Olives"), Extra("Jalapeños"), Extra("Onion")]
            },
            new()
            {
                Title = "Pepperoni Pizza",
                Description = "Pizza topped with spicy pepperoni",
                Prices = [new ProductPricePeriod { Price = 13.49m }],
                AllowAdditions = true,
                UnitTypeId = UnitId("slice"),
                Facets = [Tag("Pepperoni Pizza"), Tag("Pizza")],
                Components =
                [
                    Comp("Pizza Dough"), Comp("Tomato Sauce"),
                    Comp("Mozzarella"), Comp("Pepperoni")
                ],
                AllowedComponentAdditions = [Extra("Mushrooms"), Extra("Bell Pepper"), Extra("Olives"), Extra("Jalapeños"), Extra("Onion")]
            },
            // --- Salads ---
            new()
            {
                Title = "Caesar Salad",
                Description = "Romaine lettuce with Caesar dressing and croutons",
                Prices = [new ProductPricePeriod { Price = 8.49m }],
                AllowAdditions = true,
                UnitTypeId = UnitId("portion"),
                Facets = [Tag("Caesar Salad"), Tag("Salads")],
                Components =
                [
                    Comp("Lettuce", qty: 2), Comp("Parmesan"),
                    Comp("Caesar Dressing"), Comp("Croutons", omittable: true),
                    Comp("Chicken Breast", omittable: true)
                ],
                AllowedComponentAdditions = [Extra("Avocado"), Extra("Bacon"), Extra("Tomato")]
            },
            new()
            {
                Title = "Greek Salad",
                Description = "Tomato, cucumber, olives and feta cheese",
                Prices = [new ProductPricePeriod { Price = 7.99m }],
                AllowAdditions = true,
                UnitTypeId = UnitId("portion"),
                Facets = [Tag("Greek Salad"), Tag("Salads"), Tag("Vegetarian")],
                Components =
                [
                    Comp("Tomato"), Comp("Cucumber"), Comp("Olives"),
                    Comp("Feta Cheese"), Comp("Onion", omittable: true),
                    Comp("Bell Pepper", omittable: true)
                ],
                AllowedComponentAdditions = [Extra("Avocado"), Extra("Chicken Breast")]
            },
            // --- Pasta ---
            new()
            {
                Title = "Carbonara",
                Description = "Creamy egg and bacon pasta",
                Prices = [new ProductPricePeriod { Price = 12.49m }],
                AllowAdditions = true,
                UnitTypeId = UnitId("portion"),
                Facets = [Tag("Carbonara"), Tag("Pasta")],
                Components =
                [
                    Comp("Spaghetti"), Comp("Egg", qty: 2),
                    Comp("Bacon"), Comp("Cream"), Comp("Parmesan")
                ],
                AllowedComponentAdditions = [Extra("Mushrooms"), Extra("Chicken Breast")]
            },
            new()
            {
                Title = "Bolognese",
                Description = "Rich meat sauce pasta",
                Prices = [new ProductPricePeriod { Price = 11.99m }],
                AllowAdditions = true,
                UnitTypeId = UnitId("portion"),
                Facets = [Tag("Bolognese"), Tag("Pasta")],
                Components =
                [
                    Comp("Spaghetti"), Comp("Ground Beef"),
                    Comp("Tomato Sauce"), Comp("Onion"),
                    Comp("Mushrooms", omittable: true)
                ],
                AllowedComponentAdditions = [Extra("Parmesan"), Extra("Bell Pepper"), Extra("Jalapeños")]
            },
            // --- Desserts ---
            new()
            {
                Title = "Ice Cream",
                Description = "Assorted ice cream flavors",
                Prices = [new ProductPricePeriod { Price = 4.99m }],
                AllowAdditions = true,
                UnitTypeId = UnitId("pc"),
                Facets = [Tag("Ice Cream"), Tag("Desserts")],
                Components = [Comp("Whipped Cream", omittable: true)],
                AllowedComponentAdditions = [Extra("Chocolate Sauce"), Extra("Caramel Syrup")]
            },
            new()
            {
                Title = "Cake",
                Description = "Freshly baked cakes and slices",
                Prices = [new ProductPricePeriod { Price = 5.49m }],
                AllowAdditions = true,
                UnitTypeId = UnitId("slice"),
                Facets = [Tag("Cake"), Tag("Desserts")],
                Components =
                [
                    Comp("Whipped Cream", omittable: true),
                    Comp("Chocolate Sauce", omittable: true)
                ],
                AllowedComponentAdditions = [Extra("Caramel Syrup")]
            },
            // --- Hot Drinks ---
            new()
            {
                Title = "Coffee",
                Description = "Espresso-based beverages",
                Prices = [new ProductPricePeriod { Price = 3.49m }],
                AllowAdditions = true,
                UnitTypeId = UnitId("cup"),
                Facets = [Tag("Coffee"), Tag("Hot Drinks")],
                Components = [Comp("Espresso")],
                AllowedComponentAdditions = [Extra("Extra Shot"), Extra("Oat Milk"), Extra("Soy Milk"), Extra("Whipped Cream"), Extra("Caramel Syrup")]
            },
            new()
            {
                Title = "Tea",
                Description = "Hot and iced tea varieties",
                Prices = [new ProductPricePeriod { Price = 2.99m }],
                AllowAdditions = true,
                UnitTypeId = UnitId("cup"),
                Facets = [Tag("Tea"), Tag("Hot Drinks")],
                Components = [Comp("Tea Leaves")],
                AllowedComponentAdditions = [Extra("Oat Milk"), Extra("Soy Milk")]
            },
            // --- Cold Drinks ---
            new()
            {
                Title = "Fresh Juice",
                Description = "Freshly squeezed fruit juices",
                Prices = [new ProductPricePeriod { Price = 4.49m }],
                AllowAdditions = false,
                UnitTypeId = UnitId("ml"),
                Facets = [Tag("Fresh Juice"), Tag("Cold Drinks"), Tag("Vegan")],
                Components = [Comp("Mixed Fruit", qty: 3)]
            },
            new()
            {
                Title = "Smoothie",
                Description = "Blended fruit and yogurt smoothies",
                Prices = [new ProductPricePeriod { Price = 5.49m }],
                AllowAdditions = true,
                UnitTypeId = UnitId("ml"),
                Facets = [Tag("Smoothies"), Tag("Cold Drinks"), Tag("Vegetarian")],
                Components = [Comp("Mixed Fruit", qty: 2), Comp("Yogurt")],
                AllowedComponentAdditions = [Extra("Whipped Cream"), Extra("Chocolate Sauce")]
            },
            // --- Breakfast ---
            new()
            {
                Title = "Pancakes",
                Description = "Fluffy breakfast pancakes",
                Prices = [new ProductPricePeriod { Price = 7.49m }],
                AllowAdditions = true,
                UnitTypeId = UnitId("pc"),
                Facets = [Tag("Pancakes"), Tag("Breakfast"), Tag("Vegetarian")],
                Components =
                [
                    Comp("Egg", qty: 2), Comp("Butter"), Comp("Maple Syrup")
                ],
                AllowedComponentAdditions = [Extra("Whipped Cream"), Extra("Chocolate Sauce"), Extra("Caramel Syrup"), Extra("Mixed Fruit")]
            },
            new()
            {
                Title = "Croissant",
                Description = "Buttery French pastry",
                Prices = [new ProductPricePeriod { Price = 3.49m }],
                AllowAdditions = true,
                UnitTypeId = UnitId("pc"),
                Facets = [Tag("Croissant"), Tag("Breakfast"), Tag("Vegetarian")],
                Components = [Comp("Butter")],
                AllowedComponentAdditions = [Extra("Cheese"), Extra("Ham")]
            },
        };

        foreach (var item in menuItems)
        {
            if (f.Random.Bool(0.5f))
            {
                item.Suppliers = f.PickRandom(suppliers, f.Random.Int(1, Math.Min(2, suppliers.Count)))
                    .DistinctBy(s => s.Id)
                    .Select(s => new ProductSupplier { SupplierId = s.Id })
                    .ToList();
            }
            await productService.Save(item);
        }
        await productService.SaveChanges();

        // Phase 3: random product variants for volume
        string[] adjectives = ["Spicy", "Smoky", "Crispy", "Golden", "Grilled", "Fresh", "Loaded", "Signature", "Double", "BBQ"];
        string[] nouns = ["Burger", "Sandwich", "Wrap", "Bowl", "Salad", "Pizza", "Pasta", "Toast", "Roll", "Plate"];

        var servingUnitTypes = unitTypes.Where(u => u.Code is "pc" or "portion" or "slice" or "cup" or "bowl" or "plate").ToList();

        decimal ComponentQty(Product component) =>
            ingredientUnitTypes.TryGetValue(component.Title, out var uCode) ? uCode switch
            {
                "pc" => f.Random.Int(1, 3),
                "g" => f.PickRandom(25m, 50m, 75m, 100m, 150m),
                "ml" => f.PickRandom(15m, 30m, 50m, 100m, 150m),
                "kg" => f.PickRandom(0.25m, 0.5m, 1m),
                "l" => f.PickRandom(0.25m, 0.5m, 1m),
                _ => 1m
            } : 1m;

        var allProducts = new List<Product>(menuItems);

        for (int i = 0; i < 250; i++)
        {
            var price = Math.Round(f.Random.Decimal(3.50m, 24.99m), 2);

            var selectedIngredients = f.PickRandom(ingredients, f.Random.Int(2, Math.Min(6, ingredients.Count)))
                .DistinctBy(c => c.Id)
                .ToList();

            var product = new Product
            {
                Title = $"{f.PickRandom(adjectives)} {f.PickRandom(nouns)} #{i + 1}",
                Description = f.Lorem.Sentence(),
                Prices = [new ProductPricePeriod { Price = price }],
                UnitTypeId = f.PickRandom(servingUnitTypes).Id,
                Facets = f.PickRandom(facets, f.Random.Int(1, 3))
                    .DistinctBy(c => c.Id)
                    .Select(c => new ProductFacet { FacetId = c.Id })
                    .ToList(),
                Components = selectedIngredients
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

            allProducts.Add(product);
            await productService.Save(product);
            if ((i + 1) % BatchSize == 0)
                await productService.SaveChanges();
        }
        await productService.SaveChanges();

        return allProducts;
    }
}
