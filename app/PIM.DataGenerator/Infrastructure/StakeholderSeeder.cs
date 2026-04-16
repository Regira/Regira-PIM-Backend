using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PIM.Data;
using PIM.Models.Stakeholders.ContactData;
using PIM.Models.Stakeholders.Parties;
using Regira.Entities.Services.Abstractions;
using Person = PIM.Models.Stakeholders.Parties.Person;

namespace PIM.DataGenerator.Infrastructure;

public class StakeholderSeeder(PimDbContext dbContext, IEntityService<Party> partyService, IEntityService<RelationshipType> relationshipTypeService, ILogger<StakeholderSeeder> logger)
{
    private const int BatchSize = 1000;

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
            {
                logger.LogInformation($"Saving Parties batch {i + 1}...");
                await partyService.SaveChanges();
            }
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
            new() { Code = "UNIT", Title = "Unit",           Description = "Organization is a unit of another organization" },
            new() { Code = "CON",  Title = "Contact",        Description = "Person is a contact for the organization" },
            new() { Code = "ASC",  Title = "Associate",      Description = "Person is an associate of the organization" },
            new() { Code = "OWN",  Title = "Owner",          Description = "Person is an owner of the organization" },
            new() { Code = "MGR",  Title = "Manager",        Description = "Person manages the organization" },
            new() { Code = "ADM",  Title = "Administration", Description = "Person handles administration for the organization" },
            new() { Code = "SUP",  Title = "Support",        Description = "Person provides support for the organization" },
            new() { Code = "FRL",  Title = "Freelance",      Description = "Person works as a freelancer for the organization" },
        };

        logger.LogInformation("Seeding relationship types...");
        foreach (var type in types)
            await relationshipTypeService.Save(type);
        await relationshipTypeService.SaveChanges();

        return types;
    }

    public async Task SeedRelationships(IList<Party> parties, IList<RelationshipType> relationshipTypes)
    {
        if (await dbContext.Set<PartyRelationship>().AnyAsync())
        {
            logger.LogInformation("Relationships already exist, skipping seeding relationships.");
            return;
        }

        var f = new Faker();
        var organizations = parties.OfType<Organization>().ToList();
        var persons = parties.OfType<Person>().ToList();

        if (organizations.Count == 0 || persons.Count == 0)
            return;

        var unitType = relationshipTypes.First(t => t.Code == "UNIT");
        var contactType = relationshipTypes.First(t => t.Code == "CON");
        var personRelTypes = relationshipTypes
            .Where(t => new[] { "ASC", "OWN", "CON", "MGR", "ADM", "SUP", "FRL" }.Contains(t.Code))
            .ToList();

        // Shuffle orgs and split into 3 hierarchy levels
        var shuffled = f.Random.Shuffle(organizations).ToList();
        int third = Math.Max(1, shuffled.Count / 3);
        var level1 = shuffled.Take(third).ToList();
        var level2 = shuffled.Skip(third).Take(third).ToList();
        var level3 = shuffled.Skip(2 * third).ToList();

        // Build Org → Org Unit map (parentId → list of child org IDs)
        var unitChildMap = new Dictionary<int, List<int>>();
        foreach (var org in level2)
        {
            var parent = f.PickRandom(level1);
            if (!unitChildMap.TryGetValue(parent.Id, out var children))
                unitChildMap[parent.Id] = children = [];
            children.Add(org.Id);
        }
        foreach (var org in level3)
        {
            var parent = f.PickRandom(level2);
            if (!unitChildMap.TryGetValue(parent.Id, out var children))
                unitChildMap[parent.Id] = children = [];
            children.Add(org.Id);
        }

        int count = 0;
        logger.LogInformation("Seeding stakeholder relationships...");
        foreach (var org in organizations)
        {
            var loaded = await partyService.Details(org.Id);
            if (loaded == null)
                continue;

            var childRels = new List<PartyRelationship>();

            // Org → Org Unit relationships
            if (unitChildMap.TryGetValue(org.Id, out var childOrgIds))
            {
                childRels.AddRange(childOrgIds.Select(childId =>
                    new PartyRelationship { ChildId = childId, RelationshipTypeId = unitType.Id }));
            }

            // Org → Person relationships (Shareholder, Owner, Contact, Sales)
            if (f.Random.Bool(0.8f))
            {
                var selectedPersons = f.PickRandom(persons, f.Random.Int(2, Math.Min(8, persons.Count)))
                    .DistinctBy(p => p.Id)
                    .ToList();

                foreach (var person in selectedPersons)
                {
                    // Weight towards Contact as the default type
                    var relType = f.Random.Bool(0.4f) ? contactType : f.PickRandom(personRelTypes);
                    var contactData = new List<PartyRelationshipContactDetails>
                    {
                        new() { Title = "Work Email", Value = f.Internet.Email(person.Title), DataType = ContactDataTypes.Email, SortOrder = 0 },
                    };
                    if (f.Random.Bool(0.6f))
                        contactData.Add(new() { Title = "Work Phone", Value = f.Phone.PhoneNumber(), DataType = ContactDataTypes.Phone, SortOrder = 1 });

                    childRels.Add(new PartyRelationship
                    {
                        ChildId = person.Id,
                        RelationshipTypeId = relType.Id,
                        ContactData = contactData
                    });
                }
            }

            if (childRels.Count == 0)
                continue;

            loaded.ChildRelationships = childRels;
            await partyService.Save(loaded);
            count++;
            if (count % BatchSize == 0)
            {
                logger.LogInformation($"Saving Relations batch {count + 1}...");
                await partyService.SaveChanges();
            }
        }
        await partyService.SaveChanges();
    }
}
