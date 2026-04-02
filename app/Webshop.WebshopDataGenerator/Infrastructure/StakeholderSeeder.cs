using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Regira.Entities.Services.Abstractions;
using Webshop.Data;
using Webshop.Models.Stakeholders.ContactData;
using Webshop.Models.Stakeholders.Parties;
using Webshop.Models.Stakeholders.Parties.Relations;
using Person = Webshop.Models.Stakeholders.Parties.Person;

namespace Webshop.WebshopDataGenerator.Infrastructure;

public class StakeholderSeeder(WebshopDbContext dbContext, IEntityService<Party> partyService, IEntityService<RelationshipType> relationshipTypeService, ILogger<StakeholderSeeder> logger)
{
    private const int BatchSize = 100;

    public async Task<IList<Party>> SeedAsync()
    {
        var parties = await SeedParties();
        var relationshipTypes = await SeedRelationshipTypes();
        await SeedRelationships(parties, relationshipTypes);
        return parties;
    }

    public async Task<IList<Party>> SeedParties()
    {
        var parties = await partyService.List();

        if (parties.Any())
        {
            logger.LogInformation("Parties already exist, skipping seeding parties.");
            return parties;
        }

        parties = new List<Party>(1000);

        // Save party first (without children) to get the generated Id.
        // EF Core 8's MERGE optimization violates the FK constraint when parent
        // and children are inserted in the same batch, so we flush in two steps.
        logger.LogInformation("Seeding parties...");
        for (int i = 0; i < 1000; i++)
        {
            var f = new Faker();

            Party party = f.Random.Bool(0.7f)
                ? new Person { GivenName = f.Name.FirstName(), FamilyName = f.Name.LastName() }
                : new Organization { Name = f.Company.CompanyName() };

            parties.Add(party);
            await partyService.Save(party);
        }
        await partyService.SaveChanges();

        // Now attach children — party.Id is set, so FK will resolve correctly.
        logger.LogInformation($"Seeding addresses and contact data for {parties.Count} parties...");
        for (int i = 0; i < parties.Count; i++)
        {
            var party = parties[i];
            var f = new Faker();

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

            var contactData = new List<PartyContactDetails>
            {
                new() { Title = "Email", Value = f.Internet.Email(party.Title), DataType = ContactDataTypes.Email, SortOrder = 0 },
                new() { Title = "Phone", Value = f.Phone.PhoneNumber(),         DataType = ContactDataTypes.Phone, SortOrder = 1 },
            };
            if (party is Organization && f.Random.Bool(0.8f))
                contactData.Add(new() { Title = "Website", Value = f.Internet.Url(), DataType = ContactDataTypes.Website, SortOrder = contactData.Count });
            if (f.Random.Bool(0.5f))
                contactData.Add(new() { Title = "Mobile", Value = f.Phone.PhoneNumber(), DataType = ContactDataTypes.Phone, SortOrder = contactData.Count });

            party.Addresses = addresses;
            party.ContactData = contactData;
            await partyService.Save(party);

            if (i % BatchSize == 0)
                await partyService.SaveChanges();
        }
        await partyService.SaveChanges();

        return parties;
    }

    public async Task<IList<RelationshipType>> SeedRelationshipTypes()
    {
        var types = await relationshipTypeService.List();

        if (types.Any())
        {
            logger.LogInformation("Relationship types already exist, skipping seeding relationship types.");
            return types;
        }

        types = new List<RelationshipType>
        {
            new() { Code = "EMP",  Title = "Employee",       Description = "Person is employed by an organization" },
            new() { Code = "CUST", Title = "Customer",       Description = "Person is a customer of the organization" },
            new() { Code = "SUP",  Title = "Supplier",       Description = "Organization is a supplier" },
            new() { Code = "MBR",  Title = "Member",         Description = "Person is a member of the organization" },
            new() { Code = "REP",  Title = "Representative", Description = "Person represents the organization" },
            new() { Code = "CON",  Title = "Contact",        Description = "Person is a contact for the organization" },
        };

        logger.LogInformation("Seeding relationship types...");
        foreach (var type in types)
            await relationshipTypeService.Save(type);
        await relationshipTypeService.SaveChanges();

        return types;
    }

    public async Task SeedRelationships(IList<Party> parties, IList<RelationshipType> relationshipTypes)
    {
        if(await dbContext.Set<PartyRelationship>().AnyAsync())
        {
            logger.LogInformation("Relationships already exist, skipping seeding relationships.");
            return;
        }

        var f = new Faker();
        var organizations = parties.OfType<Organization>().ToList();
        var persons = parties.OfType<Person>().ToList();

        if (organizations.Count == 0 || persons.Count == 0)
            return;

        int count = 0;
        logger.LogInformation("Seeding stakeholder relationships...");
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
}
