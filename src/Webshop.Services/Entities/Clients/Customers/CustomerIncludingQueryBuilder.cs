using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Webshop.Models.Entities.Clients.Customers;

namespace Webshop.Services.Entities.Clients.Customers;

public class CustomerIncludingQueryBuilder : IIncludableQueryBuilder<Customer, int, CustomerIncludes>
{
    public IQueryable<Customer> AddIncludes(IQueryable<Customer> query, CustomerIncludes? includes = null)
    {
        if (includes == null || includes == CustomerIncludes.None)
            return query;

        if (includes.Value.HasFlag(CustomerIncludes.Organizations))
            query = query.Include(x => x.Organizations!).ThenInclude(co => co.Organization);
        if (includes.Value.HasFlag(CustomerIncludes.ContactData))
            query = query.Include(x => x.ContactData!);
        if (includes.Value.HasFlag(CustomerIncludes.Addresses))
            query = query.Include(x => x.Addresses!);

        return query;
    }
}