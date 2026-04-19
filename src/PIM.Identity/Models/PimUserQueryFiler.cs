using Regira.Entities.EFcore.QueryBuilders.Abstractions;

namespace PIM.Identity.Models;

/// <summary>
/// A query filter for <see cref="PimIdentityUser"/> that filters by UserName.<br />
/// The internal DbContext User entity is of type PimIdentityUser, so we need to implement for that type
/// </summary>
public class PimUserQueryFiler : IFilteredQueryBuilder<PimIdentityUser, string, PimUserSearchObject>
{
    public IQueryable<PimIdentityUser> Build(IQueryable<PimIdentityUser> query, PimUserSearchObject? so)
    {
        if (!string.IsNullOrEmpty(so?.UserName))
            query = query.Where(x => x.UserName == so.UserName);
        return query;
    }
}