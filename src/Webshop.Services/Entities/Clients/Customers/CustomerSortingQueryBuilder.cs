using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Webshop.Models.Entities.Clients.Customers;

namespace Webshop.Services.Entities.Clients.Customers;

public class CustomerSortingQueryBuilder : ISortedQueryBuilder<Customer, int, CustomerSortBy>
{
    public IQueryable<Customer> SortBy(IQueryable<Customer> query, CustomerSortBy? sortBy = null)
    {
        if (typeof(IOrderedQueryable).IsAssignableFrom(query.Expression.Type) && query is IOrderedQueryable<Customer> sorted)
        {
            return sortBy switch
            {
                CustomerSortBy.Id => sorted.ThenBy(x => x.Id),
                CustomerSortBy.IdDesc => sorted.ThenByDescending(x => x.Id),
                CustomerSortBy.GivenName => sorted.ThenBy(x => x.GivenName),
                CustomerSortBy.GivenNameDesc => sorted.ThenByDescending(x => x.GivenName),
                CustomerSortBy.FamilyName => sorted.ThenBy(x => x.FamilyName),
                CustomerSortBy.FamilyNameDesc => sorted.ThenByDescending(x => x.FamilyName),
                CustomerSortBy.Created => sorted.ThenBy(x => x.Created),
                CustomerSortBy.CreatedDesc => sorted.ThenByDescending(x => x.Created),
                CustomerSortBy.LastModified => sorted.ThenBy(x => x.LastModified),
                CustomerSortBy.LastModifiedDesc => sorted.ThenByDescending(x => x.LastModified),
                _ => sorted.ThenBy(x => x.GivenName).ThenBy(x => x.FamilyName)
            };
        }

        return sortBy switch
        {
            CustomerSortBy.Id => query.OrderBy(x => x.Id),
            CustomerSortBy.IdDesc => query.OrderByDescending(x => x.Id),
            CustomerSortBy.GivenName => query.OrderBy(x => x.GivenName).ThenBy(x => x.FamilyName),
            CustomerSortBy.GivenNameDesc => query.OrderByDescending(x => x.GivenName).ThenByDescending(x => x.FamilyName),
            CustomerSortBy.FamilyName => query.OrderBy(x => x.FamilyName).ThenBy(x => x.GivenName),
            CustomerSortBy.FamilyNameDesc => query.OrderByDescending(x => x.FamilyName).ThenByDescending(x => x.GivenName),
            CustomerSortBy.Created => query.OrderBy(x => x.Created),
            CustomerSortBy.CreatedDesc => query.OrderByDescending(x => x.Created),
            CustomerSortBy.LastModified => query.OrderBy(x => x.LastModified),
            CustomerSortBy.LastModifiedDesc => query.OrderByDescending(x => x.LastModified),
            _ => query.OrderBy(x => x.GivenName).ThenBy(x => x.FamilyName)
        };
    }
}