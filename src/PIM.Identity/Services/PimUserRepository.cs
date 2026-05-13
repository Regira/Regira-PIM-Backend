using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PIM.Identity.Data;
using PIM.Identity.Models;
using Regira.DAL.Paging;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Regira.Utilities;

namespace PIM.Identity.Services;

public class PimUserRepository(AccountsDbContext dbContext, UserManager<PimIdentityUser> userManager, IEnumerable<IFilteredQueryBuilder<PimIdentityUser, string, PimUserSearchObject>> queryFilters, IMapper mapper)
    : IEntityRepository<PimUserEntity, string, PimUserSearchObject, EntitySortBy, PimUserIncludes>
{
    public async Task<PimUserEntity?> Details(string id, CancellationToken token = default)
    {
        var item = await GetItem(id, token);
        return mapper.Map<PimUserEntity>(item!);
    }
    public async Task<IList<PimUserEntity>> List(IList<PimUserSearchObject?> searchObjects, IList<EntitySortBy> sortBy, PimUserIncludes? includes = null, PagingInfo? pagingInfo = null, CancellationToken token = default)
    {
        IQueryable<PimIdentityUser> query = Query(dbContext.Users, searchObjects, includes, pagingInfo);
        var items = await query
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync(token);
        return mapper.Map<List<PimUserEntity>>(items);
    }
    public Task<IList<PimUserEntity>> List(PimUserSearchObject? so = null, PagingInfo? pagingInfo = null, CancellationToken token = default)
        => List([so], [], null, pagingInfo, token);
    public Task<IList<PimUserEntity>> List(object? so = null, PagingInfo? pagingInfo = null, CancellationToken token = default)
        => List([Convert(so)], [], null, pagingInfo, token);

    public Task<long> Count(IList<PimUserSearchObject?> searchObjects, CancellationToken token = default)
    {
        var query = Filter(dbContext.Users, searchObjects.Select(Convert).ToList());
        return query.LongCountAsync(token);
    }
    public Task<long> Count(object? so, CancellationToken token = default)
        => Count([Convert(so)], token);
    public Task<long> Count(PimUserSearchObject? so, CancellationToken token = default)
        => Count([so], token);

    public Task<PimIdentityUser?> GetItem(string id, CancellationToken token = default)
    {
        return AddIncludes(dbContext.Users, PimUserIncludes.All)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(x => x.Id == id, token);
    }
    public IQueryable<PimIdentityUser> Filter(IQueryable<PimIdentityUser> query, PimUserSearchObject? so)
    {
        foreach (var filter in queryFilters)
        {
            query = filter.Build(query, so);
        }

        return query;
    }
    public IQueryable<PimIdentityUser> Filter(IQueryable<PimIdentityUser> query, IList<PimUserSearchObject?> searchObjects)
        => searchObjects.Aggregate((IQueryable<PimIdentityUser>?)null, (r, so) => r == null ? Filter(query, so) : r.Union(Filter(query, so))) ?? query;
    public IQueryable<PimIdentityUser> AddIncludes(IQueryable<PimIdentityUser> query, PimUserIncludes? includes)
    {
        if (includes != null)
        {
            if (includes.Value.HasFlag(PimUserIncludes.UserClaims))
            {
                query = query.Include(x => x.UserClaims);
            }
        }

        return query;
    }
    public virtual IQueryable<PimIdentityUser> Query(IQueryable<PimIdentityUser> query, IList<PimUserSearchObject?> searchObjects, PimUserIncludes? includes, PagingInfo? pagingInfo)
    {
        var filteredQuery = Filter(query, searchObjects);
        var sortedQuery = filteredQuery.OrderBy(x => x.UserName);
        var pagedQuery = sortedQuery.PageQuery(pagingInfo);
        var includingQuery = AddIncludes(pagedQuery, includes);

        return includingQuery;
    }


    public async Task Add(PimUserEntity model, CancellationToken token = default)
    {
        PrepareItem(model, null);
        var item = mapper.Map<PimIdentityUser>(model);
        var result = string.IsNullOrWhiteSpace(model.NewPassword)
            ? await userManager.CreateAsync(item)
            : await userManager.CreateAsync(item, model.NewPassword);
        if (result.Succeeded)
        {
            await Modify(model, item, token);
        }
    }
    public async Task<PimUserEntity?> Modify(PimUserEntity model, CancellationToken token = default)
    {
        var original = await GetItem(model.Id, token);
        if (original != null)
        {
            PrepareItem(model, original);
            await Modify(model, original, token);
            await UpdateUser(model, original, token);

            return model;
        }

        return null;
    }
    public async Task Save(PimUserEntity model, CancellationToken token = default)
    {
        var original = await GetItem(model.Id, token);
        if (original != null)
        {
            PrepareItem(model, original);
            await Modify(model, original, token);
            await UpdateUser(model, original, token);
        }
        else
        {
            await Add(model, token);
        }
    }

    public async Task Remove(PimUserEntity item, CancellationToken token = default)
    {
        var user = await userManager.FindByIdAsync(item.Id);
        if (user != null)
        {
            await userManager.DeleteAsync(user);
        }
    }

    public void PrepareItem(PimUserEntity model, PimIdentityUser? original)
    {
        if (string.IsNullOrWhiteSpace(model.Id))
        {
            model.Id = Guid.NewGuid().ToString();
        }

        if (original != null)
        {
            dbContext.Entry(original).CurrentValues.SetValues(model);
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                original.PasswordHash = userManager.PasswordHasher.HashPassword(original, model.NewPassword);
            }
        }
        if (model.UserClaims?.Any() == true)
        {
            foreach (var claim in model.UserClaims)
            {
                claim.UserId = model.Id;
            }
        }
    }
    public async Task UpdateUser(PimUserEntity model, PimIdentityUser item, CancellationToken token = default)
    {
        var entry = dbContext.Entry(item);
        if (entry.State == EntityState.Detached)
        {
            entry.State = EntityState.Modified;
        }
        var result = await userManager.UpdateAsync(item);
        if (!result.Succeeded)
        {
            throw new Exception(result.Errors.FirstOrDefault()?.Code);
        }
    }
    public Task Modify(PimUserEntity item, PimIdentityUser original, CancellationToken token = default)
    {
        var modified = mapper.Map<PimIdentityUser>(item);
        if (modified.UserClaims != null)
        {
            var originalClaims = original.UserClaims!;
            var claimsToRemove = originalClaims
                .Where(oc => modified.UserClaims.All(c => c.Id != oc.Id))
                .ToArray();
            var claimsToAdd = modified.UserClaims
                .Where(c => originalClaims.All(oc => c.Id != oc.Id))
                .ToArray();
            var claimsToUpdate = originalClaims.Except(claimsToRemove)
                .ToArray();

            if (claimsToRemove.Any())
            {
                dbContext.UserClaims.RemoveRange(claimsToRemove);
            }
            if (claimsToAdd.Any())
            {
                dbContext.UserClaims.AddRange(claimsToAdd);
            }
            if (claimsToUpdate.Any())
            {
                foreach (var claim in claimsToUpdate)
                {
                    var itemClaim = modified.UserClaims.First(c => c.Id == claim.Id);
                    if (itemClaim.ClaimValue != claim.ClaimValue)
                    {
                        claim.ClaimValue = itemClaim.ClaimValue;
                        dbContext.Entry(claim).State = EntityState.Modified;
                    }
                }
            }
        }

        return Task.CompletedTask;
    }


    public Task<int> SaveChanges(CancellationToken token = default)
        => dbContext.SaveChangesAsync(token);


    protected PimUserSearchObject? Convert(object? so)
        => so == null ? null
            : so as PimUserSearchObject ?? ObjectUtility.Create<PimUserSearchObject>(so);
}