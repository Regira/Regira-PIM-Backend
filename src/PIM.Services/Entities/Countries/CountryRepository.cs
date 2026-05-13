using PIM.Core.Abstractions;
using Regira.DAL.Paging;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Regira.Globalization.Utilities;
using Regira.Utilities;
using CountryEntity = PIM.Models.Countries.Country;

namespace PIM.Services.Entities.Countries;

public class CountryRepository(ICultureContext cultureContext) : IEntityService<CountryEntity, string, SearchObject<string>>
{
    public Task<CountryEntity?> Details(string id, CancellationToken token = default)
    {
        var country = CountryUtility.GetCountry(id);
        var item = country != null ? Convert(country) : null;
        return Task.FromResult(item);
    }

    public Task<IList<CountryEntity>> List(SearchObject<string>? so = null, PagingInfo? pagingInfo = null, CancellationToken token = default)
    {
        var query = CountryUtility.GetCountries();
        if (!string.IsNullOrWhiteSpace(so?.Q))
        {
            query = query
                .Select(x => new { country = x, weight = CalculateWeight(x, so) })
                .Where(x => x.weight > 0)
                .OrderByDescending(x => x.weight)
                .ThenBy(x => x.country.Title)
                .Select(x => x.country);
        }
        else
        {
            query = query
                .OrderBy(x => x.Iso2Code == cultureContext.CountryCode ? 0 : 1)
                .ThenBy(x => x.Title);
        }

        var itemQuery = query.Select(x => Convert(x));
        if (pagingInfo?.PageSize > 0)
        {
            itemQuery = itemQuery.PageItems(pagingInfo.PageSize, pagingInfo.Page - 1);
        }
        var items = itemQuery.ToList();
        return Task.FromResult(items as IList<CountryEntity>);
    }


    Task<IList<CountryEntity>> IEntityReadService<CountryEntity, string>.List(object? so, PagingInfo? pagingInfo, CancellationToken token)
        => List(Convert(so), pagingInfo, token);
    public Task<long> Count(object? so, CancellationToken token = default)
        => Count(Convert(so), token);
    public async Task<long> Count(SearchObject<string>? so, CancellationToken token = default)
        => (await List(so, null, token)).Count;


    public int CalculateWeight(Regira.Globalization.Models.Country item, SearchObject<string>? so)
    {
        var weight = 0;
        if (!string.IsNullOrWhiteSpace(so?.Q))
        {
            // title in current language
            var itemTitle = item.GetName();
            if (item.Iso2Code.Equals(so.Q, StringComparison.InvariantCultureIgnoreCase))
            {
                weight += 20;
            }
            var titles = item.Names.Values;
            if (titles.Any(title => title.Equals(so.Q, StringComparison.InvariantCultureIgnoreCase)))
            {
                weight += 25;
            }
            else if (so.Q.Length > 1 && titles.Any(title => so.Q.Equals(GetInitials(title), StringComparison.InvariantCultureIgnoreCase)))
            {
                weight += 15;
            }
            if (itemTitle.StartsWith(so.Q, StringComparison.InvariantCultureIgnoreCase))
            {
                weight += 20;
            }
            else if (item.Names.Values.Any(title => title.StartsWith(so.Q, StringComparison.InvariantCultureIgnoreCase)))
            {
                weight += 10;
            }
            else if (titles.Any(title => title.Contains(so.Q, StringComparison.InvariantCultureIgnoreCase)))
            {
                weight += 5;
            }
        }
        // Bonus points for default item
        if (weight > 0 && item.Iso2Code.Equals(cultureContext.CountryCode, StringComparison.InvariantCultureIgnoreCase))
        {
            weight += 5;
        }

        return weight;
    }
    public CountryEntity Convert(Regira.Globalization.Models.Country item)
    {
        return new CountryEntity
        {
            Id = item.Iso2Code,
            Code = item.Iso2Code,
            Title = item.GetName(),
            IsDefault = item.Iso2Code == cultureContext.CountryCode
        };
    }
    protected virtual SearchObject<string>? Convert(object? so)
    {
        if (so == null)
        {
            return null;
        }
        return so as SearchObject<string> ?? ObjectUtility.Create<SearchObject<string>>(so);
    }
    static string? GetInitials(string? input)
        => string.IsNullOrWhiteSpace(input) ? input : string.Join("", input.Split(' ').Select(s => s.First()));


    #region NotSupported
    public Task Add(CountryEntity item, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
    public Task<CountryEntity?> Modify(CountryEntity item, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
    public Task Save(CountryEntity item, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
    public Task Remove(CountryEntity item, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
    public Task<int> SaveChanges(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
    #endregion
}