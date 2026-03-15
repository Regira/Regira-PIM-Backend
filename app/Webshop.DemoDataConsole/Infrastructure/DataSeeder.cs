using Bogus;
using Regira.Entities.Services.Abstractions;
using Webshop.Models.Entities.Catalog.Allergens;
using Webshop.Models.Entities.Catalog.Articles;
using Webshop.Models.Entities.Catalog.Categories;
using Webshop.Models.Entities.Orders;
using Webshop.Models.Entities.Stakeholders.ContactData;
using Webshop.Models.Entities.Stakeholders.Parties;
using Webshop.Models.Entities.Stakeholders.Parties.Relations;
using Person = Webshop.Models.Entities.Stakeholders.Parties.Person;

namespace Webshop.DemoDataConsole.Infrastructure;

public class DataSeeder(IEntityService<Category> categoryService, IEntityService<Article> articleService,
    IEntityService<Party> partyService, IEntityService<Order> orderService, IEntityService<RelationshipType> relationshipTypeService)
{
    private const int BatchSize = 100;

    public async Task SeedAsync()
    {
        var parties = await SeedParties();
        var relationshipTypes = await SeedRelationshipTypes();
        await SeedRelationships(parties, relationshipTypes);
        var categories = await SeedCategories();
        var suppliers = parties.OfType<Organization>().ToList();
        var articles = await SeedArticles(categories, suppliers);

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

    public async Task<IList<Category>> SeedCategories()
    {
        var categories = new List<Category>
        {
            new() { Title = "Burgers",    Description = "Classic and gourmet burgers" },
            new() { Title = "Sandwiches", Description = "Hot and cold sandwiches" },
            new() { Title = "Pizza",      Description = "Stone-baked pizzas" },
            new() { Title = "Salads",     Description = "Fresh and healthy salads" },
            new() { Title = "Pasta",      Description = "Italian pasta dishes" },
            new() { Title = "Wraps",      Description = "Tortilla wraps with various fillings" },
            new() { Title = "Soups",      Description = "Hot soups and stews" },
            new() { Title = "Desserts",   Description = "Sweet treats and desserts" },
            new() { Title = "Drinks",     Description = "Beverages and drinks" },
            new() { Title = "Breakfast",  Description = "Morning meals and brunch" },
            new() { Title = "Snacks",     Description = "Light bites and snacks" },
            new() { Title = "Vegan",      Description = "Plant-based options" },
        };

        foreach (var category in categories)
            await categoryService.Save(category);
        await categoryService.SaveChanges();

        return categories;
    }

    public async Task<IList<Article>> SeedArticles(IList<Category> categories, IList<Organization> suppliers)
    {
        var f = new Faker("en");

        // Phase 1: seed component articles (ingredients/toppings)
        var components = new List<Article>
        {
            new() { Title = "Cheese",          Description = "Cheddar cheese slice",         Prices = [new ArticlePriceHistory { Price = 0.75m }] },
            new() { Title = "Lettuce",         Description = "Fresh romaine lettuce",         Prices = [new ArticlePriceHistory { Price = 0.25m }] },
            new() { Title = "Tomato",          Description = "Sliced ripe tomato",            Prices = [new ArticlePriceHistory { Price = 0.30m }] },
            new() { Title = "Onion",           Description = "Sliced red onion",              Prices = [new ArticlePriceHistory { Price = 0.20m }] },
            new() { Title = "Pickles",         Description = "Dill pickle slices",            Prices = [new ArticlePriceHistory { Price = 0.25m }] },
            new() { Title = "Bacon",           Description = "Crispy smoked bacon strips",    Prices = [new ArticlePriceHistory { Price = 1.50m }] },
            new() { Title = "Ham",             Description = "Sliced smoked ham",             Prices = [new ArticlePriceHistory { Price = 1.25m }] },
            new() { Title = "Turkey",          Description = "Sliced roasted turkey",         Prices = [new ArticlePriceHistory { Price = 1.50m }] },
            new() { Title = "Mayonnaise",      Description = "Classic mayo",                  Prices = [new ArticlePriceHistory { Price = 0.15m }] },
            new() { Title = "Ketchup",         Description = "Tomato ketchup",                Prices = [new ArticlePriceHistory { Price = 0.15m }] },
            new() { Title = "Mustard Sauce",   Description = "Yellow mustard",                Prices = [new ArticlePriceHistory { Price = 0.15m }] },
            new() { Title = "Extra Shot",      Description = "Additional espresso shot",      Prices = [new ArticlePriceHistory { Price = 0.80m }] },
            new() { Title = "Oat Milk",        Description = "Barista oat milk",              Prices = [new ArticlePriceHistory { Price = 0.60m }] },
            new() { Title = "Soy Milk",        Description = "Organic soy milk",              Prices = [new ArticlePriceHistory { Price = 0.50m }] },
            new() { Title = "Whipped Cream",   Description = "Fresh whipped cream topping",   Prices = [new ArticlePriceHistory { Price = 0.50m }] },
            new() { Title = "Chocolate Sauce", Description = "Dark chocolate drizzle",        Prices = [new ArticlePriceHistory { Price = 0.40m }] },
            new() { Title = "Caramel Syrup",   Description = "Caramel flavored syrup",        Prices = [new ArticlePriceHistory { Price = 0.45m }] },
            new() { Title = "Avocado",         Description = "Fresh sliced avocado",          Prices = [new ArticlePriceHistory { Price = 1.50m }] },
            new() { Title = "Jalapeños",       Description = "Pickled jalapeño slices",       Prices = [new ArticlePriceHistory { Price = 0.35m }] },
            new() { Title = "Sriracha",        Description = "Spicy sriracha sauce",          Prices = [new ArticlePriceHistory { Price = 0.20m }] },
            new() { Title = "Beef Patty",      Description = "Seasoned beef patty",           Prices = [new ArticlePriceHistory { Price = 2.00m }] },
            new() { Title = "Chicken Breast",  Description = "Grilled chicken breast",        Prices = [new ArticlePriceHistory { Price = 2.50m }] },
            new() { Title = "Bun",             Description = "Brioche burger bun",            Prices = [new ArticlePriceHistory { Price = 0.50m }] },
            new() { Title = "Tortilla",        Description = "Flour tortilla wrap",           Prices = [new ArticlePriceHistory { Price = 0.40m }] },
            new() { Title = "Pizza Dough",     Description = "Stone-baked pizza base",        Prices = [new ArticlePriceHistory { Price = 1.00m }] },
            new() { Title = "Mozzarella",      Description = "Fresh mozzarella cheese",       Prices = [new ArticlePriceHistory { Price = 1.20m }] },
            new() { Title = "Tomato Sauce",    Description = "Classic pizza tomato sauce",    Prices = [new ArticlePriceHistory { Price = 0.35m }] },
            new() { Title = "Pepperoni",       Description = "Sliced pepperoni",              Prices = [new ArticlePriceHistory { Price = 1.00m }] },
            new() { Title = "Mushrooms",       Description = "Sliced button mushrooms",       Prices = [new ArticlePriceHistory { Price = 0.45m }] },
            new() { Title = "Bell Pepper",     Description = "Sliced mixed bell peppers",     Prices = [new ArticlePriceHistory { Price = 0.40m }] },
        };

        foreach (var component in components)
        {
            // Assign suppliers to components
            if (f.Random.Bool(0.6f))
            {
                component.Suppliers = f.PickRandom(suppliers, f.Random.Int(1, Math.Min(3, suppliers.Count)))
                    .DistinctBy(s => s.Id)
                    .Select(s => new ArticleSupplier { SupplierId = s.Id })
                    .ToList();
            }
            await articleService.Save(component);
        }
        await articleService.SaveChanges();

        // Phase 2: seed assembly articles (products composed of components)
        string[] adjectives = ["Classic", "Spicy", "Smoky", "Crispy", "Golden", "Grilled", "Fresh", "Loaded", "Signature", "Double"];
        string[] nouns = ["Burger", "Sandwich", "Wrap", "Bowl", "Salad", "Pizza", "Pasta", "Toast", "Roll", "Plate"];

        var assemblies = new List<Article>(1000);

        for (int i = 0; i < 1000; i++)
        {
            var price = Math.Round(f.Random.Decimal(3.50m, 24.99m), 2);

            var selectedComponents = f.PickRandom(components, f.Random.Int(2, Math.Min(6, components.Count)))
                .DistinctBy(c => c.Id)
                .ToList();

            var article = new Article
            {
                Title = $"{f.PickRandom(adjectives)} {f.PickRandom(nouns)} #{i + 1}",
                Description = f.Lorem.Sentence(),
                Prices = [new ArticlePriceHistory { Price = price }],
                Categories = f.PickRandom(categories, f.Random.Int(1, 3))
                    .DistinctBy(c => c.Id)
                    .Select(c => new ArticleCategory { CategoryId = c.Id })
                    .ToList(),
                Components = selectedComponents
                    .Select(c => new ArticleComponent { ComponentId = c.Id, Quantity = f.Random.Decimal(0.5m, 5m), IsOmittable = f.Random.Bool(0.4f) })
                    .ToList(),
                AllowAdditions = f.Random.Bool(0.9f),
                AllowedComponentAdditions = f.Random.Bool(0.5f)
                    ? f.PickRandom(components, f.Random.Int(1, Math.Min(3, components.Count)))
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

            assemblies.Add(article);
            await articleService.Save(article);
            if ((i + 1) % BatchSize == 0)
                await articleService.SaveChanges();
        }
        await articleService.SaveChanges();

        return assemblies;
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
