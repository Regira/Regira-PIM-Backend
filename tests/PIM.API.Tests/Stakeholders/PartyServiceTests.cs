using Microsoft.Extensions.DependencyInjection;
using PIM.Core.Constants;
using PIM.Models.Stakeholders.ContactData;
using PIM.Models.Stakeholders.Parties;
using Regira.Entities.Services.Abstractions;
using Xunit;

namespace PIM.API.Tests.Stakeholders;

[Collection("Integration")]
public class PartyServiceTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    private IEntityService<Party, PartySearchObject, PartySortBy, PartyIncludes> GetService(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<IEntityService<Party, PartySearchObject, PartySortBy, PartyIncludes>>();

    [Fact]
    public async Task Can_Create_And_Retrieve_Person()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);

        var person = new Person { GivenName = "Jane", FamilyName = "Doe" };
        await service.Save(person);
        await service.SaveChanges();

        var result = await service.Details(person.Id);

        Assert.NotNull(result);
        Assert.Equal(PartyTypes.Person, result.PartyType);
        Assert.IsType<Person>(result);
        Assert.Equal("Jane", ((Person)result).GivenName);
    }

    [Fact]
    public async Task Can_Create_And_Retrieve_Organization()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);

        var org = new Organization { Name = "Acme Corp" };
        await service.Save(org);
        await service.SaveChanges();

        var result = await service.Details(org.Id);

        Assert.NotNull(result);
        Assert.Equal(PartyTypes.Organization, result.PartyType);
        Assert.IsType<Organization>(result);
        Assert.Equal("Acme Corp", ((Organization)result).Name);
    }

    [Fact]
    public async Task Filter_By_PartyType_Returns_Only_Matching()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);

        var suffix = Guid.NewGuid().ToString("N")[..8];
        await service.Save(new Person { GivenName = "Filter", FamilyName = $"Person-{suffix}" });
        await service.Save(new Organization { Name = $"Filter-Org-{suffix}" });
        await service.SaveChanges();

        var persons = await service.List(new PartySearchObject { PartyType = PartyTypes.Person, Query = suffix });
        var orgs = await service.List(new PartySearchObject { PartyType = PartyTypes.Organization, Query = suffix });

        Assert.All(persons, p => Assert.Equal(PartyTypes.Person, p.PartyType));
        Assert.All(orgs, o => Assert.Equal(PartyTypes.Organization, o.PartyType));
    }

    [Fact]
    public async Task Timestamps_Are_Set_On_Create()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);

        var person = new Person { GivenName = "Timestamp", FamilyName = "Test" };
        await service.Save(person);
        await service.SaveChanges();

        Assert.NotEqual(default, person.Created);
    }

    [Fact]
    public async Task Person_With_ContactData_Is_Saved_And_Included()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);

        var person = new Person
        {
            GivenName = "Contact",
            FamilyName = "Test",
            ContactData = [new PartyContactDetails { Value = "test@example.com", DataType = ContactDataTypes.Email }]
        };
        await service.Save(person);
        await service.SaveChanges();

        var result = await service.Details(person.Id);

        Assert.NotNull(result);
        Assert.NotNull(result.ContactData);
        Assert.Single(result.ContactData);
        Assert.Equal("test@example.com", result.ContactData.First().Value);
    }

    [Fact]
    public async Task List_Returns_Sorted_By_Title()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);

        var suffix = Guid.NewGuid().ToString("N")[..8];
        await service.Save(new Person { GivenName = "Zebra", FamilyName = suffix });
        await service.Save(new Person { GivenName = "Alpha", FamilyName = suffix });
        await service.SaveChanges();

        var results = await service.List([new PartySearchObject { Query = suffix }], [PartySortBy.Title]);

        Assert.Equal(2, results.Count);
        Assert.True(string.Compare(results[0].NormalizedTitle, results[1].NormalizedTitle, StringComparison.Ordinal) <= 0);
    }
}
