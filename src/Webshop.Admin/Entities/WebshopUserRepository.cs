using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Regira.DAL.Paging;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Regira.Utilities;
using Webshop.Identity.Data;
using Webshop.Identity.Models;

namespace Webshop.Admin.Entities;

public class WebshopUserRepository(WebshopAccountsDbContext dbContext, UserManager<WebshopUser> userManager, IEnumerable<IFilteredQueryBuilder<WebshopUser, string, WebshopUserSearchObject>> queryFilters, IMapper mapper)
    : IEntityRepository<WebshopUserEntity, string, WebshopUserSearchObject, EntitySortBy, WebshopUserIncludes>
{
    protected WebshopAccountsDbContext DbContext => dbContext;

    public async Task<WebshopUserEntity?> Details(string id)
    {
        var item = await GetItem(id);
        return mapper.Map<WebshopUserEntity>(item!);
    }
    public async Task<IList<WebshopUserEntity>> List(IList<WebshopUserSearchObject?> searchObjects, IList<EntitySortBy> sortBy, WebshopUserIncludes? includes = null, PagingInfo? pagingInfo = null)
    {
        IQueryable<WebshopUser> query = Query(dbContext.Users, searchObjects, includes, pagingInfo);
        var items = await query
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync();
        return mapper.Map<List<WebshopUserEntity>>(items);
    }
    public Task<IList<WebshopUserEntity>> List(WebshopUserSearchObject? so = null, PagingInfo? pagingInfo = null)
        => List([so], [], null, pagingInfo);


    public Task<IList<WebshopUserEntity>> List(object? so = null, PagingInfo? pagingInfo = null)
        => List([Convert(so)], [], null, pagingInfo);
    public Task<long> Count(IList<WebshopUserSearchObject?> searchObjects)
    {
        var query = Filter(dbContext.Users, searchObjects.Select(Convert).ToList());
        return query.LongCountAsync();
    }
    public Task<long> Count(object? so)
        => Count([Convert(so)]);
    public Task<long> Count(WebshopUserSearchObject? so)
        => Count([so]);

    public Task<WebshopUser?> GetItem(string id)
    {
        return AddIncludes(dbContext.Users, WebshopUserIncludes.All)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(x => x.Id == id);
    }
    public IQueryable<WebshopUser> Filter(IQueryable<WebshopUser> query, WebshopUserSearchObject? so)
    {
        foreach (var filter in queryFilters)
        {
            query = filter.Build(query, so);
        }

        return query;
    }
    public IQueryable<WebshopUser> Filter(IQueryable<WebshopUser> query, IList<WebshopUserSearchObject?> searchObjects)
        => searchObjects.Aggregate((IQueryable<WebshopUser>?)null, (r, so) => r == null ? Filter(query, so) : r.Union(Filter(query, so))) ?? query;
    public IQueryable<WebshopUser> AddIncludes(IQueryable<WebshopUser> query, WebshopUserIncludes? includes)
    {
        if (includes != null)
        {
            if (includes.Value.HasFlag(WebshopUserIncludes.UserClaims))
            {
                query = query.Include(x => x.UserClaims);
            }
        }

        return query;
    }
    public virtual IQueryable<WebshopUser> Query(IQueryable<WebshopUser> query, IList<WebshopUserSearchObject?> searchObjects, WebshopUserIncludes? includes, PagingInfo? pagingInfo)
    {
        var filteredQuery = Filter(query, searchObjects);
        var sortedQuery = filteredQuery.OrderBy(x => x.UserName);
        var pagedQuery = sortedQuery.PageQuery(pagingInfo);
        var includingQuery = AddIncludes(pagedQuery, includes);

        return includingQuery;
    }


    public async Task Add(WebshopUserEntity model)
    {
        PrepareItem(model, null);
        var item = mapper.Map<WebshopUser>(model);
        var result = string.IsNullOrWhiteSpace(item.NewPassword)
            ? await userManager.CreateAsync(item)
            : await userManager.CreateAsync(item, item.NewPassword);
        if (result.Succeeded)
        {
            await Modify(model, item);
        }
    }
    public async Task<WebshopUserEntity?> Modify(WebshopUserEntity model)
    {
        var original = await GetItem(model.Id);
        if (original != null)
        {
            PrepareItem(model, original);
            await Modify(model, original);
            await UpdateUser(model, original);

            return model;
        }

        return null;
    }
    public async Task Save(WebshopUserEntity model)
    {
        var original = await GetItem(model.Id);
        if (original != null)
        {
            PrepareItem(model, original);
            await Modify(model, original);
            await UpdateUser(model, original);
        }
        else
        {
            await Add(model);
        }
    }

    public async Task Remove(WebshopUserEntity item)
    {
        var user = await userManager.FindByIdAsync(item.Id);
        if (user != null)
        {
            await userManager.DeleteAsync(user);
        }
    }

    public void PrepareItem(WebshopUserEntity model, WebshopUser? original)
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
    public async Task UpdateUser(WebshopUserEntity model, WebshopUser item)
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
    public Task Modify(WebshopUserEntity item, WebshopUser original)
    {
        if (item.UserClaims != null)
        {
            var originalEntity = mapper.Map<WebshopUserEntity>(original);

            dbContext.UpdateRelatedCollection<WebshopUserEntity, IdentityUserClaimEntity, string, string>(item, originalEntity, x => x.UserClaims);
        }

        return Task.CompletedTask;
    }


    public Task<int> SaveChanges(CancellationToken token = default)
        => dbContext.SaveChangesAsync(token);


    protected WebshopUserSearchObject? Convert(object? so)
        => so == null ? null
            : so as WebshopUserSearchObject ?? ObjectUtility.Create<WebshopUserSearchObject>(so);
}