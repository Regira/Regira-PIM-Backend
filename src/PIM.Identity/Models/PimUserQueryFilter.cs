using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Keywords.Abstractions;
using Regira.Entities.QueryBuilders.Abstractions;

namespace PIM.Identity.Models;

/// <summary>
/// A query filter for <see cref="PimIdentityUser"/> that filters by UserName, and by <c>?q=</c> on user name or email.<br />
/// The internal DbContext User entity is of type PimIdentityUser, so we need to implement for that type
/// </summary>
public class PimUserQueryFilter(IQKeywordHelper qHelper, UserManager<PimIdentityUser> userManager) : IFilteredQueryBuilder<PimIdentityUser, string, PimUserSearchObject>
{
    public IQueryable<PimIdentityUser> Build(IQueryable<PimIdentityUser> query, PimUserSearchObject? so)
    {
        if (!string.IsNullOrEmpty(so?.UserName))
            query = query.Where(x => x.UserName == so.UserName);
        // ?q= as the other PIM lists read it: every word must match the user name or the email.
        // Each word goes through the UserManager's normalizer, the one that wrote NormalizedUserName/NormalizedEmail.
        foreach (var keyword in qHelper.Parse(so?.Q))
        {
            var name = $"%{userManager.NormalizeName(keyword.Trimmed)}%";
            var email = $"%{userManager.NormalizeEmail(keyword.Trimmed)}%";
            query = query.Where(x => EF.Functions.Like(x.NormalizedUserName!, name) || EF.Functions.Like(x.NormalizedEmail!, email));
        }
        return query;
    }
}