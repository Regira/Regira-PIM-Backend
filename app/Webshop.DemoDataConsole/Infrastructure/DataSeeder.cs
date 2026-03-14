using Bogus;
using Regira.Entities.Services.Abstractions;
using Webshop.Models.Entities.Catalog.Allergens;
using Webshop.Models.Entities.Catalog.Categories;
using Webshop.Models.Entities.Catalog.Parts;
using Webshop.Models.Entities.Catalog.Products;
using Webshop.Models.Entities.Clients.Addresses;
using Webshop.Models.Entities.Clients.ContactData;
using Webshop.Models.Entities.Clients.Customers;
using Webshop.Models.Entities.Clients.Organizations;
using Webshop.Models.Entities.Orders;

namespace Webshop.DemoDataConsole.Infrastructure;

public class DataSeeder(IEntityService<Category> categoryService, IEntityService<Product> productService, IEntityService<Allergen> allergenService, IEntityService<Part> partService,
    IEntityService<Organization> organizationService, IEntityService<Customer> customerService, IEntityService<Order> orderService)
{
    private const int BatchSize = 100;

    public async Task SeedAsync()
    {
        var organizations = await SeedOrganization();
        var customers = await SeedCustomer(organizations);
        var allergens = await SeedAllergens();
        var parts = await SeedParts();
        var categories = await SeedCategories();
        var products = await SeedProducts(categories, allergens, parts);

        await SeedOrders(customers, products, allergens, parts);
    }

    public async Task<IList<Organization>> SeedOrganization()
    {
        var faker = new Faker<Organization>("en")
            .RuleFor(o => o.Title, f => f.Company.CompanyName())
            .RuleFor(o => o.Address, f => f.Address.FullAddress())
            .RuleFor(o => o.Phone, f => f.Phone.PhoneNumber("##########"))
            .RuleFor(o => o.Email, f => f.Internet.Email());

        var organizations = faker.Generate(20);

        foreach (var org in organizations)
            await organizationService.Save(org);
        await organizationService.SaveChanges();

        return organizations;
    }

    public async Task<IList<Customer>> SeedCustomer(IList<Organization> organizations)
    {
        var faker = new Faker<Customer>("en")
            .RuleFor(c => c.GivenName, f => f.Name.FirstName())
            .RuleFor(c => c.FamilyName, f => f.Name.LastName());

        var customers = faker.Generate(1000);

        for (int i = 0; i < customers.Count; i++)
        {
            var customer = customers[i];
            var f = new Faker("en");

            customer.Addresses = [
                new CustomerAddress
                {
                    Title = "Home",
                    Street = f.Address.StreetName(),
                    Number = f.Random.Int(1, 200).ToString(),
                    PostalCode = f.Address.ZipCode(),
                    City = f.Address.City(),
                    CountryCode = "BE",
                    SortOrder = 0
                }
            ];

            customer.ContactData = [
                new CustomerContactData
                {
                    Title = "Email",
                    Value = f.Internet.Email(customer.GivenName, customer.FamilyName),
                    DataType = ContactDataTypes.Email,
                    SortOrder = 0
                },
                new CustomerContactData
                {
                    Title = "Phone",
                    Value = f.Phone.PhoneNumber(),
                    DataType = ContactDataTypes.Phone,
                    SortOrder = 1
                }
            ];

            var orgCount = f.Random.Int(0, 2);
            if (orgCount > 0 && organizations.Count > 0)
            {
                customer.Organizations = f.PickRandom(organizations, Math.Min(orgCount, organizations.Count))
                    .DistinctBy(o => o.Id)
                    .Select(o => new CustomerOrganization { OrganizationId = o.Id })
                    .ToList();
            }

            await customerService.Save(customer);
            if ((i + 1) % BatchSize == 0)
                await customerService.SaveChanges();
        }
        await customerService.SaveChanges();

        return customers;
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
                        .Select(p => new ProductPart { PartId = p.Id, Quantity = f.Random.Decimal(0.5m, 5m) })
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

    public async Task<IList<Order>> SeedOrders(IList<Customer> customers, IList<Product> products, IList<Allergen> allergens, IList<Part> parts)
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
