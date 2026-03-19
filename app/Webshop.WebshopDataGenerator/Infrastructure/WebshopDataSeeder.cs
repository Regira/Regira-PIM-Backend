using Bogus;
using Regira.Entities.Services.Abstractions;
using Webshop.Models.Catalog.Articles;
using Webshop.Models.Orders;
using Webshop.Models.Stakeholders.ContactData;
using Webshop.Models.Stakeholders.Parties;
using Webshop.Models.Stakeholders.Parties.Relations;
using Webshop.Models.Taxonomy.Facets;
using Webshop.Models.Taxonomy.FacetGroups;
using Person = Webshop.Models.Stakeholders.Parties.Person;

namespace Webshop.WebshopDataGenerator.Infrastructure;

public class WebshopDataSeeder(IEntityService<Facet> facetService, IEntityService<FacetGroup> facetGroupService, IEntityService<Article> articleService,
    IEntityService<Party> partyService, IEntityService<Order> orderService, IEntityService<RelationshipType> relationshipTypeService)
{
    private const int BatchSize = 100;

    public async Task SeedAsync()
    {
        var parties = await SeedParties();
        var relationshipTypes = await SeedRelationshipTypes();
        await SeedRelationships(parties, relationshipTypes);
        var facets = await SeedFacets();
        await SeedFacetGroups(facets);
        var suppliers = parties.OfType<Organization>().ToList();
        var articles = await SeedArticles(facets, suppliers);

        await SeedOrders(parties, articles);
    }


    public async Task<IList<Party>> SeedParties()
    {
        var parties = new List<Party>(1000);

        for (int i = 0; i < 1000; i++)
        {
            var f = new Faker("en");

            Party party = f.Random.Bool(0.7f)
                ? new Person { GivenName = f.Name.FirstName(), FamilyName = f.Name.LastName() }
                : new Organization { Name = f.Company.CompanyName() };

            var addresses = new List<PartyAddress>
            {
                new()
                {
                    Title = party is Organization ? "Office" : "Home",
                    Street = f.Address.StreetName(),
                    HouseNumber = f.Random.Int(1, 200).ToString(),
                    PostalCode = f.Address.ZipCode(),
                    City = f.Address.City(),
                    CountryCode = "BE",
                    SortOrder = 0
                }
            };
            if (f.Random.Bool(0.4f))
                addresses.Add(new()
                {
                    Title = party is Organization ? "Billing" : "Work",
                    Street = f.Address.StreetName(),
                    HouseNumber = f.Random.Int(1, 200).ToString(),
                    PostalCode = f.Address.ZipCode(),
                    City = f.Address.City(),
                    CountryCode = "BE",
                    SortOrder = 1
                });
            party.Addresses = addresses;

            var contactData = new List<PartyContactDetails>
            {
                new() { Title = "Email",  Value = f.Internet.Email(party.Title),  DataType = ContactDataTypes.Email, SortOrder = 0 },
                new() { Title = "Phone",  Value = f.Phone.PhoneNumber(),           DataType = ContactDataTypes.Phone, SortOrder = 1 },
            };
            if (party is Organization && f.Random.Bool(0.8f))
                contactData.Add(new() { Title = "Website", Value = f.Internet.Url(), DataType = ContactDataTypes.Website, SortOrder = contactData.Count });
            if (f.Random.Bool(0.5f))
                contactData.Add(new() { Title = "Mobile", Value = f.Phone.PhoneNumber(), DataType = ContactDataTypes.Phone, SortOrder = contactData.Count });
            party.ContactData = contactData;

            parties.Add(party);
            await partyService.Save(party);
            if ((i + 1) % BatchSize == 0)
                await partyService.SaveChanges();
        }
        await partyService.SaveChanges();

        return parties;
    }

    public async Task<IList<RelationshipType>> SeedRelationshipTypes()
    {
        var types = new List<RelationshipType>
        {
            new() { Code = "EMP",  Title = "Employee",       Description = "Person is employed by an organization" },
            new() { Code = "CUST", Title = "Customer",       Description = "Person is a customer of the organization" },
            new() { Code = "SUP",  Title = "Supplier",       Description = "Organization is a supplier" },
            new() { Code = "MBR",  Title = "Member",         Description = "Person is a member of the organization" },
            new() { Code = "REP",  Title = "Representative", Description = "Person represents the organization" },
            new() { Code = "CON",  Title = "Contact",        Description = "Person is a contact for the organization" },
        };

        foreach (var type in types)
            await relationshipTypeService.Save(type);
        await relationshipTypeService.SaveChanges();

        return types;
    }

    public async Task SeedRelationships(IList<Party> parties, IList<RelationshipType> relationshipTypes)
    {
        var f = new Faker("en");
        var organizations = parties.OfType<Organization>().ToList();
        var persons = parties.OfType<Person>().ToList();

        if (organizations.Count == 0 || persons.Count == 0)
            return;

        int count = 0;
        foreach (var org in organizations)
        {
            if (!f.Random.Bool(0.7f))
                continue;

            var loaded = await partyService.Details(org.Id);
            if (loaded == null)
                continue;

            var relType = f.PickRandom(relationshipTypes);
            loaded.ChildRelationships = f.PickRandom(persons, f.Random.Int(2, Math.Min(8, persons.Count)))
                .DistinctBy(p => p.Id)
                .Select(p =>
                {
                    var contactData = new List<PartyRelationshipContactDetails>
                    {
                        new() { Title = "Work Email", Value = f.Internet.Email(p.Title), DataType = ContactDataTypes.Email, SortOrder = 0 },
                    };
                    if (f.Random.Bool(0.6f))
                        contactData.Add(new() { Title = "Work Phone", Value = f.Phone.PhoneNumber(), DataType = ContactDataTypes.Phone, SortOrder = 1 });
                    return new PartyRelationship { ChildId = p.Id, RelationshipTypeId = relType.Id, ContactData = contactData };
                })
                .ToList();

            await partyService.Save(loaded);
            count++;
            if (count % BatchSize == 0)
                await partyService.SaveChanges();
        }
        await partyService.SaveChanges();
    }

    public async Task<IList<Facet>> SeedFacets()
    {
        // Root categories
        var food       = new Facet { Title = "Food",       Description = "All food items" };
        var beverages  = new Facet { Title = "Beverages",  Description = "All drinks and beverages" };
        var dietary    = new Facet { Title = "Dietary",    Description = "Dietary preference categories" };

        // Mid-level categories
        var burgers    = new Facet { Title = "Burgers",    Description = "Classic and gourmet burgers" };
        var sandwiches = new Facet { Title = "Sandwiches", Description = "Hot and cold sandwiches" };
        var pizza      = new Facet { Title = "Pizza",      Description = "Stone-baked pizzas" };
        var salads     = new Facet { Title = "Salads",     Description = "Fresh and healthy salads" };
        var pasta      = new Facet { Title = "Pasta",      Description = "Italian pasta dishes" };
        var wraps      = new Facet { Title = "Wraps",      Description = "Tortilla wraps with various fillings" };
        var soups      = new Facet { Title = "Soups",      Description = "Hot soups and stews" };
        var desserts   = new Facet { Title = "Desserts",   Description = "Sweet treats and desserts" };
        var breakfast  = new Facet { Title = "Breakfast",  Description = "Morning meals and brunch" };
        var snacks     = new Facet { Title = "Snacks",     Description = "Light bites and snacks" };
        var hotDrinks  = new Facet { Title = "Hot Drinks",  Description = "Coffee, tea and warm beverages" };
        var coldDrinks = new Facet { Title = "Cold Drinks", Description = "Chilled beverages and juices" };
        var vegan      = new Facet { Title = "Vegan",       Description = "Plant-based options" };
        var vegetarian = new Facet { Title = "Vegetarian",  Description = "Meat-free options" };
        var glutenFree = new Facet { Title = "Gluten-Free", Description = "Gluten-free options" };
        var keto       = new Facet { Title = "Keto",        Description = "Low-carb, high-fat options" };

        // Leaf-level specifics
        var classicBurger  = new Facet { Title = "Classic Burger",  Description = "Traditional beef burger with lettuce and tomato" };
        var cheeseburger   = new Facet { Title = "Cheeseburger",   Description = "Beef burger with melted cheese" };
        var veggieBurger   = new Facet { Title = "Veggie Burger",  Description = "Plant-based burger patty" };
        var margherita     = new Facet { Title = "Margherita",     Description = "Classic tomato and mozzarella pizza" };
        var pepperoniPizza = new Facet { Title = "Pepperoni Pizza",Description = "Pizza topped with spicy pepperoni" };
        var caesarSalad    = new Facet { Title = "Caesar Salad",   Description = "Romaine lettuce with Caesar dressing and croutons" };
        var greekSalad     = new Facet { Title = "Greek Salad",    Description = "Tomato, cucumber, olives and feta cheese" };
        var carbonara      = new Facet { Title = "Carbonara",      Description = "Creamy egg and bacon pasta" };
        var bolognese      = new Facet { Title = "Bolognese",      Description = "Rich meat sauce pasta" };
        var iceCream       = new Facet { Title = "Ice Cream",      Description = "Assorted ice cream flavors" };
        var cake           = new Facet { Title = "Cake",           Description = "Freshly baked cakes and slices" };
        var coffee         = new Facet { Title = "Coffee",         Description = "Espresso-based beverages" };
        var tea            = new Facet { Title = "Tea",            Description = "Hot and iced tea varieties" };
        var freshJuice     = new Facet { Title = "Fresh Juice",    Description = "Freshly squeezed fruit juices" };
        var smoothies      = new Facet { Title = "Smoothies",      Description = "Blended fruit and yogurt smoothies" };
        var pancakes       = new Facet { Title = "Pancakes",       Description = "Fluffy breakfast pancakes" };
        var croissant      = new Facet { Title = "Croissant",      Description = "Buttery French pastry" };

        var facets = new List<Facet>
        {
            food, beverages, dietary,
            burgers, sandwiches, pizza, salads, pasta, wraps, soups, desserts, breakfast, snacks,
            hotDrinks, coldDrinks, vegan, vegetarian, glutenFree, keto,
            classicBurger, cheeseburger, veggieBurger, margherita, pepperoniPizza,
            caesarSalad, greekSalad, carbonara, bolognese,
            iceCream, cake, coffee, tea, freshJuice, smoothies, pancakes, croissant
        };

        foreach (var facet in facets)
            await facetService.Save(facet);
        await facetService.SaveChanges();

        // Parent-child relationships
        RelatedFacet Child(Facet child) => new() { ChildId = child.Id };

        food.ChildEntities       = [Child(burgers), Child(sandwiches), Child(pizza), Child(salads), Child(pasta), Child(wraps), Child(soups), Child(desserts), Child(breakfast), Child(snacks)];
        beverages.ChildEntities  = [Child(hotDrinks), Child(coldDrinks)];
        dietary.ChildEntities    = [Child(vegan), Child(vegetarian), Child(glutenFree), Child(keto)];
        burgers.ChildEntities    = [Child(classicBurger), Child(cheeseburger), Child(veggieBurger)];
        pizza.ChildEntities      = [Child(margherita), Child(pepperoniPizza)];
        salads.ChildEntities     = [Child(caesarSalad), Child(greekSalad)];
        pasta.ChildEntities      = [Child(carbonara), Child(bolognese)];
        desserts.ChildEntities   = [Child(iceCream), Child(cake)];
        hotDrinks.ChildEntities  = [Child(coffee), Child(tea)];
        coldDrinks.ChildEntities = [Child(freshJuice), Child(smoothies)];
        breakfast.ChildEntities  = [Child(pancakes), Child(croissant)];

        foreach (var parent in new[] { food, beverages, dietary, burgers, pizza, salads, pasta, desserts, hotDrinks, coldDrinks, breakfast })
            await facetService.Save(parent);
        await facetService.SaveChanges();

        return facets;
    }

    public async Task<IList<FacetGroup>> SeedFacetGroups(IList<Facet> facets)
    {
        var byTitle = facets.ToDictionary(f => f.Title, f => f);

        FacetGroupLink Link(Facet facet) => new() { ChildId = facet.Id };

        var groups = new List<FacetGroup>
        {
            new()
            {
                Code = "MAIN", Title = "Main Dishes", Description = "Hearty main course items",
                Facets = [Link(byTitle["Food"]), Link(byTitle["Burgers"]), Link(byTitle["Sandwiches"]),
                          Link(byTitle["Pizza"]), Link(byTitle["Pasta"]), Link(byTitle["Wraps"]),
                          Link(byTitle["Classic Burger"]), Link(byTitle["Cheeseburger"]),
                          Link(byTitle["Margherita"]), Link(byTitle["Pepperoni Pizza"]),
                          Link(byTitle["Carbonara"]), Link(byTitle["Bolognese"])]
            },
            new()
            {
                Code = "LIGHT", Title = "Light Meals", Description = "Lighter options and sides",
                Facets = [Link(byTitle["Salads"]), Link(byTitle["Soups"]), Link(byTitle["Snacks"]),
                          Link(byTitle["Caesar Salad"]), Link(byTitle["Greek Salad"])]
            },
            new()
            {
                Code = "SWEET", Title = "Sweet & Drinks", Description = "Desserts and beverages",
                Facets = [Link(byTitle["Desserts"]), Link(byTitle["Beverages"]),
                          Link(byTitle["Hot Drinks"]), Link(byTitle["Cold Drinks"]),
                          Link(byTitle["Ice Cream"]), Link(byTitle["Cake"]),
                          Link(byTitle["Coffee"]), Link(byTitle["Tea"]),
                          Link(byTitle["Fresh Juice"]), Link(byTitle["Smoothies"])]
            },
            new()
            {
                Code = "MEAL", Title = "Meal Time", Description = "Time-of-day meal categories",
                Facets = [Link(byTitle["Breakfast"]), Link(byTitle["Snacks"]),
                          Link(byTitle["Pancakes"]), Link(byTitle["Croissant"])]
            },
            new()
            {
                Code = "DIET", Title = "Dietary", Description = "Dietary preference categories",
                Facets = [Link(byTitle["Dietary"]), Link(byTitle["Vegan"]), Link(byTitle["Vegetarian"]),
                          Link(byTitle["Gluten-Free"]), Link(byTitle["Keto"]),
                          Link(byTitle["Veggie Burger"]), Link(byTitle["Caesar Salad"]), Link(byTitle["Greek Salad"])]
            },
            new()
            {
                Code = "POP", Title = "Popular Items", Description = "Customer favorites across all categories",
                Facets = [Link(byTitle["Classic Burger"]), Link(byTitle["Margherita"]),
                          Link(byTitle["Caesar Salad"]), Link(byTitle["Coffee"]),
                          Link(byTitle["Pancakes"]), Link(byTitle["Ice Cream"])]
            },
        };

        foreach (var group in groups)
            await facetGroupService.Save(group);
        await facetGroupService.SaveChanges();

        return groups;
    }

    public async Task<IList<Article>> SeedArticles(IList<Facet> facets, IList<Organization> suppliers)
    {
        var f = new Faker("en");
        var facetByTitle = facets.ToDictionary(x => x.Title, x => x);

        // Phase 1: seed ingredient articles
        var ingredients = new List<Article>
        {
            // Proteins
            new() { Title = "Beef Patty",      Description = "Seasoned beef patty",           Prices = [new ArticlePricePeriod { Price = 2.00m }] },
            new() { Title = "Veggie Patty",    Description = "Plant-based burger patty",      Prices = [new ArticlePricePeriod { Price = 2.25m }] },
            new() { Title = "Chicken Breast",  Description = "Grilled chicken breast",        Prices = [new ArticlePricePeriod { Price = 2.50m }] },
            new() { Title = "Ground Beef",     Description = "Seasoned ground beef",          Prices = [new ArticlePricePeriod { Price = 2.00m }] },
            new() { Title = "Bacon",           Description = "Crispy smoked bacon strips",    Prices = [new ArticlePricePeriod { Price = 1.50m }] },
            new() { Title = "Ham",             Description = "Sliced smoked ham",             Prices = [new ArticlePricePeriod { Price = 1.25m }] },
            new() { Title = "Turkey",          Description = "Sliced roasted turkey",         Prices = [new ArticlePricePeriod { Price = 1.50m }] },
            new() { Title = "Pepperoni",       Description = "Sliced pepperoni",              Prices = [new ArticlePricePeriod { Price = 1.00m }] },
            new() { Title = "Egg",             Description = "Free-range egg",                Prices = [new ArticlePricePeriod { Price = 0.35m }] },
            // Dairy
            new() { Title = "Cheese",          Description = "Cheddar cheese slice",          Prices = [new ArticlePricePeriod { Price = 0.75m }] },
            new() { Title = "Mozzarella",      Description = "Fresh mozzarella cheese",       Prices = [new ArticlePricePeriod { Price = 1.20m }] },
            new() { Title = "Parmesan",        Description = "Grated Parmesan cheese",        Prices = [new ArticlePricePeriod { Price = 1.00m }] },
            new() { Title = "Feta Cheese",     Description = "Crumbled feta cheese",          Prices = [new ArticlePricePeriod { Price = 1.10m }] },
            new() { Title = "Cream",           Description = "Heavy cooking cream",           Prices = [new ArticlePricePeriod { Price = 0.60m }] },
            new() { Title = "Butter",          Description = "Salted butter",                 Prices = [new ArticlePricePeriod { Price = 0.40m }] },
            new() { Title = "Yogurt",          Description = "Natural Greek yogurt",          Prices = [new ArticlePricePeriod { Price = 0.55m }] },
            // Vegetables
            new() { Title = "Lettuce",         Description = "Fresh romaine lettuce",         Prices = [new ArticlePricePeriod { Price = 0.25m }] },
            new() { Title = "Tomato",          Description = "Sliced ripe tomato",            Prices = [new ArticlePricePeriod { Price = 0.30m }] },
            new() { Title = "Onion",           Description = "Sliced red onion",              Prices = [new ArticlePricePeriod { Price = 0.20m }] },
            new() { Title = "Cucumber",        Description = "Sliced fresh cucumber",         Prices = [new ArticlePricePeriod { Price = 0.25m }] },
            new() { Title = "Bell Pepper",     Description = "Sliced mixed bell peppers",     Prices = [new ArticlePricePeriod { Price = 0.40m }] },
            new() { Title = "Mushrooms",       Description = "Sliced button mushrooms",       Prices = [new ArticlePricePeriod { Price = 0.45m }] },
            new() { Title = "Olives",          Description = "Kalamata olives",               Prices = [new ArticlePricePeriod { Price = 0.50m }] },
            new() { Title = "Avocado",         Description = "Fresh sliced avocado",          Prices = [new ArticlePricePeriod { Price = 1.50m }] },
            new() { Title = "Pickles",         Description = "Dill pickle slices",            Prices = [new ArticlePricePeriod { Price = 0.25m }] },
            new() { Title = "Jalapeños",       Description = "Pickled jalapeño slices",       Prices = [new ArticlePricePeriod { Price = 0.35m }] },
            // Sauces & condiments
            new() { Title = "Ketchup",         Description = "Tomato ketchup",                Prices = [new ArticlePricePeriod { Price = 0.15m }] },
            new() { Title = "Mustard Sauce",   Description = "Yellow mustard",                Prices = [new ArticlePricePeriod { Price = 0.15m }] },
            new() { Title = "Mayonnaise",      Description = "Classic mayo",                  Prices = [new ArticlePricePeriod { Price = 0.15m }] },
            new() { Title = "Tomato Sauce",    Description = "Classic pizza tomato sauce",     Prices = [new ArticlePricePeriod { Price = 0.35m }] },
            new() { Title = "Caesar Dressing", Description = "Creamy Caesar salad dressing",  Prices = [new ArticlePricePeriod { Price = 0.45m }] },
            new() { Title = "Sriracha",        Description = "Spicy sriracha sauce",          Prices = [new ArticlePricePeriod { Price = 0.20m }] },
            new() { Title = "Chocolate Sauce", Description = "Dark chocolate drizzle",        Prices = [new ArticlePricePeriod { Price = 0.40m }] },
            new() { Title = "Caramel Syrup",   Description = "Caramel flavored syrup",        Prices = [new ArticlePricePeriod { Price = 0.45m }] },
            new() { Title = "Maple Syrup",     Description = "Pure maple syrup",              Prices = [new ArticlePricePeriod { Price = 0.50m }] },
            // Bases & grains
            new() { Title = "Bun",             Description = "Brioche burger bun",            Prices = [new ArticlePricePeriod { Price = 0.50m }] },
            new() { Title = "Pizza Dough",     Description = "Stone-baked pizza base",        Prices = [new ArticlePricePeriod { Price = 1.00m }] },
            new() { Title = "Tortilla",        Description = "Flour tortilla wrap",           Prices = [new ArticlePricePeriod { Price = 0.40m }] },
            new() { Title = "Spaghetti",       Description = "Italian durum wheat spaghetti", Prices = [new ArticlePricePeriod { Price = 0.55m }] },
            new() { Title = "Croutons",        Description = "Garlic herb croutons",          Prices = [new ArticlePricePeriod { Price = 0.30m }] },
            // Beverages & extras
            new() { Title = "Espresso",        Description = "Single espresso shot",          Prices = [new ArticlePricePeriod { Price = 0.80m }] },
            new() { Title = "Extra Shot",      Description = "Additional espresso shot",      Prices = [new ArticlePricePeriod { Price = 0.80m }] },
            new() { Title = "Tea Leaves",      Description = "Premium loose-leaf tea",        Prices = [new ArticlePricePeriod { Price = 0.60m }] },
            new() { Title = "Oat Milk",        Description = "Barista oat milk",              Prices = [new ArticlePricePeriod { Price = 0.60m }] },
            new() { Title = "Soy Milk",        Description = "Organic soy milk",              Prices = [new ArticlePricePeriod { Price = 0.50m }] },
            new() { Title = "Mixed Fruit",     Description = "Seasonal fresh fruit blend",    Prices = [new ArticlePricePeriod { Price = 1.20m }] },
            new() { Title = "Whipped Cream",   Description = "Fresh whipped cream topping",   Prices = [new ArticlePricePeriod { Price = 0.50m }] },
        };

        foreach (var ingredient in ingredients)
        {
            if (f.Random.Bool(0.6f))
            {
                ingredient.Suppliers = f.PickRandom(suppliers, f.Random.Int(1, Math.Min(3, suppliers.Count)))
                    .DistinctBy(s => s.Id)
                    .Select(s => new ArticleSupplier { SupplierId = s.Id })
                    .ToList();
            }
            await articleService.Save(ingredient);
        }
        await articleService.SaveChanges();

        // Lookup helpers
        var ing = ingredients.ToDictionary(i => i.Title, i => i);
        ArticleComponent Comp(string name, decimal qty = 1, bool omittable = false)
            => new() { ComponentId = ing[name].Id, Quantity = qty, IsOmittable = omittable };
        ArticleAllowedComponentAddition Extra(string name)
            => new() { ComponentId = ing[name].Id };
        ArticleFacet Tag(string name)
            => new() { FacetId = facetByTitle[name].Id };

        // Phase 2: menu items derived from leaf-level facets
        var menuItems = new List<Article>
        {
            // --- Burgers ---
            new()
            {
                Title = "Classic Burger",
                Description = "Traditional beef burger with lettuce and tomato",
                Prices = [new ArticlePricePeriod { Price = 8.99m }],
                AllowAdditions = true,
                Facets = [Tag("Classic Burger"), Tag("Burgers"), Tag("Food")],
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
                Prices = [new ArticlePricePeriod { Price = 9.49m }],
                AllowAdditions = true,
                Facets = [Tag("Cheeseburger"), Tag("Burgers"), Tag("Food")],
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
                Prices = [new ArticlePricePeriod { Price = 9.99m }],
                AllowAdditions = true,
                Facets = [Tag("Veggie Burger"), Tag("Burgers"), Tag("Food"), Tag("Vegan"), Tag("Vegetarian")],
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
                Prices = [new ArticlePricePeriod { Price = 11.99m }],
                AllowAdditions = true,
                Facets = [Tag("Margherita"), Tag("Pizza"), Tag("Food"), Tag("Vegetarian")],
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
                Prices = [new ArticlePricePeriod { Price = 13.49m }],
                AllowAdditions = true,
                Facets = [Tag("Pepperoni Pizza"), Tag("Pizza"), Tag("Food")],
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
                Prices = [new ArticlePricePeriod { Price = 8.49m }],
                AllowAdditions = true,
                Facets = [Tag("Caesar Salad"), Tag("Salads"), Tag("Food")],
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
                Prices = [new ArticlePricePeriod { Price = 7.99m }],
                AllowAdditions = true,
                Facets = [Tag("Greek Salad"), Tag("Salads"), Tag("Food"), Tag("Vegetarian")],
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
                Prices = [new ArticlePricePeriod { Price = 12.49m }],
                AllowAdditions = true,
                Facets = [Tag("Carbonara"), Tag("Pasta"), Tag("Food")],
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
                Prices = [new ArticlePricePeriod { Price = 11.99m }],
                AllowAdditions = true,
                Facets = [Tag("Bolognese"), Tag("Pasta"), Tag("Food")],
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
                Prices = [new ArticlePricePeriod { Price = 4.99m }],
                AllowAdditions = true,
                Facets = [Tag("Ice Cream"), Tag("Desserts"), Tag("Food")],
                Components = [Comp("Whipped Cream", omittable: true)],
                AllowedComponentAdditions = [Extra("Chocolate Sauce"), Extra("Caramel Syrup")]
            },
            new()
            {
                Title = "Cake",
                Description = "Freshly baked cakes and slices",
                Prices = [new ArticlePricePeriod { Price = 5.49m }],
                AllowAdditions = true,
                Facets = [Tag("Cake"), Tag("Desserts"), Tag("Food")],
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
                Prices = [new ArticlePricePeriod { Price = 3.49m }],
                AllowAdditions = true,
                Facets = [Tag("Coffee"), Tag("Hot Drinks"), Tag("Beverages")],
                Components = [Comp("Espresso")],
                AllowedComponentAdditions = [Extra("Extra Shot"), Extra("Oat Milk"), Extra("Soy Milk"), Extra("Whipped Cream"), Extra("Caramel Syrup")]
            },
            new()
            {
                Title = "Tea",
                Description = "Hot and iced tea varieties",
                Prices = [new ArticlePricePeriod { Price = 2.99m }],
                AllowAdditions = true,
                Facets = [Tag("Tea"), Tag("Hot Drinks"), Tag("Beverages")],
                Components = [Comp("Tea Leaves")],
                AllowedComponentAdditions = [Extra("Oat Milk"), Extra("Soy Milk")]
            },
            // --- Cold Drinks ---
            new()
            {
                Title = "Fresh Juice",
                Description = "Freshly squeezed fruit juices",
                Prices = [new ArticlePricePeriod { Price = 4.49m }],
                AllowAdditions = false,
                Facets = [Tag("Fresh Juice"), Tag("Cold Drinks"), Tag("Beverages"), Tag("Vegan")],
                Components = [Comp("Mixed Fruit", qty: 3)]
            },
            new()
            {
                Title = "Smoothie",
                Description = "Blended fruit and yogurt smoothies",
                Prices = [new ArticlePricePeriod { Price = 5.49m }],
                AllowAdditions = true,
                Facets = [Tag("Smoothies"), Tag("Cold Drinks"), Tag("Beverages"), Tag("Vegetarian")],
                Components = [Comp("Mixed Fruit", qty: 2), Comp("Yogurt")],
                AllowedComponentAdditions = [Extra("Whipped Cream"), Extra("Chocolate Sauce")]
            },
            // --- Breakfast ---
            new()
            {
                Title = "Pancakes",
                Description = "Fluffy breakfast pancakes",
                Prices = [new ArticlePricePeriod { Price = 7.49m }],
                AllowAdditions = true,
                Facets = [Tag("Pancakes"), Tag("Breakfast"), Tag("Food"), Tag("Vegetarian")],
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
                Prices = [new ArticlePricePeriod { Price = 3.49m }],
                AllowAdditions = true,
                Facets = [Tag("Croissant"), Tag("Breakfast"), Tag("Food"), Tag("Vegetarian")],
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
                    .Select(s => new ArticleSupplier { SupplierId = s.Id })
                    .ToList();
            }
            await articleService.Save(item);
        }
        await articleService.SaveChanges();

        // Phase 3: random article variants for volume
        string[] adjectives = ["Spicy", "Smoky", "Crispy", "Golden", "Grilled", "Fresh", "Loaded", "Signature", "Double", "BBQ"];
        string[] nouns = ["Burger", "Sandwich", "Wrap", "Bowl", "Salad", "Pizza", "Pasta", "Toast", "Roll", "Plate"];

        var allArticles = new List<Article>(menuItems);

        for (int i = 0; i < 500; i++)
        {
            var price = Math.Round(f.Random.Decimal(3.50m, 24.99m), 2);

            var selectedIngredients = f.PickRandom(ingredients, f.Random.Int(2, Math.Min(6, ingredients.Count)))
                .DistinctBy(c => c.Id)
                .ToList();

            var article = new Article
            {
                Title = $"{f.PickRandom(adjectives)} {f.PickRandom(nouns)} #{i + 1}",
                Description = f.Lorem.Sentence(),
                Prices = [new ArticlePricePeriod { Price = price }],
                Facets = f.PickRandom(facets, f.Random.Int(1, 3))
                    .DistinctBy(c => c.Id)
                    .Select(c => new ArticleFacet { FacetId = c.Id })
                    .ToList(),
                Components = selectedIngredients
                    .Select(c => new ArticleComponent { ComponentId = c.Id, Quantity = f.Random.Decimal(0.5m, 5m), IsOmittable = f.Random.Bool(0.4f) })
                    .ToList(),
                AllowAdditions = f.Random.Bool(0.9f),
                AllowedComponentAdditions = f.Random.Bool(0.5f)
                    ? f.PickRandom(ingredients, f.Random.Int(1, Math.Min(3, ingredients.Count)))
                        .DistinctBy(c => c.Id)
                        .Select(c => new ArticleAllowedComponentAddition { ComponentId = c.Id })
                        .ToList()
                    : null,
                Suppliers = f.Random.Bool(0.5f)
                    ? f.PickRandom(suppliers, f.Random.Int(1, Math.Min(2, suppliers.Count)))
                        .DistinctBy(s => s.Id)
                        .Select(s => new ArticleSupplier { SupplierId = s.Id })
                        .ToList()
                    : null,
            };

            allArticles.Add(article);
            await articleService.Save(article);
            if ((i + 1) % BatchSize == 0)
                await articleService.SaveChanges();
        }
        await articleService.SaveChanges();

        return allArticles;
    }

    public async Task<IList<Order>> SeedOrders(IList<Party> customers, IList<Article> articles)
    {
        var f = new Faker("en");
        var statuses = Enum.GetValues<OrderStatus>();
        var orders = new List<Order>(10000);

        // Collect component articles for additions/omissions
        var componentArticles = articles
            .SelectMany(a => a.Components?.Select(c => c.ComponentId) ?? [])
            .Distinct()
            .ToList();

        for (int i = 0; i < 10000; i++)
        {
            var customer = f.PickRandom(customers);
            var selectedArticles = f.PickRandom(articles, f.Random.Int(1, 5)).DistinctBy(a => a.Id).ToList();

            var orderLines = selectedArticles.Select((article, idx) =>
            {
                var unitPrice = article.Prices?.FirstOrDefault()?.Price ?? 0m;
                var quantity = f.Random.Decimal(1m, 3m);

                List<OrderLineComponentAddition>? partAdditions = null;
                if (f.Random.Bool(0.4f) && componentArticles.Count > 0)
                {
                    partAdditions = f.PickRandom(componentArticles, f.Random.Int(1, Math.Min(3, componentArticles.Count)))
                        .Distinct()
                        .Select(id => new OrderLineComponentAddition { ArticleId = id, Price = f.Random.Decimal(0.15m, 1.50m) })
                        .ToList();
                }

                List<OrderLineComponentOmission>? partOmissions = null;
                if (article.Components?.Count > 0 && f.Random.Bool(0.3f))
                {
                    var omittable = article.Components.Where(c => c.IsOmittable).Select(c => c.ComponentId).ToList();
                    if (omittable.Count > 0)
                    {
                        partOmissions = f.PickRandom(omittable, f.Random.Int(1, Math.Min(2, omittable.Count)))
                            .Distinct()
                            .Select(id => new OrderLineComponentOmission { ArticleId = id, Price = f.Random.Decimal(0.15m, 1.50m) })
                            .ToList();
                    }
                }

                return new OrderLine
                {
                    ArticleId = article.Id,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    SortOrder = idx,
                    ComponentAdditions = partAdditions,
                    ComponentOmissions = partOmissions
                };
            }).ToList();

            var order = new Order
            {
                CustomerId = customer.Id,
                Status = f.PickRandom(statuses),
                OrderLines = orderLines,
                ScheduledDate = f.Random.Bool(0.3f) ? DateTime.UtcNow.AddDays(f.Random.Int(1, 60)) : null,
            };

            orders.Add(order);
            await orderService.Save(order);
            if ((i + 1) % BatchSize == 0)
                await orderService.SaveChanges();
        }
        await orderService.SaveChanges();

        return orders;
    }
}
