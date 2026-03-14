using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Services.Abstractions;
using Webshop.Models.Entities.Clients.Customers;
using Webshop.Models.Entities.Clients.Organizations;
using Xunit;

namespace Webshop.API.Tests.Clients;

[Collection("Integration")]
public class CustomerServiceTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    private IEntityService<Customer, CustomerSearchObject, CustomerSortBy, CustomerIncludes> GetService(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<IEntityService<Customer, CustomerSearchObject, CustomerSortBy, CustomerIncludes>>();

    private IEntityService<Organization> GetOrgService(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<IEntityService<Organization>>();

    [Fact]
    public async Task Int_Id_Is_Auto_Generated()
    {
        using var scope = fixture.CreateScope();
        var service = GetService(scope);

        var customer = new Customer { GivenName = "AutoId", FamilyName = "User" };
        Assert.Equal(0, customer.Id);

        await service.Save(customer);
        await service.SaveChanges();

        Assert.True(customer.Id > 0);
    }

    [Fact]
    public async Task Filter_By_Organization()
    {
        using var scope = fixture.CreateScope();
        var orgService = GetOrgService(scope);
        var service = GetService(scope);

        var org = new Organization { Title = "Test Org" };
        await orgService.Save(org);
        await orgService.SaveChanges();

        var c1 = new Customer
        {
            GivenName = "Org",
            FamilyName = "Member",
            Organizations = [new CustomerOrganization { OrganizationId = org.Id }]
        };
        var c2 = new Customer { GivenName = "Indie", FamilyName = "Customer" };
        await service.Save(c1);
        await service.Save(c2);
        await service.SaveChanges();

        var results = await service.List([new CustomerSearchObject { OrganizationId = [org.Id] }], [], CustomerIncludes.Organizations);
        Assert.All(results, c => Assert.Contains(c.Organizations!, co => co.OrganizationId == org.Id));
    }

    [Fact]
    public async Task Can_Retrieve_With_Organization_Included()
    {
        using var scope = fixture.CreateScope();
        var orgService = GetOrgService(scope);
        var service = GetService(scope);

        var org = new Organization { Title = "Inc Org" };
        await orgService.Save(org);
        await orgService.SaveChanges();

        var customer = new Customer
        {
            GivenName = "Inc",
            FamilyName = "Member",
            Organizations = [new CustomerOrganization { OrganizationId = org.Id }]
        };
        await service.Save(customer);
        await service.SaveChanges();

        var retrieved = await service.Details(customer.Id);
        Assert.NotNull(retrieved);
        Assert.NotNull(retrieved.Organizations);
        Assert.Contains(retrieved.Organizations!, co => co.Organization != null && co.Organization.Title == "Inc Org");
    }
}
