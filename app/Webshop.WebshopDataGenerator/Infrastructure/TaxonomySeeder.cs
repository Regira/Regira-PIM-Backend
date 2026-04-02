using Microsoft.Extensions.Logging;
using Regira.Entities.Services.Abstractions;
using Webshop.Models.Taxonomy.FacetGroups;
using Webshop.Models.Taxonomy.Facets;

namespace Webshop.WebshopDataGenerator.Infrastructure;

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
        var facets = await facetService.List();

        if (facets.Any())
        {
            logger.LogInformation("Facets already exist, skipping seeding.");
            return facets;
        }

        // Root categories
        var food = new Facet { Title = "Food", Description = "All food items" };
        var beverages = new Facet { Title = "Beverages", Description = "All drinks and beverages" };
        var dietary = new Facet { Title = "Dietary", Description = "Dietary preference categories" };

        // Mid-level categories
        var burgers = new Facet { Title = "Burgers", Description = "Classic and gourmet burgers" };
        var sandwiches = new Facet { Title = "Sandwiches", Description = "Hot and cold sandwiches" };
        var pizza = new Facet { Title = "Pizza", Description = "Stone-baked pizzas" };
        var salads = new Facet { Title = "Salads", Description = "Fresh and healthy salads" };
        var pasta = new Facet { Title = "Pasta", Description = "Italian pasta dishes" };
        var wraps = new Facet { Title = "Wraps", Description = "Tortilla wraps with various fillings" };
        var soups = new Facet { Title = "Soups", Description = "Hot soups and stews" };
        var desserts = new Facet { Title = "Desserts", Description = "Sweet treats and desserts" };
        var breakfast = new Facet { Title = "Breakfast", Description = "Morning meals and brunch" };
        var snacks = new Facet { Title = "Snacks", Description = "Light bites and snacks" };
        var hotDrinks = new Facet { Title = "Hot Drinks", Description = "Coffee, tea and warm beverages" };
        var coldDrinks = new Facet { Title = "Cold Drinks", Description = "Chilled beverages and juices" };
        var vegan = new Facet { Title = "Vegan", Description = "Plant-based options" };
        var vegetarian = new Facet { Title = "Vegetarian", Description = "Meat-free options" };
        var glutenFree = new Facet { Title = "Gluten-Free", Description = "Gluten-free options" };
        var keto = new Facet { Title = "Keto", Description = "Low-carb, high-fat options" };

        // Leaf-level specifics
        var classicBurger = new Facet { Title = "Classic Burger", Description = "Traditional beef burger with lettuce and tomato" };
        var cheeseburger = new Facet { Title = "Cheeseburger", Description = "Beef burger with melted cheese" };
        var veggieBurger = new Facet { Title = "Veggie Burger", Description = "Plant-based burger patty" };
        var margherita = new Facet { Title = "Margherita", Description = "Classic tomato and mozzarella pizza" };
        var pepperoniPizza = new Facet { Title = "Pepperoni Pizza", Description = "Pizza topped with spicy pepperoni" };
        var caesarSalad = new Facet { Title = "Caesar Salad", Description = "Romaine lettuce with Caesar dressing and croutons" };
        var greekSalad = new Facet { Title = "Greek Salad", Description = "Tomato, cucumber, olives and feta cheese" };
        var carbonara = new Facet { Title = "Carbonara", Description = "Creamy egg and bacon pasta" };
        var bolognese = new Facet { Title = "Bolognese", Description = "Rich meat sauce pasta" };
        var iceCream = new Facet { Title = "Ice Cream", Description = "Assorted ice cream flavors" };
        var cake = new Facet { Title = "Cake", Description = "Freshly baked cakes and slices" };
        var coffee = new Facet { Title = "Coffee", Description = "Espresso-based beverages" };
        var tea = new Facet { Title = "Tea", Description = "Hot and iced tea varieties" };
        var freshJuice = new Facet { Title = "Fresh Juice", Description = "Freshly squeezed fruit juices" };
        var smoothies = new Facet { Title = "Smoothies", Description = "Blended fruit and yogurt smoothies" };
        var pancakes = new Facet { Title = "Pancakes", Description = "Fluffy breakfast pancakes" };
        var croissant = new Facet { Title = "Croissant", Description = "Buttery French pastry" };

        facets = new List<Facet>
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

        // Parent-child relationships — saved after flush so parent Ids are set
        FacetLink Child(Facet child) => new() { ChildId = child.Id };

        food.ChildEntities = [Child(burgers), Child(sandwiches), Child(pizza), Child(salads), Child(pasta), Child(wraps), Child(soups), Child(desserts), Child(breakfast), Child(snacks)];
        beverages.ChildEntities = [Child(hotDrinks), Child(coldDrinks)];
        dietary.ChildEntities = [Child(vegan), Child(vegetarian), Child(glutenFree), Child(keto)];
        burgers.ChildEntities = [Child(classicBurger), Child(cheeseburger), Child(veggieBurger)];
        pizza.ChildEntities = [Child(margherita), Child(pepperoniPizza)];
        salads.ChildEntities = [Child(caesarSalad), Child(greekSalad)];
        pasta.ChildEntities = [Child(carbonara), Child(bolognese)];
        desserts.ChildEntities = [Child(iceCream), Child(cake)];
        hotDrinks.ChildEntities = [Child(coffee), Child(tea)];
        coldDrinks.ChildEntities = [Child(freshJuice), Child(smoothies)];
        breakfast.ChildEntities = [Child(pancakes), Child(croissant)];

        logger.LogInformation("Seeding facets...");
        foreach (var parent in new[] { food, beverages, dietary, burgers, pizza, salads, pasta, desserts, hotDrinks, coldDrinks, breakfast })
            await facetService.Save(parent);
        await facetService.SaveChanges();

        return facets;
    }

    public async Task<IList<FacetGroup>> SeedFacetGroups(IList<Facet> facets)
    {
        var groups = await facetGroupService.List();

        if (groups.Any())
        {
            logger.LogInformation("Facet groups already exist, skipping seeding.");
            return groups;
        }

        var byTitle = facets.ToDictionary(f => f.Title, f => f);

        FacetParentGroup Link(Facet facet) => new() { FacetId = facet.Id };

        groups = new List<FacetGroup>
        {
            new()
            {
                Code = "MAIN", Title = "Main Dishes", Description = "Hearty main course items",
                ChildFacets = [Link(byTitle["Food"]), Link(byTitle["Burgers"]), Link(byTitle["Sandwiches"]),
                          Link(byTitle["Pizza"]), Link(byTitle["Pasta"]), Link(byTitle["Wraps"]),
                          Link(byTitle["Classic Burger"]), Link(byTitle["Cheeseburger"]),
                          Link(byTitle["Margherita"]), Link(byTitle["Pepperoni Pizza"]),
                          Link(byTitle["Carbonara"]), Link(byTitle["Bolognese"])]
            },
            new()
            {
                Code = "LIGHT", Title = "Light Meals", Description = "Lighter options and sides",
                ChildFacets = [Link(byTitle["Salads"]), Link(byTitle["Soups"]), Link(byTitle["Snacks"]),
                          Link(byTitle["Caesar Salad"]), Link(byTitle["Greek Salad"])]
            },
            new()
            {
                Code = "SWEET", Title = "Sweet & Drinks", Description = "Desserts and beverages",
                ChildFacets = [Link(byTitle["Desserts"]), Link(byTitle["Beverages"]),
                          Link(byTitle["Hot Drinks"]), Link(byTitle["Cold Drinks"]),
                          Link(byTitle["Ice Cream"]), Link(byTitle["Cake"]),
                          Link(byTitle["Coffee"]), Link(byTitle["Tea"]),
                          Link(byTitle["Fresh Juice"]), Link(byTitle["Smoothies"])]
            },
            new()
            {
                Code = "MEAL", Title = "Meal Time", Description = "Time-of-day meal categories",
                ChildFacets = [Link(byTitle["Breakfast"]), Link(byTitle["Snacks"]),
                          Link(byTitle["Pancakes"]), Link(byTitle["Croissant"])]
            },
            new()
            {
                Code = "DIET", Title = "Dietary", Description = "Dietary preference categories",
                ChildFacets = [Link(byTitle["Dietary"]), Link(byTitle["Vegan"]), Link(byTitle["Vegetarian"]),
                          Link(byTitle["Gluten-Free"]), Link(byTitle["Keto"]),
                          Link(byTitle["Veggie Burger"]), Link(byTitle["Caesar Salad"]), Link(byTitle["Greek Salad"])]
            },
            new()
            {
                Code = "POP", Title = "Popular Items", Description = "Customer favorites across all categories",
                ChildFacets = [Link(byTitle["Classic Burger"]), Link(byTitle["Margherita"]),
                          Link(byTitle["Caesar Salad"]), Link(byTitle["Coffee"]),
                          Link(byTitle["Pancakes"]), Link(byTitle["Ice Cream"])]
            },
        };

        logger.LogInformation("Seeding facet groups...");

        // Save groups without links first to get their generated Ids,
        // then re-save with links to avoid EF Core 8 MERGE FK conflicts.
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
