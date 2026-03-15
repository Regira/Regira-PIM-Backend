using Bogus;
using Regira.Entities.Services.Abstractions;
using Webshop.Models.Entities.Catalog.Allergens;
using Webshop.Models.Entities.Catalog.Categories;
using Webshop.Models.Entities.Catalog.Parts;
using Webshop.Models.Entities.Catalog.Products;
using Webshop.Models.Entities.Orders;
using Webshop.Models.Entities.Stakeholders.ContactData;
using Webshop.Models.Entities.Stakeholders.Parties;
using Webshop.Models.Entities.Stakeholders.Parties.Relations;
using Person = Webshop.Models.Entities.Stakeholders.Parties.Person;

namespace Webshop.DemoDataConsole.Infrastructure;

public class DataSeeder(IEntityService<Category> categoryService, IEntityService<Product> productService, IEntityService<Allergen> allergenService, IEntityService<Part> partService,
    IEntityService<Party> partyService, IEntityService<Order> orderService, IEntityService<RelationshipType> relationshipTypeService)
{
    private const int BatchSize = 100;

    public async Task SeedAsync()
    {
        var parties = await SeedParties();
        var relationshipTypes = await SeedRelationshipTypes();
        await SeedRelationships(parties, relationshipTypes);
        var allergens = await SeedAllergens();
        var parts = await SeedParts();
        var categories = await SeedCategories();
        var products = await SeedProducts(categories, allergens, parts);

        await SeedOrders(parties, products, allergens, parts);
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

    public async Task<IList<Allergen>> SeedAllergens()
    {
        var allergens = new List<Allergen>
        {
            new() { Title = "Gluten",    Code = "GLU", Description = "Found in wheat, barley, rye and oats" },
            new() { Title = "Dairy",     Code = "DAI", Description = "Includes milk, cheese, butter and cream" },
            new() { Title = "Nuts",      Code = "NUT", Description = "Tree nuts including almonds, walnuts, cashews" },
            new() { Title = "Eggs",      Code = "EGG", Description = "Chicken eggs and derivatives" },
            new() { Title = "Soy",       Code = "SOY", Description = "Soybeans and soy-derived products" },
            new() { Title = "Shellfish", Code = "SHE", Description = "Shrimp, crab, lobster and other crustaceans" },
            new() { Title = "Fish",      Code = "FIS", Description = "All species of finned fish" },
            new() { Title = "Sesame",    Code = "SES", Description = "Sesame seeds and sesame oil" },
            new() { Title = "Celery",    Code = "CEL", Description = "Including celeriac" },
            new() { Title = "Mustard",   Code = "MUS", Description = "Mustard seeds and derivatives" },
            new() { Title = "Lupin",     Code = "LUP", Description = "Lupin seeds and flour" },
            new() { Title = "Peanuts",   Code = "PEA", Description = "Peanuts and peanut-derived products" },
        };

        foreach (var allergen in allergens)
            await allergenService.Save(allergen);
        await allergenService.SaveChanges();

        return allergens;
    }

    public async Task<IList<Part>> SeedParts()
    {
        var parts = new List<Part>
        {
            new() { Title = "Cheese",          Description = "Cheddar cheese slice",         Prices = [new PartPriceHistory { Price = 0.75m }] },
            new() { Title = "Lettuce",         Description = "Fresh romaine lettuce",         Prices = [new PartPriceHistory { Price = 0.25m }] },
            new() { Title = "Tomato",          Description = "Sliced ripe tomato",            Prices = [new PartPriceHistory { Price = 0.30m }] },
            new() { Title = "Onion",           Description = "Sliced red onion",              Prices = [new PartPriceHistory { Price = 0.20m }] },
            new() { Title = "Pickles",         Description = "Dill pickle slices",            Prices = [new PartPriceHistory { Price = 0.25m }] },
            new() { Title = "Bacon",           Description = "Crispy smoked bacon strips",    Prices = [new PartPriceHistory { Price = 1.50m }] },
            new() { Title = "Ham",             Description = "Sliced smoked ham",             Prices = [new PartPriceHistory { Price = 1.25m }] },
            new() { Title = "Turkey",          Description = "Sliced roasted turkey",         Prices = [new PartPriceHistory { Price = 1.50m }] },
            new() { Title = "Mayonnaise",      Description = "Classic mayo",                  Prices = [new PartPriceHistory { Price = 0.15m }] },
            new() { Title = "Ketchup",         Description = "Tomato ketchup",                Prices = [new PartPriceHistory { Price = 0.15m }] },
            new() { Title = "Mustard Sauce",   Description = "Yellow mustard",                Prices = [new PartPriceHistory { Price = 0.15m }] },
            new() { Title = "Extra Shot",      Description = "Additional espresso shot",      Prices = [new PartPriceHistory { Price = 0.80m }] },
            new() { Title = "Oat Milk",        Description = "Barista oat milk",              Prices = [new PartPriceHistory { Price = 0.60m }] },
            new() { Title = "Soy Milk",        Description = "Organic soy milk",              Prices = [new PartPriceHistory { Price = 0.50m }] },
            new() { Title = "Whipped Cream",   Description = "Fresh whipped cream topping",  Prices = [new PartPriceHistory { Price = 0.50m }] },
            new() { Title = "Chocolate Sauce", Description = "Dark chocolate drizzle",        Prices = [new PartPriceHistory { Price = 0.40m }] },
            new() { Title = "Caramel Syrup",   Description = "Caramel flavored syrup",        Prices = [new PartPriceHistory { Price = 0.45m }] },
            new() { Title = "Avocado",         Description = "Fresh sliced avocado",          Prices = [new PartPriceHistory { Price = 1.50m }] },
            new() { Title = "Jalapeños",       Description = "Pickled jalapeño slices",       Prices = [new PartPriceHistory { Price = 0.35m }] },
            new() { Title = "Sriracha",        Description = "Spicy sriracha sauce",          Prices = [new PartPriceHistory { Price = 0.20m }] },
            // Non-food items
            new() { Title = "Cutlery",         Description = "Knife, fork and spoon set",     Prices = [new PartPriceHistory { Price = 0m }], IsGlobalAddition = true },
            new() { Title = "Napkins",         Description = "Paper napkins",                  Prices = [new PartPriceHistory { Price = 0m }], IsGlobalAddition = true },
            new() { Title = "Chopsticks",      Description = "Disposable wooden chopsticks",   Prices = [new PartPriceHistory { Price = 0m }], IsGlobalAddition = true },
            new() { Title = "Straw",           Description = "Paper drinking straw",           Prices = [new PartPriceHistory { Price = 0m }], IsGlobalAddition = true },
            new() { Title = "Extra Plate",     Description = "Additional serving plate",       Prices = [new PartPriceHistory { Price = 0.25m }] },
            new() { Title = "Takeaway Box",    Description = "Cardboard takeaway container",   Prices = [new PartPriceHistory { Price = 0.50m }] },
        };

        foreach (var part in parts)
            await partService.Save(part);
        await partService.SaveChanges();

        return parts;
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

    private async Task<IList<Product>> SeedProducts(IList<Category> categories, IList<Allergen> allergens, IList<Part> parts)
    {
        var f = new Faker("en");
        string[] adjectives = ["Classic", "Spicy", "Smoky", "Crispy", "Golden", "Grilled", "Fresh", "Loaded", "Signature", "Double"];
        string[] nouns = ["Burger", "Sandwich", "Wrap", "Bowl", "Salad", "Pizza", "Pasta", "Toast", "Roll", "Plate"];

        var products = new List<Product>(1000);

        for (int i = 0; i < 1000; i++)
        {
            var price = Math.Round(f.Random.Decimal(3.50m, 24.99m), 2);
            var product = new Product
            {
                Title = $"{f.PickRandom(adjectives)} {f.PickRandom(nouns)} #{i + 1}",
                Description = f.Lorem.Sentence(),
                Prices = [new ProductPriceHistory { Price = price }],
                Categories = f.PickRandom(categories, f.Random.Int(1, 3))
                    .DistinctBy(c => c.Id)
                    .Select(c => new ProductCategory { CategoryId = c.Id })
                    .ToList(),
                Allergens = f.Random.Bool(0.75f)
                    ? f.PickRandom(allergens, f.Random.Int(1, Math.Min(4, allergens.Count)))
                        .DistinctBy(a => a.Id)
                        .Select(a => new ProductAllergen { AllergenId = a.Id })
                        .ToList()
                    : null,
                Parts = f.Random.Bool(0.8f)
                    ? f.PickRandom(parts, f.Random.Int(2, Math.Min(6, parts.Count)))
                        .DistinctBy(p => p.Id)
                        .Select(p => new ProductPart { PartId = p.Id, Quantity = f.Random.Decimal(0.5m, 5m), IsOmitable = f.Random.Bool(0.4f) })
                        .ToList()
                    : null,
                AllowAdditions = f.Random.Bool(0.9f),
                AllowedPartAdditions = f.Random.Bool(0.5f)
                    ? f.PickRandom(parts, f.Random.Int(1, Math.Min(3, parts.Count)))
                        .DistinctBy(p => p.Id)
                        .Select(p => new ProductAllowedPartAddition { PartId = p.Id })
                        .ToList()
                    : null,
            };

            products.Add(product);
            await productService.Save(product);
            if ((i + 1) % BatchSize == 0)
                await productService.SaveChanges();
        }
        await productService.SaveChanges();

        return products;
    }

    public async Task<IList<Order>> SeedOrders(IList<Party> customers, IList<Product> products, IList<Allergen> allergens, IList<Part> parts)
    {
        var f = new Faker("en");
        var statuses = Enum.GetValues<OrderStatus>();
        var orders = new List<Order>(10000);

        for (int i = 0; i < 10000; i++)
        {
            var customer = f.PickRandom(customers);
            var selectedProducts = f.PickRandom(products, f.Random.Int(1, 5)).DistinctBy(p => p.Id).ToList();

            var orderLines = selectedProducts.Select((product, idx) =>
            {
                var unitPrice = product.Prices?.FirstOrDefault()?.Price ?? 0m;
                var quantity = f.Random.Decimal(1m, 3m);

                List<OrderLinePartAddition>? partAdditions = null;
                if (f.Random.Bool(0.4f))
                {
                    partAdditions = f.PickRandom(parts, f.Random.Int(1, Math.Min(3, parts.Count)))
                        .DistinctBy(p => p.Id)
                        .Select(p => new OrderLinePartAddition { PartId = p.Id, Price = p.Prices?.FirstOrDefault()?.Price ?? 0m })
                        .ToList();
                }

                List<OrderLinePartOmission>? partOmissions = null;
                if (product.Parts?.Count > 0 && f.Random.Bool(0.3f))
                {
                    partOmissions = f.PickRandom(parts, f.Random.Int(1, Math.Min(2, parts.Count)))
                        .DistinctBy(p => p.Id)
                        .Select(p => new OrderLinePartOmission { PartId = p.Id, Price = p.Prices?.FirstOrDefault()?.Price ?? 0m })
                        .ToList();
                }

                return new OrderLine
                {
                    ProductId      = product.Id,
                    Quantity       = quantity,
                    UnitPrice      = unitPrice,
                    SortOrder      = idx,
                    PartAdditions  = partAdditions,
                    PartOmissions = partOmissions
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
