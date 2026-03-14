using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Keywords.Abstractions;
using Webshop.Models.Entities.Clients.Customers;

namespace Webshop.Services.Entities.Clients.Customers;

public class CustomerQueryBuilder(IQKeywordHelper qHelper) : FilteredQueryBuilderBase<Customer, int, CustomerSearchObject>
{
    public override IQueryable<Customer> Build(IQueryable<Customer> query, CustomerSearchObject? so)
    {
        if (so == null)
            return query;

        if (so.OrganizationId?.Any() == true)
            query = query.Where(x => x.Organizations!.Any(co => so.OrganizationId.Contains(co.OrganizationId)));
        
        return query;
    }
}